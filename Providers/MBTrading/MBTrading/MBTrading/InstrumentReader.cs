#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MBTCOMLib;
using MBTORDERSLib;
using MBTQUOTELib;
using TickZoom.Api;

namespace TickZoom.MBTrading
{
	public enum QuoteType {
		Level1,
		Level2
	}
	/// <summary>
	/// Description of Instrument.
	/// </summary>
	public class InstrumentReader : IMbtQuotesNotify
    {
		private static readonly Log log = Factory.Log.GetLogger(typeof(InstrumentReader));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		public Log orderLog = Factory.Log.GetLogger(typeof(InstrumentReader));
        public int lastChangeTime;
        public object quotesLocker = new object();
        public object level2Locker = new object();
        Level2Collection level2Bids;
        Level2Collection level2Asks; 
        QuoteType quoteType = QuoteType.Level1;
        private MbtOrderClient m_OrderClient = null;
        private MbtQuotes m_Quotes = null;
        private MbtAccount m_account = null;
        private MbtPosition m_position = null;
        private bool advised = false;
        private Thread quoteThread = null;
        private bool cancelThread = false;
        private Receiver receiver;
        TickIO lastTick;
        TickIO tick;
		double previous, current, change;
		TimeStamp tempDateTime;
		int prevTickCount = 0;
		SymbolInfo instrument;
        Dictionary<string,MbtOpenOrder> m_orders = new Dictionary<string,MbtOpenOrder>();
        
        public InstrumentReader(MbtOrderClient orderClient, MbtQuotes quotes, SymbolInfo instrument)
        {
        	m_OrderClient = orderClient;
        	m_Quotes = quotes;
        	this.instrument = instrument;
        	lastChangeTime = Environment.TickCount;
        }
        
        public void Initialize() {
        	tick = Factory.TickUtil.TickIO();
            lastTick = Factory.TickUtil.TickIO();
        	tick.lSymbol = instrument.BinaryIdentifier;
            this.level2Bids = new Level2Collection(this, instrument, enumMarketSide.msBid);
            this.level2Asks = new Level2Collection(this, instrument, enumMarketSide.msAsk);
        }
        
        public void ProcessQuote(ref QUOTERECORD pQuote) {
			tempDateTime = (TimeStamp) pQuote.UTCDateTime;
        	tick.init(tempDateTime, pQuote.dBid, pQuote.dAsk);
        	if( trace) {
        		log.Symbol = instrument.Symbol;
        		log.TimeStamp = tempDateTime;
        		log.Trace("Create QUOTE tick: " + tick);
        	}
        	if( tick.Time.Year > 2000) {
				previous = lastTick.Bid + lastTick.Ask;
				current = tick.Bid + tick.Ask;
				change = previous - current;
				change = change < 0 ? -change : change;
			
				// If changed or at least 1 second has elapsed
				if( change > 0 || Environment.TickCount - prevTickCount > 1000) {
	        		lastTick = tick;
	        		TickBinary binary = new TickBinary();
	        		binary = tick.Extract();
	        		receiver.OnSend( ref binary);
	        		prevTickCount = Environment.TickCount;
				} else {
					// At least send a tick every 1 second.
				}
        	}
        }

        int lastUpdateTick = 0;
        public void ProcessLevel2(LEVEL2RECORD pRec)
        {       	
//        	log.Info(pRec.side + " " + pRec.bstrMMID + " " + pRec.bClosed + " " + pRec.dPrice + " " + (pRec.lSize/instrument.LotSize));
        	switch( pRec.side) {
        		case enumMarketSide.msAsk:
	        		level2Asks.Process(pRec);
        			break;
        		case enumMarketSide.msBid:
	        		level2Bids.Process(pRec);
        			break;
        		default:
        			throw new ApplicationException("Invalid Level II Side");
        	}
        	if( Environment.TickCount > lastUpdateTick + 30) {
        		if( level2Bids.HasChanged || level2Asks.HasChanged ) {
	        		LogChange(TradeSide.None,0,0);
	        	}
			}
        }
        
        public void Connect()
        {
        	Disconnect();
            try
            {
            	cancelThread = false;
	            quoteThread = new Thread(new ThreadStart(AdviseSymbols));
	            quoteThread.IsBackground = true;
	            quoteThread.Name = "AdviseSymbol "+instrument.Symbol;
	            quoteThread.Priority = ThreadPriority.BelowNormal;
	            quoteThread.Start();
            }
            catch (Exception e)
            {
                log.Notice(string.Format("Could not AdviseSymbol {0} - {1}!\n", Instrument.Symbol, e.Message));
            }
        }
        
        private void AdviseSymbols() {
            if (Level2Bids.Count > 0) Level2Bids.Clear();
            if (Level2Asks.Count > 0) Level2Asks.Clear();
            if( QuoteType == QuoteType.Level1) m_Quotes.AdviseSymbol(this, Instrument.Symbol, (int)enumQuoteServiceFlags.qsfLevelOne);
            if( QuoteType == QuoteType.Level2) m_Quotes.AdviseSymbol(this, Instrument.Symbol, (int)enumQuoteServiceFlags.qsfLevelTwo);
            advised = true;
            while(!cancelThread) {
            	Application.DoEvents();
            	Thread.Sleep(5);
    		}           
            m_Quotes.UnadviseSymbol(this, Instrument.Symbol, (int)enumQuoteServiceFlags.qsfLevelTwo);
            m_Quotes.UnadviseSymbol(this, Instrument.Symbol, (int)enumQuoteServiceFlags.qsfLevelOne);
            Level2Bids.Clear();
            Level2Asks.Clear();            
        }
        
        public void Close() {
        	Disconnect();
        }
        
        public void Disconnect()
        {
			cancelThread = true;
			if( quoteThread!=null) {
				if( quoteThread.IsAlive) {
		    		quoteThread.Join();
				}
		    	quoteThread=null;
			}
        }
        
        public void LogChange(byte side, double price, long size) {
    		// Update the last Dom total and time
    		// so we can log any changes if ticks don't
    		// come in timely due to a pause in the market.
    		lastUpdateTick = Environment.TickCount;
    		level2Bids.UpdateTotalSize();
    		level2Asks.UpdateTotalSize();
    		
    		// Now log any change to tick.
    		TimeStamp timeStamp = new TimeStamp();
    		timeStamp.Internal = DateTime.Now.ToUniversalTime().ToOADate();
    		if( LastBid > 0 && LastAsk > 0) {
	    		tick.init(timeStamp, side, price, (int) size,
				             LastBid, LastAsk,
				             level2Bids.DepthSizes,
				             level2Asks.DepthSizes);
		     	if( trace) {
		    		log.Symbol = instrument.Symbol;
		    		log.TimeStamp = tempDateTime;
		    		log.Trace("Created Trade,Quote,DOM tick: " + tick);
		    	}
	        	// Grab a reference since other thread might change it.
	        	TickBinary binary = new TickBinary();
	        	binary = tick.Extract();
				receiver.OnSend( ref binary);
    		}
		}
		
        object signalLock = new Object();
    	int orderStart = 0;
    	long followupSignal = 0;
    	bool orderSubmitted = false;
    	int ignoredTickCount = 0;
	    public void Signal( double _size) {
    		lock(signalLock) {
    			// Sizing now handles on client side
//	    		int size = (int) (_size*Instrument.TradeSize);
    			long size = (long) _size;
    			int elapsed = Environment.TickCount - ignoredTickCount;
		    	if( m_account == null) {
	    			if( ignoredTickCount == 0 || elapsed > 5000) {
		    			orderLog.Notice( "Signal("+size+") ignored. Account not loaded.");
		    			ignoredTickCount = Environment.TickCount;
    				}
		    		return;
	    		}
    			if( orderSubmitted || m_orders.Count > 0 ) {
	    			if( ignoredTickCount == 0 || elapsed > 5000) {
		    			orderLog.Notice( "Signal("+size+") ignored. Orders already pending.");
		    			ignoredTickCount = Environment.TickCount;
	    			} 
	    			return;
	    		}
    			ignoredTickCount = Environment.TickCount;
	    		InternalSignal(size);
    		}
    	}
    	
//    	private void CancelOrders() {
//	    	MbtOpenOrders orders = m_OrderClient.OpenOrders;
//	    	for( int i=0; i< orders.Count; i++) {
//	    		TickConsole.Notice("Order " + i + ": " + Display(orders[i]));
//	    		string bstrRetMsg = null;
//	    		if( m_OrderClient.Cancel( orders[i].OrderNumber, ref bstrRetMsg) == true) {
//		    		TickConsole.Notice("Order " + i + ": Canceled. " + bstrRetMsg);
//	    		} else {
//		    		TickConsole.Notice("Order " + i + ": Cancel Failed: " + bstrRetMsg);
//	    		}
//	    	}
//    	}
    	
	    private void InternalSignal( long size) {
			// Check if already same as desired size	    	
    		int currentSize = GetPositionSize();
    		if( currentSize == size) {
    			return;
    		}
   			int beforeSignal = Environment.TickCount;
	    	orderStart = Environment.TickCount;
	    	orderLog.Notice( "Signal( " + size + "), " + 
	    	                      " position size: " + currentSize + ", " + 
	    	                      " position: " + Display(m_position));
	    	if( m_account.PermedForEquities &&
	    	   (size > 0 && currentSize < 0 ||
	    	    size < 0 && currentSize > 0)) {
	    		SubmitOrder(-currentSize);
	    		followupSignal = size;
	    	} else {
    			SubmitOrder(size - currentSize);
	    	}
    		int elapsed = Environment.TickCount - beforeSignal;
    		orderLog.Notice("InternalSignal() " + elapsed + "ms");
	    }

	    public int GetPositionSize() {
	    	int size = 0;
	    	try {
	    		MbtPosition position = m_position;
		    	if( position != null) {
			    	size += position.PendingBuyShares - position.PendingSellShares +
			    			position.IntradayPosition + position.OvernightPosition;
		    	}
	    	} catch(Exception ex) {
	    		orderLog.Notice( "Exception: " + ex);
	    	}
	    	return size;
	    }

		public void SubmitOrder(long size)
		{
            orderLog.Notice("SubmitOrder("+size+")");
            long lVol = Math.Abs(size);
            int lBuySell = size > 0 ? MBConst.VALUE_BUY : MBConst.VALUE_SELL;
			string sSym = instrument.Symbol;
			double dPrice = size > 0 ? LastAsk : LastBid;
			int	lTIF = MBConst.VALUE_GTC;
            int lOrdType = MBConst.VALUE_MARKET;
			int lVolType = MBConst.VALUE_NORMAL;
			string sRoute = "MBTX";
			string bstrRetMsg = null;
			System.DateTime dDte = new System.DateTime(0);
			string sOrder = String.Format("{0} {1} {2} at {3:c} on {4}",
                  lBuySell == MBConst.VALUE_BUY ? "Buy" : "Sell",
                  lVol, sSym, dPrice, sRoute);
            orderLog.Notice(sOrder);
            bool bSubmit = m_OrderClient.Submit(lBuySell,(int)lVol,sSym,dPrice,0.0,lTIF,0,lOrdType,lVolType,0,m_account,sRoute,"", 0, 0, dDte, dDte, 0, 0, 0, 0, 0, ref bstrRetMsg);
			if( bSubmit )
			{
                orderLog.Notice(
					String.Format("Order submission successful! bstrRetMsg = [{0}]", bstrRetMsg)); // wide string!
				orderSubmitted = true;
			}
			else
			{
                orderLog.Notice(
					String.Format("Order submission failed! bstrRetMsg = [{0}]", bstrRetMsg)); // wide string!
			}
		}

        public void OnCancelPlaced(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnCancelPlaced( " + order.OrderNumber + " )");
	    }
		
        public void OnReplacePlaced(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnReplacePlaced( " + order.OrderNumber + " )");
	    }
		
        public void OnReplaceRejected(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnReplaceRejected( " + order.OrderNumber + " )");
	    }
		
        public void OnCancelRejected(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnCancelRejected( " + order.OrderNumber + " )");
	    }
	    
        public void OnClose(int x) {
			m_account = null;
	    	if( debug) orderLog.Debug( "Account: OnClose( " + x + " )");
	    }

		public void OnSubmit(MbtOpenOrder order) {
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
	    	orderLog.Notice( "OnSubmit( " + order.OrderNumber + " ) " + elapsed + "ms" );
			orderSubmitted = false;
	    }
        
        public void OnAcknowledge(MbtOpenOrder order) {
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
	    	orderLog.Notice( "OnAcknowledge( " + order.OrderNumber + " ) " + elapsed + "ms" );
	    }
        public void OnExecute(MbtOpenOrder order) { 
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
    		orderLog.Notice( "OnExecute( " + order.OrderNumber + " ), " + elapsed + "ms");
	    }
		
        public void OnRemove(MbtOpenOrder order) { 
    		int elapsed = Environment.TickCount - orderStart;
    		orderLog.Notice( "OnRemove( " + order.OrderNumber + " ), " + elapsed + "ms, position: " + Display(m_position));
    		if( followupSignal != 0) {
    			orderLog.Notice("Followup Signal = " + followupSignal );
    			InternalSignal( followupSignal);
    			followupSignal = 0;
    		} else {
	    		orderLog.Notice( "Account Status: " + Display(m_account));
    		}
			m_orders.Remove(order.OrderNumber);
		}
		
        public void OnHistoryAdded(MbtOrderHistory orderhistory) {
	    	SaveHistory(orderhistory);
	    }
		
        public void OnPositionAdded(MbtPosition position) {
	    	m_position = position;
	    	if( debug) orderLog.Debug( "OnPositionAdded( )");
	    }
        public void OnPositionUpdated(MbtPosition position) {
	    	m_position = position;
	    	if( debug) orderLog.Debug( "Account: OnPositionUpdated(  )");
	    }
		
        public void OnBalanceUpdate(MbtAccount account) {
	    	this.m_account = account;
	    	if( debug) orderLog.Debug( "OnBalanceUpdate( " + account.Account + "," + account.Customer + " )");
	    }
		
        public void OnDefaultAccountChanged(MbtAccount account) {
	    	this.m_account = account;
	    	if( debug) orderLog.Debug( "OnDefaultAccountChanged( " + account.Account + "," + account.Customer + " )");
	    }
		
        public void OnAccountUnavailable(MbtAccount account) {
	    	this.m_account = null;
	    	if( debug) orderLog.Debug( "OnAccountUnavailable( " + account.Account + "," + account.Customer + " )");
	    }
		
	    bool firstTime = true;
        public void OnAccountLoaded(MbtAccount account) {
	    	this.m_account = account;
	    	if( firstTime) {
	    		orderLog.Notice( "Starting account = " + Display(m_account));
	    		m_position = m_OrderClient.Positions.Find(m_account, instrument.Symbol);
	    		if( m_position != null) {
		    		orderLog.Notice( "Starting position = " + Display(m_position));
	    		}
		    	MbtOpenOrders orders = m_OrderClient.OpenOrders;
		    	orders.LockItems();
		    	for( int i=0; i< orders.Count; i++) {
		    		orderLog.Notice("Order " + i + ": " + Display(orders[i]));
		    		m_orders[orders[i].OrderNumber] = orders[i];
		    	}
		    	orders.UnlockItems();
		    	foreach( string orderNum in m_orders.Keys) {
		    		string bstrRetMsg = null;
		    		if( m_OrderClient.Cancel( orderNum, ref bstrRetMsg) == true) {
			    		orderLog.Notice("Order " + orderNum + ": Canceled. " + bstrRetMsg);
		    		} else {
			    		orderLog.Notice("Order " + orderNum + ": Cancel Failed: " + bstrRetMsg);
		    		}
		    	}
	    	}
	    	firstTime = false;
	    }

		bool firstHistory = true;
		void SaveHistory( MbtOrderHistory history) {
			if( firstHistory) {
				orderLog.Notice( DisplayHeader(history));
				firstHistory = false;
			}
			orderLog.Notice( DisplayValues(history));
		}
		string DisplayAccount( MbtAccount acct) {
//			TickConsole.Notice("Account: Old Account: " + Display(acct));
			string retVal = "";
			retVal += "Equity="+acct.CurrentEquity;
			retVal += ",Excess="+acct.CurrentExcess;
			retVal += ",MMRUsed="+acct.MMRUsed;
			retVal += ",MMRMultiplier="+acct.MMRMultiplier;
			return retVal;
		}
		
		string DisplayOrder( MbtOpenOrder order) {
//			TickConsole.Notice("Account: Old Order: " + Display(order));
			string retVal = "";
			retVal += "BuySell="+(order.BuySell==MBConst.VALUE_BUY?"Buy":"Sell");
			retVal += ",Symbol="+order.Symbol;
			retVal += ",Quantity="+order.Quantity;
			retVal += ",Price="+order.Price;
			retVal += ",OrderType="+order.OrderType;
			retVal += ",OrderNumber="+order.OrderNumber;
			retVal += ",SharesFilled="+order.SharesFilled;
			return retVal;
		}
		string DisplayHeader( Object obj) {
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						if( i!=0) { retVal += ","; }
						retVal += name;
			    	} catch(TargetParameterCountException) {
						// Ignore
			    	}
				}
	    	} catch(Exception ex) {
	    		orderLog.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}	
		string DisplayValues( Object obj) {
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						string value = pi.GetValue(obj, new object[] {}).ToString();
						if( i!=0) { retVal += ","; }
						retVal += value;
			    	} catch(TargetParameterCountException) {
						// Ignore
			    	}
				}
	    	} catch(Exception ex) {
	    		log.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}	

		string Display( Object obj) {
			if( obj == null) {
				return "null";
			}
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				bool first=true;
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						string value = pi.GetValue(obj, new object[] {}).ToString();
						if( !first) { retVal += ",";}
						first=false;
						retVal += name + "=";
						retVal += value;
			    	} catch(TargetParameterCountException) {
						// Ignore
			    	}
				}
	    	} catch(Exception ex) {
	    		log.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}

    	#region IMbtQuotesNotify Members
        void MBTQUOTELib.IMbtQuotesNotify.OnOptionsData(ref OPTIONSRECORD pRec)
        {
        }
        void MBTQUOTELib.IMbtQuotesNotify.OnTSData(ref TSRECORD pRec)
        {
        }
        void MBTQUOTELib.IMbtQuotesNotify.OnQuoteData(ref QUOTERECORD pQuote)
        {
        	lock( quotesLocker) {
//       			ProcessQuote(ref pQuote);
        	}
        }
//        DateTimeInPlace tempDateTime;
        void MBTQUOTELib.IMbtQuotesNotify.OnLevel2Data(ref LEVEL2RECORD pRec)
        {
        	lock( level2Locker) {
        		try {
            		lastChangeTime = Environment.TickCount;
//            		if( pRec.dPrice > 100 || pRec.dPrice < 1) {
//            		if( pRec.bstrMMID == "LEER-Q") {
//            			log.Info( 
//            			  "pRec.bClosed = " + pRec.bClosed + ", " +
//            			  "pRec.bstrMMID = " + pRec.bstrMMID + ", " +
//            			  "pRec.bstrSource = " + pRec.bstrSource + ", " +
//            			  "pRec.bstrSymbol = " + pRec.bstrSymbol + ", " +
//            			  "pRec.dPrice = " + pRec.dPrice + ", " +
//            			  "pRec.lSize = " + pRec.lSize + ", " +
//            			  "pRec.side = " + pRec.side + ", " +
//            			  "pRec.UTCTime = " + pRec.UTCTime + ", "
//            			 );
//            		}
				if( pRec.dPrice < 100000)
	            	ProcessLevel2(pRec);
        		} catch( Exception e) {
        			log.Notice(e.ToString());
        		}
        	}
        }
        #endregion

		public Level2Collection Level2Bids {
			get { return level2Bids; }
		}
		public Level2Collection Level2Asks {
			get { return level2Asks; }
		}

		public QuoteType QuoteType {
			get { return quoteType; }
			set { quoteType = value; }
		}
        
        public string LogMarket() {
        	return level2Bids[0].dPrice + "/" + level2Asks[0].dPrice + "  " + 
        		level2Bids[0].lSize/instrument.Level2LotSize + "/" + level2Asks[0].lSize/instrument.Level2LotSize;
        }
         
		public SymbolInfo Instrument {
			get { return instrument; }
		}
		public double LastAsk {
			get { return level2Asks.LastPrice; }
		}
        
		public double LastBid {
			get { return level2Bids.LastPrice; }
		}
    	
		public MbtAccount Account {
			get { return m_account; }
			set { m_account = value; }
		}
        
		public bool Advised {
			get { return advised; }
		}
        
		public int LastChangeTime {
			get { return lastChangeTime; }
		}
		
		public override string ToString()
		{
			return "InstrumentReader for " + instrument.Symbol;
		}
        
		public Receiver Receiver {
			get { return receiver; }
			set { receiver = value; }
		}
    }
}

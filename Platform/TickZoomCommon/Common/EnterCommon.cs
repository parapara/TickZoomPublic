#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of StrategySupport.
	/// </summary>
	public class EnterCommon : StrategySupport
	{
		private LogicalOrder buyMarket;
		private LogicalOrder sellMarket;
		private LogicalOrder buyStop;
		private LogicalOrder sellStop;
		private LogicalOrder buyLimit;
		private LogicalOrder sellLimit;
		protected Dictionary<string,Tick> setupList = new Dictionary<string,Tick>();
		private bool enableWrongSideOrders = false;
		private bool allowReversal;
		private bool nextBar;
		
		public EnterCommon(bool allowReversal, bool nextBar, StrategyCommon strategy) : base(strategy) {
			this.allowReversal = allowReversal;
			this.nextBar = nextBar;
		}
		
		public override void OnInitialize()
		{
			if( IsDebug) Log.Debug("OnInitialize()");
			Drawing.Color = Color.Black;
			buyMarket = Data.CreateOrder(this);
			buyMarket.Type = OrderType.BuyMarket;
			sellMarket = Data.CreateOrder(this);
			sellMarket.Type = OrderType.SellMarket;
			buyStop = Data.CreateOrder(this);
			buyStop.Type = OrderType.BuyStop;
			sellStop = Data.CreateOrder(this);
			sellStop.Type = OrderType.SellStop;
			buyLimit = Data.CreateOrder(this);
			buyLimit.Type = OrderType.BuyLimit;
			sellLimit = Data.CreateOrder(this);
			sellLimit.Type = OrderType.SellLimit;
			Strategy.OrderManager.Add( buyStop);
			Strategy.OrderManager.Add( sellStop);
			Strategy.OrderManager.Add( buyLimit);
			Strategy.OrderManager.Add( sellLimit);
		}
	
		public sealed override bool OnProcessTick(Tick tick)
		{
			if( IsTrace) Log.Trace("OnProcessTick() Model.Signal="+Strategy.Position.Signal);
			
			if( Strategy.Position.IsFlat ||
			   (allowReversal && Strategy.Position.IsShort) ) {
				if( buyMarket.IsActive ) ProcessBuyMarket(tick);
				if( buyStop.IsActive ) ProcessBuyStop(tick);
				if( buyLimit.IsActive ) ProcessBuyLimit(tick);
			}
			
			if( Strategy.Position.IsFlat ||
			   (allowReversal && Strategy.Position.IsLong) ) {
				if( sellMarket.IsActive ) ProcessSellMarket(tick);
				if( sellStop.IsActive ) ProcessSellStop(tick);
				if( sellLimit.IsActive ) ProcessSellLimit(tick);
			}
			
			return true;
		}

        public void CancelOrders()
        {
        	buyMarket.IsActive = false;
            sellMarket.IsActive = false;
        	buyStop.IsActive = false;
            sellStop.IsActive = false;
            buyLimit.IsActive = false;
            sellLimit.IsActive = false;
        }

        private void ProcessBuyMarket(Tick tick)
        {
			LogicalOrder order = buyMarket;
			if( order.IsActive) {
				LogEntry("Long Market Entry at " + tick);
				Strategy.Position.Signal = order.Positions;
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(order,tick.Ask,Strategy.Position.Signal);
				}
				CancelOrders();
				setupList.Clear();
			} 
		}
        
		private void ProcessSellMarket(Tick tick) {
			LogicalOrder order = sellMarket;
			if( order.IsActive) {
				LogEntry("Short Market Entry at " + tick);
				Strategy.Position.Signal = - order.Positions;
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(order,tick.Bid,Strategy.Position.Signal);
				}
				CancelOrders();
                setupList.Clear();
			} 
		}
		
        private void ProcessBuyLimit(Tick tick)
        {
			LogicalOrder order = buyLimit;
			if( order.IsActive &&
			    Strategy.Position.IsFlat)
            {
                if (tick.Ask <= order.Price || 
				    (tick.IsTrade && tick.Price < order.Price))
                {
                    LogEntry("Long Limit Entry at " + tick);
                    Strategy.Position.Signal = order.Positions;
                    if (Strategy.Performance.GraphTrades)
                    {
                        Strategy.Chart.DrawTrade(order, tick.Ask, Strategy.Position.Signal);
                    }
                    CancelOrders();
                    setupList.Clear();
                }
			} 
		}
		
		private void ProcessSellLimit(Tick tick) {
			LogicalOrder order = sellLimit;
			if( order.IsActive &&
			    Strategy.Position.IsFlat)
            {
                if (tick.Bid >= order.Price || 
				    (tick.IsTrade && tick.Price > order.Price))
                {
                    LogEntry("Short Limit Entry at " + tick);
                    Strategy.Position.Signal = -order.Positions;
                    if (Strategy.Performance.GraphTrades)
                    {
                        Strategy.Chart.DrawTrade(order, tick.Bid, Strategy.Position.Signal);
                    }
                    CancelOrders();
                    setupList.Clear();
                }
			} 
		}
		
		private void ProcessBuyStop(Tick tick) {
			LogicalOrder order = buyStop;
			if( order.IsActive &&
			    Strategy.Position.IsFlat &&
			    tick.Ask >= order.Price) {
				LogEntry("Long Stop Entry at " + tick);
				Strategy.Position.Signal = order.Positions;
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(order,tick.Ask,Strategy.Position.Signal);
				}
				CancelOrders();
                setupList.Clear();
			} 
		}
		
		private void ProcessSellStop(Tick tick) {
			LogicalOrder order = sellStop;
			if( order.IsActive &&
			    Strategy.Position.IsFlat &&
			    tick.Ask <= order.Price) {
				LogEntry("Short Stop Entry at " + tick);
				Strategy.Position.Signal = - order.Positions;
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(order,tick.Ask,Strategy.Position.Signal);
				}
				CancelOrders();
                setupList.Clear();
			}
		}
		
		private void LogEntry(string description) {
			if( Chart.IsDynamicUpdate) {
				Log.Notice("Bar="+Chart.DisplayBars.CurrentBar+", " + description);
			} else {
        		if( IsDebug) Log.Debug("Bar="+Chart.DisplayBars.CurrentBar+", " + description);
			}
		}
		
        #region Properties		
        public void SellMarket() {
        	SellMarket(1);
        }
        
        public void SellMarket( double positions) {
        	if( Strategy.Position.IsShort) {
        		string reversal = allowReversal ? "or long " : "";
        		string reversalEnd = allowReversal ? " since AllowReversal is true" : "";
        		throw new TickZoomException("Strategy must be flat "+reversal+"before a sell market entry"+reversalEnd+".");
        	}
        	if( !allowReversal && Strategy.Position.IsLong) {
        		throw new TickZoomException("Strategy cannot enter sell market when position is short. Set AllowReversal to true to allow this.");
        	}
        	if( !allowReversal && Strategy.Position.HasPosition) {
        		throw new TickZoomException("Strategy must be flat before a short market entry.");
        	}
        	/// <summary>
        	/// comment.
        	/// </summary>
        	/// <param name="allowReversal"></param>
        	sellMarket.Price = 0;
        	sellMarket.Positions = positions;
        	sellMarket.IsActive = true;
        }
        
        [Obsolete("AllowReversals = true is now default until reverse order types.",true)]
        public void SellMarket(bool allowReversal) {
        }
        
        [Obsolete("AllowReversals = true is now default until reverse order types.",true)]
        public void SellMarket( double positions, bool allowReversal) {
		}
        
        public void BuyMarket() {
        	BuyMarket( 1);
        }
        
        public void BuyMarket(double positions) {
        	if( Strategy.Position.IsLong) {
        		string reversal = allowReversal ? "or short " : "";
        		string reversalEnd = allowReversal ? " since AllowReversal is true" : "";
        		throw new TickZoomException("Strategy must be flat "+reversal+"before a long market entry"+reversalEnd+".");
        	}
        	if( !allowReversal && Strategy.Position.IsShort) {
        		throw new TickZoomException("Strategy cannot enter long market when position is short. Set AllowReversal to true to allow this.");
        	}
        	buyMarket.Price = 0;
        	buyMarket.Positions = positions;
        	buyMarket.IsActive = true;
        }
        
        [Obsolete("AllowReversals = true is now default until reverse order types.",true)]
        public void BuyMarket(bool allowReversal) {
        }
        
        [Obsolete("AllowReversals = true is now default until reverse order types.",true)]
        public void BuyMarket( double positions, bool allowReversal) {
		}
        
        public void BuyLimit( double price) {
        	BuyLimit( price, 1);
        }
        	
        /// <summary>
        /// Create a active buy limit order.
        /// </summary>
        /// <param name="price">Order price.</param>
        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
        ///  use PositionSize.Size.</param>

        public void BuyLimit( double price, double positions) {
        	if( Strategy.Position.HasPosition) {
        		throw new TickZoomException("Strategy must be flat before setting a long limit entry.");
        	}
        	double ask = Ticks[0].Ask;
        	if( price >= ask) {
        		if( enableWrongSideOrders) {
        			BuyMarket(positions);
		        	buyLimit.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Enter Buy Limit price " + price + " was greater than or equal to the current ask price " + ask + " at bar " + bar);
        		}
        	}
        	buyLimit.Price = price;
        	buyLimit.Positions = positions;
        	buyLimit.IsActive = true;
		}
        
        public void SellLimit( double price) {
        	SellLimit( price, 1);
        }
        	
        /// <summary>
        /// Create a active sell limit order.
        /// </summary>
        /// <param name="price">Order price.</param>
        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
        ///  use PositionSize.Size.</param>

        public void SellLimit( double price, double positions) {
        	if( Strategy.Position.HasPosition) {
        		throw new TickZoomException("Strategy must be flat before setting a short limit entry.");
        	}
        	double bid = Ticks[0].Bid;
        	if( price <= bid) {
        		if( enableWrongSideOrders) {
        			SellMarket(positions);
		        	sellLimit.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Enter Sell limit price " + price + " was less than or equal to the current bid price " + bid + " at bar " + bar);
        		}
        	}
        	sellLimit.Price = price;
        	sellLimit.Positions = positions;
        	sellLimit.IsActive = true;
		}
        
        public void BuyStop( double price) {
        	BuyStop( price, 1);
        }
        
        /// <summary>
        /// Create a active buy stop order.
        /// </summary>
        /// <param name="price">Order price.</param>
        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
        ///  use PositionSize.Size.</param>

        public void BuyStop( double price, double positions) {
        	if( Strategy.Position.HasPosition) {
        		throw new TickZoomException("Strategy must be flat before setting a long stop entry.");
        	}
        	double bid = Ticks[0].Bid;
        	if( price <= bid) {
        		if( enableWrongSideOrders) {
        			BuyMarket(positions);
		        	buyStop.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Enter Buy Stop price " + price + " was less than or equal to the current bid price " + bid + " at bar " + bar);
        		}
        	}
        	buyStop.Price = price;
        	buyStop.Positions = positions;
        	buyStop.IsActive = true;
		}
	
        public void SellStop( double price) {
        	SellStop( price, 1);
        }
        
        /// <summary>
        /// Create a active sell stop order.
        /// </summary>
        /// <param name="price">Order price.</param>
        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
        ///  use PositionSize.Size.</param>
        
        public void SellStop( double price, double positions) {
        	if( Strategy.Position.HasPosition) {
        		throw new TickZoomException("Strategy must be flat before setting a short stop entry.");
        	}
        	double ask = Ticks[0].Ask;
        	if( price >= ask) {
        		if( enableWrongSideOrders) {
        			SellMarket(positions);
		        	sellStop.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Enter Sell Stop price " + price + " was greater than or equal to the current ask price " + ask + " at bar " + bar);
        		}
        	}
        	sellStop.Price = price;
        	sellStop.Positions = positions;
        	sellStop.IsActive = true;
        }
        
		#endregion
	
		public override string ToString()
		{
			return FullName;
		}
		
		public void ClearSetups() {
			setupList.Clear();
		}
		
		public bool IsSetup(string name) {
			Tick tick;
			if( setupList.TryGetValue(name, out tick) == false) {
				return false;
			} else {
				return true;
			}
		}

		public TimeStamp SetupTime(string number) {
			Tick tick;
			if( setupList.TryGetValue(number, out tick) == false) {
				throw new ApplicationException( "SetupTime() called with invalid setup number: " + number);
			} else {
				return tick.Time;
			}
		}
		
		public Tick SetupTick(string name) {
			Tick tick;
			if( setupList.TryGetValue(name, out tick) == false) {
				throw new ApplicationException( "SetupTick() called with invalid setup name: " + name);
			} else {
				return tick;
			}
		}
		
		public Elapsed SetupSpan(string number) {
			return Strategy.Data.Ticks[0].Time - SetupTime(number);
		}
		
		public void Setup(string name) {
			setupList[name] = Strategy.Data.Ticks[0];
		}
		
		// This just makes sure nothing uses PositionChange
		// here because only Strategy.PositionChange must be used.
		public new int Position {
			get { return 0; }
			set { /* ignore */ }
		}
		
		public bool EnableWrongSideOrders {
			get { return enableWrongSideOrders; }
			set { enableWrongSideOrders = value; }
		}

		public bool HasBuyOrder {
			get {
				return buyStop.IsActive || buyLimit.IsActive || buyMarket.IsActive;
			}
		}
		
		public bool HasSellOrder {
			get {
				return sellStop.IsActive || sellLimit.IsActive || sellMarket.IsActive;
			}
		}
	}
}

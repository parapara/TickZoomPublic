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
using System.Diagnostics;
using System.Drawing;

using TickZoom.Api;


namespace TickZoom.Common
{
	/// <summary>
	/// Description of StrategySupport.
	/// </summary>
	public class ExitCommon : StrategySupport
	{
		private LogicalOrder buyMarket;
		private LogicalOrder sellMarket;
		private LogicalOrder buyStop;
		private LogicalOrder sellStop;
		private LogicalOrder buyLimit;
		private LogicalOrder sellLimit;
		private PositionInterface position;
		private bool enableWrongSideOrders = false;
		
		public ExitCommon(Strategy strategy) : base(strategy) {
		}
		
		public override void OnInitialize()
		{
			if( IsTrace) Log.Trace(FullName+".Initialize()");
			Drawing.Color = Color.Black;
			buyMarket = Data.CreateOrder(this);
			buyMarket.Type = OrderType.BuyMarket;
			sellMarket = Data.CreateOrder(this);
			sellMarket.Type = OrderType.SellMarket;
			buyStop = Data.CreateOrder(this);
			buyStop.Type = OrderType.BuyStop;
			buyStop.TradeDirection = TradeDirection.Exit;
			sellStop = Data.CreateOrder(this);
			sellStop.Type = OrderType.SellStop;
			sellStop.TradeDirection = TradeDirection.Exit;
			buyLimit = Data.CreateOrder(this);
			buyLimit.Type = OrderType.BuyLimit;
			buyLimit.TradeDirection = TradeDirection.Exit;
			sellLimit = Data.CreateOrder(this);
			sellLimit.Type = OrderType.SellLimit;
			sellLimit.TradeDirection = TradeDirection.Exit;
			Strategy.OrderManager.Add( buyStop);
			Strategy.OrderManager.Add( sellStop);
			Strategy.OrderManager.Add( buyLimit);
			Strategy.OrderManager.Add( sellLimit);
			position = Strategy.Position;
		}
	
		public sealed override bool OnProcessTick(Tick tick)
		{
			if( IsTrace) Log.Trace("OnProcessTick() Model="+Strategy+" Signal="+Strategy.Position.Signal);
			if( position.IsLong) {
				buyStop.IsActive = false;
				buyLimit.IsActive = false;
			}
			if( position.IsShort) {
				sellLimit.IsActive = false;
				sellStop.IsActive = false;
			}
			if( position.HasPosition ) {
				// copy signal in case of increased position size
				if( buyStop.IsActive ) ProcessBuyStop(tick);
				if( sellStop.IsActive ) ProcessSellStop(tick);
				if( buyLimit.IsActive ) ProcessBuyLimit(tick);
				if( sellLimit.IsActive ) ProcessSellLimit(tick);
			}
			return true;
		}
		
		private void FlattenSignal() {
			Strategy.Position.Signal = 0;
			CancelOrders();
		}
	
		public void CancelOrders() {
			buyStop.IsActive = false;
			sellStop.IsActive = false;
			buyLimit.IsActive = false;
			sellLimit.IsActive = false;
		}
		
		private void ProcessBuyStop(Tick tick) {
			if( buyStop.IsActive &&
			    Strategy.Position.IsShort &&
			    tick.Ask >= buyStop.Price) {
				LogExit("Buy Stop Exit at " + tick);
				FlattenSignal();
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(buyStop,tick.Ask,Strategy.Position.Signal);
				}
				CancelOrders();
			} 
		}
		
		private void ProcessBuyLimit(Tick tick) {
			if( buyLimit.IsActive && 
			    Strategy.Position.IsShort)
            {
                if (tick.Ask <= buyLimit.Price || 
				    (tick.IsTrade && tick.Price < buyLimit.Price))
                {
                    LogExit("Buy Limit Exit at " + tick);
                    FlattenSignal();
                    if (Strategy.Performance.GraphTrades)
                    {
                        Strategy.Chart.DrawTrade(buyLimit, tick.Ask, Strategy.Position.Signal);
                    }
                    CancelOrders();
                }
			} 
		}
		
		private void ProcessSellStop(Tick tick) {
			if( sellStop.IsActive &&
			    Strategy.Position.IsLong &&
			    tick.Bid <= sellStop.Price) {
				LogExit("Sell Stop Exit at " + tick);
				FlattenSignal();
				if( Strategy.Performance.GraphTrades) {
	                Strategy.Chart.DrawTrade(sellStop,tick.Ask,Strategy.Position.Signal);
				}
				CancelOrders();
			}
		}
		
		private void ProcessSellLimit(Tick tick) {
			if( sellLimit.IsActive &&
			    Strategy.Position.IsLong)
            {
                if (tick.Bid >= sellLimit.Price || 
				    (tick.IsTrade && tick.Price > sellLimit.Price))
                {
                    LogExit("Sell Stop Limit at " + tick);
                    FlattenSignal();
                    if (Strategy.Performance.GraphTrades)
                    {
                        Strategy.Chart.DrawTrade(sellLimit, tick.Bid, Strategy.Position.Signal);
                    }
                    CancelOrders();
                }
			}
		}
		
		private void LogExit(string description) {
			if( Chart.IsDynamicUpdate) {
				Log.Notice("Bar="+Chart.DisplayBars.CurrentBar+", " + description);
			} else {
        		if( IsDebug) Log.Debug("Bar="+Chart.DisplayBars.CurrentBar+", " + description);
			}
		}

        #region Orders

        public void GoFlat() {
        	if( Strategy.Position.IsFlat) {
        		throw new TickZoomException("Strategy must have a position before attempting to go flat.");
        	}
        	if( Strategy.Position.IsLong) {
	        	sellMarket.Price = 0;
	        	sellMarket.Positions = Strategy.Position.Size;
	        	sellMarket.IsActive = true;
				if( Strategy.Performance.GraphTrades) {
	        		Strategy.Chart.DrawTrade(sellMarket,Ticks[0].Bid,Strategy.Position.Signal);
				}
        	}
        	if( Strategy.Position.IsShort) {
	        	buyMarket.Price = 0;
	        	buyMarket.Positions = Strategy.Position.Size;
	        	buyMarket.IsActive = true;
				if( Strategy.Performance.GraphTrades) {
	        		Strategy.Chart.DrawTrade(buyMarket,Ticks[0].Ask,Strategy.Position.Signal);
				}
        	}
        	LogExit("GoFlat");
        	FlattenSignal();
		}
	
        public void BuyStop(double price) {
        	if( Strategy.Position.IsLong) {
        		throw new TickZoomException("Strategy must be short or flat before setting a buy stop to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.Now.HasSellOrder) {
        			throw new TickZoomException("When flat, a sell order must be active before creating a buy order to exit.");
        		}
			}
        	double bid = Ticks[0].Bid;
   			if( Strategy.Position.HasPosition && price <= bid) {
        		if( enableWrongSideOrders ) {
        			GoFlat();
        			buyStop.IsActive = false;
        			return;
       			} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Exit Buy Stop price " + price + " was less than or equal to the current bid price " + bid + " at bar " + bar);
        	    }
        	} else {
        		buyStop.Price = price;
        		buyStop.IsActive = true;
        	}
        }
	
        public void SellStop( double price) {
        	if( Strategy.Position.IsShort) {
        		throw new TickZoomException("Strategy must be long or flat before setting a sell stop to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.Now.HasBuyOrder) {
        			throw new TickZoomException("When flat, a buy order must be active before creating a sell order to exit.");
        		}
        	}
        	double ask = Ticks[0].Ask;
   			if( Strategy.Position.HasPosition && price >= ask) {
        		if( enableWrongSideOrders ) {
        			GoFlat();
        			sellStop.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Exit Sell Stop price " + price + " was greater than or equal to the current ask price " + ask + " at bar " + bar);
        		}
			} else {
				sellStop.Price = price;
				sellStop.IsActive = true;
			}
		}
        
        public void BuyLimit(double price) {
        	if( Strategy.Position.IsLong) {
        		throw new TickZoomException("Strategy must be short or flat before setting a buy limit to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.Now.HasSellOrder) {
        			throw new TickZoomException("When flat, a sell order must be active before creating a buy order to exit.");
        		}
			}
        	double ask = Ticks[0].Ask;
   			if( Strategy.Position.HasPosition && price >= ask) {
        		if( enableWrongSideOrders ) {
       				GoFlat();
        			buyLimit.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Exit Buy Limit price " + price + " was greater than or equal to the current ask price " + ask + " at bar " + bar);
        		}
			} else {
        		buyLimit.Price = price;
        		buyLimit.IsActive = true;
			}
		}
	
        public void SellLimit( double price) {
        	if( Strategy.Position.IsShort) {
        		throw new TickZoomException("Strategy must be long or flat before setting a sell limit to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.Now.HasBuyOrder) {
        			throw new TickZoomException("When flat, a buy order must be active before creating a sell order to exit.");
        		}
			}
        	double bid = Ticks[0].Bid;
   			if( Strategy.Position.HasPosition && price <= bid) {
        		if( enableWrongSideOrders ) {
        			GoFlat();
        			sellLimit.IsActive = false;
        			return;
        		} else {
        			int bar = Chart.ChartBars.BarCount;
        			throw new TickZoomException("Exit Sell limit price " + price + " was less than or equal to the current bid price " + bid + " at bar " + bar);
        		}
			} else {
				sellLimit.Price = price;
    	    	sellLimit.IsActive = true;
			}
		}
        
		#endregion

		
		public override string ToString()
		{
			return FullName;
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
	}
}

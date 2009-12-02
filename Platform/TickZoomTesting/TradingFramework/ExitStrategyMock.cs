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
using TickZoom.Api;
using TickZoom.Common;

//using mscoree;







#if TESTING
namespace TickZoom.TradingFramework
{
	public class ExitStrategyMock : ExitStrategy {
		private static readonly Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly bool trace = log.IsTraceEnabled;
		public List<TimeStamp> signalChanges = new List<TimeStamp>();
		public List<double> signalDirection = new List<double>();
		double prevSignal = 0;
		public TradeSignalTest tradeSignalTest;
		
		public ExitStrategyMock(Strategy strategy) : base(strategy) {
		}
		
		public override void OnInitialize()
		{
			base.OnInitialize();
			if( trace) log.Trace(FullName+" Initialize()");
			Position = tradeSignalTest = new TradeSignalTest(this);
			signalChanges = new List<TimeStamp>();
			List<int> signalDirection = new List<int>();
		}
		
		public class TradeSignalTest : PositionCommon {
			Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			new ExitStrategyMock model;
			public TradeSignalTest( ExitStrategyMock formula ) : base(formula) {
				this.model = formula;
			}
			public override double Signal  {
				get { return base.Signal; }
				set { base.Signal = value;
						if( base.Signal != model.prevSignal) {
							Tick tick = model.Ticks[0];
							log.Debug( model.signalChanges.Count + " " + tick);
							model.signalChanges.Add(tick.Time);
							model.signalDirection.Add(base.Signal);
							model.prevSignal = base.Signal;
						}
				}
			}
	
		}
		public void TickConsoleWrite() {
			for( int i = 0; i< signalChanges.Count; i++) {
				TimeStamp time = signalChanges[i];
				double signal = signalDirection[i];
				// DO NOT COMMENT OUT
				log.Debug( i + ": " + time + " Direction: " + signal);
				// DO NOT COMMENT OUT
			}
		}
	}
}
#endif

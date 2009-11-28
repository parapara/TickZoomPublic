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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

using TickZoom.Api;
using ZedGraph;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of MoneyManagerSupport.
	/// </summary>
	public class Equity : StrategySupport
	{
		TransactionPairs daily;
		TransactionPairs weekly;
		TransactionPairs monthly;
		TransactionPairs yearly;
		TransactionPairsBinary dailyBinary;
		TransactionPairsBinary weeklyBinary;
		TransactionPairsBinary monthlyBinary;
		TransactionPairsBinary yearlyBinary;
		double closedEquity = 0;
		double openEquity = 0;
		StrategySupportInterface next;
		Strategy strategy;
		double startingEquity = 10000;
		bool graphEquity = false;
		IndicatorCommon equity;
		ProfitLoss equityProfitLoss;

		public Equity(Strategy strategy) : base(strategy)
		{
			this.strategy = strategy;
			equityProfitLoss = new TradeProfitLoss(strategy);
			dailyBinary   = new TransactionPairsBinary();
			daily  = new TransactionPairs(equityProfitLoss,dailyBinary);
			dailyBinary.Name = "Daily";
			weeklyBinary  = new TransactionPairsBinary();
			weekly  = new TransactionPairs(equityProfitLoss,weeklyBinary);
			weeklyBinary.Name = "Weekly";
			monthlyBinary = new TransactionPairsBinary();
			monthly  = new TransactionPairs(equityProfitLoss,monthlyBinary);
			monthlyBinary.Name = "Monthly";
			yearlyBinary = new TransactionPairsBinary();
			yearly  = new TransactionPairs(equityProfitLoss,yearlyBinary);
			yearlyBinary.Name = "Yearly";
		}
		
		public override void OnInitialize()
		{
			next = Chain.Next.Model as StrategySupportInterface;
			RequestUpdate(Intervals.Day1);
			RequestUpdate(Intervals.Week1);
			RequestUpdate(Intervals.Month1);
			RequestUpdate(Intervals.Year1);
			closedEquity = startingEquity;
			
			if( graphEquity) {
				equity = new IndicatorCommon();
				equity.Drawing.IsVisible = true;
				equity.Drawing.PaneType = PaneType.Secondary;
				equity.Drawing.GraphType = GraphType.FilledLine;
				equity.Drawing.Color = Color.Green;
				equity.Drawing.GroupName = "SimpleEquity";
				equity.Name = "SimpleEquity";
				AddIndicator(equity);
			}
			
		}
		
		public sealed override bool OnProcessTick(Tick tick)
		{
			Position.Change(next.Position.Current);
			return true;
		}
		
		public void OnChangeClosedEquity(double profitLoss) {
			closedEquity += profitLoss;
		}
		
		public void OnSetOpenEquity(double value) {
			openEquity = value;
		}
		
		public override bool OnIntervalOpen(Interval interval)
		{
			if( IsTrace) Log.Trace("IntervalOpen("+interval+")");
			if( Data == null) {
				Log.Debug("Data is null");
			}
			
			if( Ticks == null) {
				Log.Debug("Ticks is null");
			}
			
			if( Ticks[0] == null) {
				Log.Debug("Ticks is null");
			}
			
			TimeStamp dt;
			try { 
				dt = Ticks[0].Time;
			} catch( NullReferenceException ex) {
				Log.Debug("RETRYING: Found exception " + ex);
				Thread.Sleep(100);
				dt = Ticks[0].Time;
			}
			if( interval.Equals(Intervals.Day1)) {
                // original Fx tick data lacking in most of March 2004, enable next line to filter that time
				//isSkip = Days.Time[0].Month == 3 && Days.Time[0].Year == 2004;
				CalcNew(dailyBinary);
			} else if( interval.Equals(Intervals.Week1)) {
				CalcNew(weeklyBinary);
			} else if( interval.Equals(Intervals.Month1)) {
				CalcNew(monthlyBinary);
			} else if( interval.Equals(Intervals.Year1)) {
				CalcNew(yearlyBinary);
			}
			return true;
		}

		public override bool OnIntervalClose(Interval interval)
		{
			if( IsTrace) Log.Trace("IntervalClose("+interval+")");
			if( interval.Equals(Intervals.Day1)) {
				CalcEnd(dailyBinary);
			} else if( interval.Equals(Intervals.Week1)) {
				CalcEnd(weeklyBinary);
			} else if( interval.Equals(Intervals.Month1)) {
				CalcEnd(monthlyBinary);
			} else if( interval.Equals(Intervals.Year1)) {
				CalcEnd(yearlyBinary);
			}
			return true;
		}
		
		public override bool OnIntervalClose()
		{
			if( graphEquity) {
				equity[0] = CurrentEquity;
			}
			return true;
		}

		void CalcNew(TransactionPairsBinary periodTrades) {
			TransactionPairBinary trade = TransactionPairBinary.Create();
			trade.Direction = 1;
			trade.EntryPrice = CurrentEquity;
			trade.EntryTime = Ticks[0].Time;
			periodTrades.Add(trade);
		}

		void CalcEnd(TransactionPairsBinary periodTrades) {
			if( periodTrades.Count>0) {
				TransactionPairBinary pair = periodTrades[periodTrades.Count - 1];
				pair.ExitPrice = CurrentEquity;
				pair.ExitTime = Ticks[0].Time;
				periodTrades[periodTrades.Count - 1] = pair;
			}
		}
		
		public bool WriteReport(string name, string folder, StrategyStats strategyStats) {
			EquityStatsReport equityStats = new EquityStatsReport(this);
			equityStats.StrategyStats = strategyStats;
			equityStats.WriteReport(name,folder);
			return true;
		}

		[Obsolete("This method is never used.",true)]
		public override double Fitness {
			get { TradeStats stats = new TradeStats( CompletedMonthly); 
				return stats.Average; }
		}
		
		[Obsolete("Override OnGetFitness() or OnStatistics() in a strategy instead.",true)]
		public override string OnOptimizeResults() {
			throw new NotImplementedException();
		}

		[Obsolete("Use Performance.ProfitLossCalculation.Slippage or create your own ProfitLossCalculation instead.",true)]
		public double Slippage {
			get { return 0.0D; }
			set {  }
		}
		
		[Obsolete("Use Performance.ProfitLossCalculation.Slippage or create your own ProfitLossCalculation instead.",true)]
		public double Commission {
			get { return 0.0D; }
			set {  }
		}

		[Obsolete("Use Daily instead.",true)]
		public TransactionPairs CompletedDaily {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,dailyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Weekly instead.",true)]
		public TransactionPairs CompletedWeekly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,weeklyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Monthly instead.",true)]
		public TransactionPairs CompletedMonthly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,monthlyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
	
		[Obsolete("Use Yearly instead.",true)]
		public TransactionPairs CompletedYearly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,yearlyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Daily {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,dailyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Weekly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,weeklyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Monthly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,monthlyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Yearly {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(equityProfitLoss,yearlyBinary.GetCompletedList(Ticks[0].Time,CurrentEquity,Bars.BarCount));
				} else {
					return new TransactionPairs(equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public double ProfitToday {
			get { 
				if( dailyBinary.Count > 0) {
			         return CurrentEquity - dailyBinary.Current.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}
		
		[Browsable(false)]
		public double ProfitForWeek {
			get { 
				if( weeklyBinary.Count > 0) {
			         return CurrentEquity - weeklyBinary.Current.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}

		[Browsable(false)]
		public double ProfitForMonth {
			get { 
				if( monthlyBinary.Count > 0) {
			         return CurrentEquity - monthlyBinary.Current.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}
		
		public double StartingEquity {
			get { return startingEquity; }
			set { startingEquity = value; }
		}
		
		[Browsable(false)]
		public double CurrentEquity {
			get { 
				return ClosedEquity + OpenEquity;
			}
		}
		
		/// <summary>
		/// ClosedEquity return the running total of profit or loss
		/// from all closed trades.
		/// </summary>
		public double ClosedEquity {
			get {
				return closedEquity;
			}
		}
		
		/// <summary>
		/// OpenEquity returns zero unless there is an open position.
		/// In that case, it returns the amount of open equity.
		/// </summary>
		[Browsable(false)]
		public double OpenEquity {
			get { return openEquity; }
		}
		
		public EquityStats CalculateStatistics() {
			return new EquityStats(Daily,Weekly,Monthly,Yearly);
		}
		
		public bool GraphEquity {
			get { return graphEquity; }
			set { graphEquity = value; }
		}
		
	}
}

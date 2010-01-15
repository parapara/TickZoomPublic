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
	public class Equity : StrategyInterceptor
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
		Model model;
		Performance performance;
		double startingEquity = 10000;
		bool graphEquity = false;
		IndicatorCommon equity;
		ProfitLoss equityProfitLoss;
		bool isMultiSymbolPortfolio;
		PortfolioInterface portfolio;
		bool enableYearlyStats = false;
		bool enableMonthlyStats = false;
		bool enableWeeklyStats = false;
		bool enableDailyStats = false;
		bool isInitialized = false;
		
		public Equity(Model model, Performance performance) 
		{
			this.model = model;
			this.performance = performance;
			equityProfitLoss = new TradeProfitLoss(model);
			dailyBinary   = new TransactionPairsBinary();
		}
		
		public void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			if( EventType.Initialize == eventType) {
				OnInitialize();
			}
			context.Invoke();
			if( EventType.Open == eventType) {
				if( eventDetail != null) {
					OnIntervalOpen((Interval)eventDetail);
				}
			} else if( EventType.Close == eventType) {
				if( eventDetail == null) {
					OnIntervalClose();
				} else {
					OnIntervalClose((Interval)eventDetail);
				}
			}
		}

		public void OnInitialize()
		{
			portfolio = model as PortfolioInterface;
			if( portfolio != null) {
				isMultiSymbolPortfolio = portfolio.PortfolioType == PortfolioType.MultiSymbol;
			}
			daily  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary);
			dailyBinary.Name = "Daily";
			weeklyBinary  = new TransactionPairsBinary();
			weekly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary);
			weeklyBinary.Name = "Weekly";
			monthlyBinary = new TransactionPairsBinary();
			monthly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary);
			monthlyBinary.Name = "Monthly";
			yearlyBinary = new TransactionPairsBinary();
			yearly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary);
			yearlyBinary.Name = "Yearly";
			closedEquity = startingEquity;
			
			if( graphEquity) {
				equity = new IndicatorCommon();
				equity.Drawing.IsVisible = true;
				equity.Drawing.PaneType = PaneType.Secondary;
				equity.Drawing.GraphType = GraphType.FilledLine;
				equity.Drawing.Color = Color.Green;
				equity.Drawing.GroupName = "SimpleEquity";
				equity.Name = "SimpleEquity";
				model.AddIndicator(equity);
			}
			
			if( enableYearlyStats) {
				model.RequestUpdate(Intervals.Year1);
			}
			if( enableMonthlyStats) {
				model.RequestUpdate(Intervals.Month1);
			}
			if( enableWeeklyStats) {
				model.RequestUpdate(Intervals.Week1);
			}
			if( enableDailyStats) {
				model.RequestUpdate(Intervals.Day1);
			}
			
			model.AddInterceptor( EventType.Open, this);
			model.AddInterceptor( EventType.Close, this);
			isInitialized = true;
		}
		
		public void OnChangeClosedEquity(double profitLoss) {
			closedEquity += profitLoss;
		}
		
		public bool OnIntervalOpen(Interval interval)
		{
			TimeStamp dt;
			try { 
				dt = model.Ticks[0].Time;
			} catch( NullReferenceException ex) {
				Thread.Sleep(100);
				dt = model.Ticks[0].Time;
			}
			if( dailyBinary.Count == 0) CalcNew(dailyBinary);
			if( weeklyBinary.Count == 0) CalcNew(weeklyBinary);
			if( monthlyBinary.Count == 0) CalcNew(monthlyBinary);
			if( yearlyBinary.Count == 0) CalcNew(yearlyBinary);
			return true;
		}

		public bool OnIntervalClose(Interval interval)
		{
			if( interval.Equals(Intervals.Day1)) {
				CalcEnd(dailyBinary);
				CalcNew(dailyBinary);
			} else if( interval.Equals(Intervals.Week1)) {
				CalcEnd(weeklyBinary);
				CalcNew(weeklyBinary);
			} else if( interval.Equals(Intervals.Month1)) {
				CalcEnd(monthlyBinary);
				CalcNew(monthlyBinary);
			} else if( interval.Equals(Intervals.Year1)) {
				CalcEnd(yearlyBinary);
				CalcNew(yearlyBinary);
			}
			return true;
		}
		
		public bool OnIntervalClose()
		{
			if( graphEquity) {
				equity[0] = CurrentEquity;
			}
			return true;
		}
		
		public double CurrentPrice(double direction) {
			return CurrentEquity;
		}

		void CalcNew(TransactionPairsBinary periodTrades) {
			TransactionPairBinary trade = TransactionPairBinary.Create();
			trade.Direction = 1;
			trade.EntryPrice = CurrentEquity;
			trade.EntryTime = model.Ticks[0].Time;
			periodTrades.Add(trade);
		}

		void CalcEnd(TransactionPairsBinary periodTrades) {
			if( periodTrades.Count>0) {
				TransactionPairBinary pair = periodTrades[periodTrades.Count - 1];
				pair.ExitPrice = CurrentEquity;
				pair.ExitTime = model.Ticks[0].Time;
				pair.Completed = true;
				periodTrades[periodTrades.Count - 1] = pair;
			}
		}
		
		public bool WriteReport(string name, string folder, StrategyStats strategyStats) {
			EquityStatsReport equityStats = new EquityStatsReport(this);
			equityStats.StrategyStats = strategyStats;
			equityStats.WriteReport(name,folder);
			return true;
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
		
		public double GetCurrentEquity(double direction) {
			return CurrentEquity;
		}

		[Obsolete("Use Daily instead.",true)]
		public TransactionPairs CompletedDaily {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Weekly instead.",true)]
		public TransactionPairs CompletedWeekly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Monthly instead.",true)]
		public TransactionPairs CompletedMonthly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
	
		[Obsolete("Use Yearly instead.",true)]
		public TransactionPairs CompletedYearly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Daily {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Weekly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Monthly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Yearly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
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
		
		public double NetProfit {
			get { return CurrentEquity - StartingEquity; }
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
			get { 
				if( isMultiSymbolPortfolio) {
					return portfolio.GetOpenEquity();
				} else {
					try { 
						return performance.ComboTrades.OpenProfitLoss;
					} catch( Exception) {
						int x = 0;
						return 0;
					}
				}
			}
		}
		
		public EquityStats CalculateStatistics() {
			return new EquityStats(Daily,Weekly,Monthly,Yearly);
		}
		
		public bool GraphEquity {
			get { return graphEquity; }
			set { if( isInitialized) {
					throw new ApplicationException("You must set GraphEquity before initialize event occurs.");
				} else {
					graphEquity = value;
				}
			}
		}
		
		public bool EnableYearlyStats {
			get { return enableYearlyStats; }
			set { enableYearlyStats = value; }
		}
		
		public bool EnableMonthlyStats {
			get { return enableMonthlyStats; }
			set { enableMonthlyStats = value; }
		}
		
		public bool EnableWeeklyStats {
			get { return enableWeeklyStats; }
			set { enableWeeklyStats = value; }
		}
		
		public bool EnableDailyStats {
			get { return enableDailyStats; }
			set { enableDailyStats = value; }
		}
	}
}

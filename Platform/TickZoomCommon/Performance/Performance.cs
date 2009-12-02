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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.ComponentModel;
using TickZoom.Api;


namespace TickZoom.Common
{
	public class Performance : StrategySupport
	{
		TransactionPairs pairs;
		TransactionPairs comboTrades;
		TransactionPairsBinary transactionPairsBinary;
		TransactionPairsBinary comboTradesBinary;
		Strategy strategy;
		bool graphTrades = true;
		bool graphAveragePrice = false;
		IndicatorCommon avgPrice;
		Chain equityChain;
		StrategySupport next;
		TradeProfitLoss profitLoss;

		public Performance(Strategy strategy) : base(strategy)
		{
			this.strategy = strategy;
			profitLoss = new TradeProfitLoss(strategy);
			Model equity = new Equity(strategy);
			equityChain = Chain.InsertAfter(equity.Chain);
			transactionPairsBinary  = new TransactionPairsBinary();
			pairs = new TransactionPairs(profitLoss,transactionPairsBinary);
			transactionPairsBinary.Name = "Trades";
			comboTradesBinary  = new TransactionPairsBinary();
			comboTrades  = new TransactionPairs(profitLoss,comboTradesBinary);
			comboTradesBinary.Name = "ComboTrades";
		}
		
		public override void OnInitialize()
		{
			next = Chain.Next.Model as StrategySupport;
			profitLoss.FullPointValue = Data.FullPointValue;

			if( graphAveragePrice) {
				avgPrice = new IndicatorCommon();
				avgPrice.Drawing.IsVisible = true;
				avgPrice.Drawing.PaneType = PaneType.Primary;
				avgPrice.Drawing.Color = Color.Green;
				avgPrice.Drawing.GroupName = "Avg Price";
				avgPrice.Name = "Avg Price";
				AddIndicator(avgPrice);
			}
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			if( IsTrace) Log.Trace("ProcessTick() Previous="+next+" Signal="+next.Position.Signal);
			if( next.Position.Signal != Position.Signal) {
				if( IsTrace) Log.Trace("ProcessTick() Signal Changed.");
				if( IsTrace) Log.Indent();
				if( Position.IsFlat) {
					EnterTransactionInternal();
					EnterComboTradeInternal();
				} else if( next.Position.IsFlat) {
					ExitTransactionInternal();
					ExitComboTradeInternal();
//					if( Position.IsShort && graphTrades) {
//						Chart.DrawArrow(ArrowDirection.Up,Color.Black,12.5f,Chart.ChartBars.BarCount,Ticks[0].Ask);
//					}
//					if( Position.IsLong && graphTrades) {
//						Chart.DrawArrow(ArrowDirection.Down,Color.Black,12.5f,Chart.ChartBars.BarCount,Ticks[0].Bid);
//					}
				} else if( (next.Position.IsLong && Position.IsShort) || (next.Position.IsShort && Position.IsLong)) {
					// The signal must be opposite. Either -1 / 1 or 1 / -1
					ExitTransactionInternal();
					ExitComboTradeInternal();
					EnterTransactionInternal();
					EnterComboTradeInternal();
				} else {
					// Instead it has increased or decreased position size.
					ExitTransactionInternal();
					EnterTransactionInternal();
					ChangeComboSizeInternal();
				}
				if( IsTrace) Log.Outdent();
			} 
			Position.Signal = next.Position.Signal;
			if( Position.HasPosition) {
				transactionPairsBinary.Current.UpdatePrice(tick);
				comboTradesBinary.Current.UpdatePrice(tick);
				double pnl = comboTrades.ProfitInPosition(comboTrades.Current,tick);
				if( pnl != 0) {
					Equity.OnSetOpenEquity( pnl);
				}
			}
			return true;
		}
		
		private void EnterTransactionInternal() {
			if( IsTrace) Log.Trace("EnterTradeInternal()");
			TransactionPairBinary pair = TransactionPairBinary.Create();
			pair.Direction = next.Position.Signal;
			if( next.Position.Signal > 0) {
				pair.EntryPrice = Ticks[0].Ask;
			} else {
				pair.EntryPrice = Ticks[0].Bid;
			}
			pair.EntryTime = Ticks[0].Time;
			pair.EntryBar = Chart.ChartBars.BarCount;
			transactionPairsBinary.Add(pair);
		}
		
		private void EnterComboTradeInternal() {
			if( next.Position.Signal > 0) {
				EnterComboTrade(Ticks[0].Ask);
			} else if( next.Position.Signal < 0) {
				EnterComboTrade(Ticks[0].Bid);
			} else {
				throw new ApplicationException( "Direction was unset.");
			}
		}

		public void EnterComboTrade(double fillPrice) {
			if( IsTrace) Log.Trace("EnterComboTradeInternal()");
			TransactionPairBinary pair = TransactionPairBinary.Create();
			pair.Direction = next.Position.Signal;
			pair.EntryPrice = fillPrice;
			pair.EntryTime = Ticks[0].Time;
			pair.EntryBar = Chart.ChartBars.BarCount;
			comboTradesBinary.Add(pair);
			Strategy.OnEnterTrade();
		}
		
		private void ChangeComboSizeInternal() {
			if( IsTrace) Log.Trace("ChangeComboSizeInternal()");
			TransactionPairBinary combo = comboTradesBinary.Current;
			if( next.Position.Signal > 0) {
				combo.ChangeSize(next.Position.Signal,Ticks[0].Ask);
			} else {
				combo.ChangeSize(next.Position.Signal,Ticks[0].Bid);
			}
			comboTradesBinary.Current = combo;
		}
		
		private void ExitTransactionInternal() {
			TransactionPairBinary transactionPair = transactionPairsBinary.Current;
			Tick tick = Ticks[0];
			if( transactionPair.Direction > 0) {
				transactionPair.ExitPrice = tick.Bid;
			} else {
				transactionPair.ExitPrice = tick.Ask;
			}
			transactionPair.ExitTime = tick.Time;
			transactionPair.ExitBar = Chart.ChartBars.BarCount;
			transactionPair.Completed = true;
			transactionPairsBinary.Current = transactionPair;
			if( IsTrace) Log.Trace(transactionPair);
		}
		
		private void ExitComboTradeInternal() {
			TransactionPairBinary comboTrade = comboTradesBinary.Current;
			Tick tick = Ticks[0];
			if( comboTrade.Direction > 0) {
				ExitComboTrade(tick.Bid);
			} else {
				ExitComboTrade(tick.Ask);
			}
		}
					
		public void ExitComboTrade(double fillPrice) {
			if( IsTrace) Log.Trace("ExitComboTradeInternal()");
			TransactionPairBinary comboTrade = comboTradesBinary.Current;
			Tick tick = Ticks[0];
			comboTrade.ExitPrice = fillPrice;
			comboTrade.ExitTime = tick.Time;
			comboTrade.ExitBar = Chart.ChartBars.BarCount;
			comboTrade.Completed = true;
			comboTradesBinary.Current = comboTrade;
			double pnl = profitLoss.CalculateProfit(comboTrade.Direction,comboTrade.EntryPrice,comboTrade.ExitPrice);
			Equity.OnSetOpenEquity(0);
			Equity.OnChangeClosedEquity( pnl);
			
//			Chart.DrawArrow(Color.Black,12.5f,ChartBars.CurrentBar,Ticks[0].Bid+400,ChartBars.CurrentBar,Ticks[0].Bid+300);
//			Chart.DrawText("EOD",Color.Black,ChartBars.CurrentBar,Ticks[0].Bid+400, TextOrientation.LowerRight);
//			int comboExitPrice = 0;
//			Color color;
//			if( comboDirection > 0) {
//				color = Color.Blue;
//				comboExitPrice = Ticks[0].Ask;
//			} else {
//				color = Color.Red;
//				comboExitPrice = Ticks[0].Bid;
//			}
//			if( Chart != null) {
//				Chart.DrawLine(color,comboEntryBar,comboEntryPrice,
//				               ChartBars.CurrentBar,comboExitPrice, LineStyle.Solid);
//			}
			Strategy.OnExitTrade();
		}
		
		protected virtual void EnterTrade() {

		}
		
		protected virtual void ExitTrade() {
			
		}
		
		public override bool OnIntervalClose()
		{
			if( graphAveragePrice) {
				if( comboTradesBinary.Count > 0) {
					TransactionPairBinary combo = comboTradesBinary.Current;
					if( !combo.Completed) {
						avgPrice[0] = combo.EntryPrice;
					} else {
						avgPrice[0] = double.NaN;
					}
				}
			}
			return true;
		}
		
		public void RemoveChildren() {
			Equity.Chain.Remove();
		}
		
		public Equity Equity {
			get { return (Equity) equityChain.Model; }
			set { equityChain = equityChain.Replace(value.Chain); }
		}
		
		public bool WriteReport(string name, string folder) {
			name = name.Replace("/","").Replace(@"\","");
			TradeStatsReport tradeStats = new TradeStatsReport(this);
			tradeStats.WriteReport(name, folder);
			StrategyStats stats = new StrategyStats(TransactionPairs,ComboTrades);
			Equity.WriteReport(name,folder,stats);
			IndexForReport index = new IndexForReport(this);
			index.WriteReport(name, folder);
			return true;
		}

		public double Slippage {
			get { return profitLoss.Slippage; }
			set { profitLoss.Slippage = value; }
		}
		
		public double Commission {
			get { return profitLoss.Commission; }
			set { profitLoss.Commission = value; }
		}
		
#region Obsolete Methods		
		
		[Obsolete("Use WriteReport(name,folder) instead.",true)]
		public void WriteReport(string name,StreamWriter writer) {
			throw new NotImplementedException();
		}

		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public override double Fitness {
			get { return Equity.Fitness; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public override string OnOptimizeResults() {
			throw new NotImplementedException();
		}

		[Obsolete("Please use TransactionPairs instead.",true)]
    	public TransactionPairs CompletedTrades {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(profitLoss,transactionPairsBinary.GetCompletedList(Ticks[0].Time,Ticks[0].Bid,Bars.BarCount));
				} else {
					return new TransactionPairs(profitLoss);
				}
			}
		}

    	public TransactionPairs TransactionPairs {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(profitLoss,transactionPairsBinary.GetCompletedList(Ticks[0].Time,Ticks[0].Bid,Bars.BarCount));
				} else {
					return new TransactionPairs(profitLoss);
				}
			}
		}
		
		[Obsolete("Please use Performance.Equity.Daily instead.",true)]
		public TransactionPairs CompletedDaily {
			get { return Equity.Daily; }
		}

		[Obsolete("Please use Performance.Equity.Weekly instead.",true)]
		public TransactionPairs CompletedWeekly {
			get { return Equity.Weekly; }
		}

		public TransactionPairs ComboTrades {
			get { if( Ticks.Count>0) {
					return new TransactionPairs(profitLoss,comboTradesBinary.GetCompletedList(Ticks[0].Time,Ticks[0].Bid,Bars.BarCount));
				} else {
					return new TransactionPairs(profitLoss);
				}
			}
		}
		
		[Obsolete("Please use Performance.Equity.Monthly instead.",true)]
		public TransactionPairs CompletedMonthly {
			get { return Equity.Monthly; }
		}
	
		[Obsolete("Please use Performance.Equity.Yearly instead.",true)]
		public TransactionPairs CompletedYearly {
			get { return Equity.Yearly; }
		}
		
		[Obsolete("Please use Performance.ComboTrades instead.",true)]
		public TransactionPairs CompletedComboTrades {
			get { return ComboTrades; }
		}
		
		[Obsolete("Please use TransactionPairs instead.",true)]
		public TransactionPairs Trades {
			get { return TransactionPairs; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Daily {
			get { return Equity.Daily; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Weekly {
			get { return Equity.Weekly; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Monthly {
			get { return Equity.Monthly; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Yearly {
			get { return Equity.Yearly; }
		}
		
		public bool GraphTrades {
			get { return graphTrades; }
			set { graphTrades = value; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitToday {
			get { return Equity.CurrentEquity; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitForWeek {
			get { return Equity.ProfitForWeek; }
		}

		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitForMonth {
			get { return Equity.ProfitForMonth; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double StartingEquity {
			get { return Equity.StartingEquity; }
			set { Equity.StartingEquity = value; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double CurrentEquity {
			get { 
				return Equity.CurrentEquity;
			}
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ClosedEquity {
			get {
				return Equity.ClosedEquity;
			}
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double OpenEquity {
			get {
				return Equity.OpenEquity;
			}
		}
		
		public StrategyStats CalculateStatistics() {
			return new StrategyStats(TransactionPairs,ComboTrades);
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public bool GraphEquity {
			get { return Equity.GraphEquity; }
			set { Equity.GraphEquity = value; }
		}
		
		public bool GraphAveragePrice {
			get { return graphAveragePrice; }
			set { graphAveragePrice = value; }
		}
		
		[Obsolete("Please use Slippage and Commission properties on Performance.",true)]
		public ProfitLoss ProfitLoss {
			get { return null; }
			set { }
		}


#endregion		

	}

	[Obsolete("Please user Performance instead.",true)]
	public class PerformanceCommon : Performance {
		public PerformanceCommon(Strategy strategy) : base(strategy)
		{
			
		}
	}
	
}
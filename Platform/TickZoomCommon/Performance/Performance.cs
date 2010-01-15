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
	public class Performance : StrategyInterceptor
	{
		TransactionPairs comboTrades;
		TransactionPairsBinary comboTradesBinary;
		bool graphTrades = true;
		bool graphAveragePrice = false;
		IndicatorCommon avgPrice;
		Equity equity;
		TradeProfitLoss profitLoss;
		List<double> positionChanges = new List<double>();
		PositionCommon position;
		Model model;
		
		public Performance(Model model)
		{
			this.model = model;
			profitLoss = new TradeProfitLoss(model);
			equity = new Equity(model,this);
			comboTradesBinary  = new TransactionPairsBinary();
			comboTradesBinary.Name = "ComboTrades";
			position = new PositionCommon(model);
		}
		
		public double GetCurrentPrice( double direction) {
			System.Diagnostics.Debug.Assert(direction!=0);
			if( direction > 0) {
				return model.Ticks[0].Bid;
			} else {
				return model.Ticks[0].Ask;
			}
		}
		
		EventContext context;
		
		public void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			this.context = context;
			if( EventType.Initialize == eventType) {
				model.AddInterceptor( EventType.Close, this);
				model.AddInterceptor( EventType.Tick, this);
				OnInitialize();
			}
			context.Invoke();
			if( EventType.Tick == eventType) {
				OnProcessTick((Tick)eventDetail);
			}
		}
		
		public void OnInitialize()
		{
			comboTrades  = new TransactionPairs(GetCurrentPrice,profitLoss,comboTradesBinary);
			profitLoss.FullPointValue = model.Data.SymbolInfo.FullPointValue;

			if( graphAveragePrice) {
				avgPrice = new IndicatorCommon();
				avgPrice.Drawing.IsVisible = true;
				avgPrice.Drawing.PaneType = PaneType.Primary;
				avgPrice.Drawing.Color = Color.Green;
				avgPrice.Drawing.GroupName = "Avg Price";
				avgPrice.Name = "Avg Price";
				model.AddIndicator(avgPrice);
			}
		}
		
		public bool OnProcessTick(Tick tick)
		{
			if( context.Position.Current != position.Current) {
				positionChanges.Add(context.Position.Current);
				if( position.IsFlat) {
					EnterComboTradeInternal();
				} else if( context.Position.IsFlat) {
					ExitComboTradeInternal();
				} else if( (context.Position.IsLong && position.IsShort) || (context.Position.IsShort && position.IsLong)) {
					// The signal must be opposite. Either -1 / 1 or 1 / -1
					ExitComboTradeInternal();
					EnterComboTradeInternal();
				} else {
					// Instead it has increased or decreased position size.
					ChangeComboSizeInternal();
				}
			} 
			position.Copy(context.Position);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.Result.Position.Copy(context.Position);
			}

			if( model is Portfolio) {
				Portfolio portfolio = (Portfolio) model;
				double tempNetClosedEquity = 0;
				foreach( Strategy tempStrategy in portfolio.Strategies) {
					tempNetClosedEquity += tempStrategy.Performance.Equity.ClosedEquity;
					tempNetClosedEquity -= tempStrategy.Performance.Equity.StartingEquity;
				}
				double tempNetPortfolioEquity = 0;
				tempNetPortfolioEquity += portfolio.Performance.Equity.ClosedEquity;
				tempNetPortfolioEquity -= portfolio.Performance.Equity.StartingEquity;
				if( tempNetClosedEquity != tempNetPortfolioEquity) {
					int x = 0;
				}
			}
			return true;
		}
		
		private void EnterComboTradeInternal() {
			EnterComboTrade(context.Position.Price);
		}

		public void EnterComboTrade(double fillPrice) {
			TransactionPairBinary pair = TransactionPairBinary.Create();
			pair.Direction = context.Position.Current;
			pair.EntryPrice = fillPrice;
			Tick tick = model.Ticks[0];
			pair.EntryTime = tick.Time;
			pair.EntryBar = model.Chart.ChartBars.BarCount;
			comboTradesBinary.Add(pair);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnEnterTrade();
			}
		}
		
		private void ChangeComboSizeInternal() {
			TransactionPairBinary combo = comboTradesBinary.Current;
			combo.ChangeSize(context.Position.Current,context.Position.Price);
			comboTradesBinary.Current = combo;
		}
		
		private void ExitComboTradeInternal() {
			ExitComboTrade(context.Position.Price);
		}
					
		public void ExitComboTrade(double fillPrice) {
			TransactionPairBinary comboTrade = comboTradesBinary.Current;
			Tick tick = model.Ticks[0];
			comboTrade.ExitPrice = fillPrice;
			comboTrade.ExitTime = tick.Time;
			comboTrade.ExitBar = model.Chart.ChartBars.BarCount;
			comboTrade.Completed = true;
			comboTradesBinary.Current = comboTrade;
			double pnl = profitLoss.CalculateProfit(comboTrade.Direction,comboTrade.EntryPrice,comboTrade.ExitPrice);
			Equity.OnChangeClosedEquity( pnl);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnExitTrade();
			}
		}
		
		protected virtual void EnterTrade() {

		}
		
		protected virtual void ExitTrade() {
			
		}
		
		public bool OnIntervalClose()
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
		
		public Equity Equity {
			get { return equity; }
			set { equity = value; }
		}
		
		public bool WriteReport(string name, string folder) {
			name = name.StripInvalidPathChars();
			TradeStatsReport tradeStats = new TradeStatsReport(this);
			tradeStats.WriteReport(name, folder);
			StrategyStats stats = new StrategyStats(ComboTrades);
			Equity.WriteReport(name,folder,stats);
			IndexForReport index = new IndexForReport(this);
			index.WriteReport(name, folder);
			return true;
		}

		public double Slippage {
			get { return profitLoss.Slippage; }
			set { profitLoss.Slippage = value; }
		}
		
		
		public List<double> PositionChanges {
			get { return positionChanges; }
		}

		public double Commission {
			get { return profitLoss.Commission; }
			set { profitLoss.Commission = value; }
		}
		
		public PositionCommon Position {
			get { return position; }
			set { position = value; }
		}
		
#region Obsolete Methods		
		
		[Obsolete("Use WriteReport(name,folder) instead.",true)]
		public void WriteReport(string name,StreamWriter writer) {
			throw new NotImplementedException();
		}

		[Obsolete("Please use ComboTrades instead.",true)]
    	public TransactionPairs TransactionPairs {
			get { return null; }
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
			get { 
				if( comboTradesBinary.Count > 0) {
					comboTradesBinary.Current.TryUpdate(model.Ticks[0]);
				}
				return comboTrades;
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
			return new StrategyStats(ComboTrades);
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
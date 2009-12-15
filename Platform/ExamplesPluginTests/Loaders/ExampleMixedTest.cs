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
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 5/25/2009
 * Time: 3:36 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion


using System;
using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;

namespace Loaders
{
	[TestFixture]
	public class ExampleMixedTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
		
		Portfolio multiSymbolPortfolio;			
		Portfolio singleSymbolPortfolio;			
   		Strategy exampleSimple;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			TimeStamp.SetCustomUtcOffset(-4);
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    	starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    	starter.DataFolder = "TestData";
	    	starter.ProjectProperties.Starter.Symbols = "FullTick,Daily4Sim";
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
			
			// Set up chart
	    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    	starter.ShowChartCallback = null;
	
			// Run the loader.
			ExampleMixedLoader loader = new ExampleMixedLoader();
    		starter.Run(loader);

 			ShowChartCallback showChartCallback = new ShowChartCallback(HistoricalShowChart);
 			showChartCallback();
 
 			// Get the stategy
    		multiSymbolPortfolio = loader.TopModel as Portfolio;
    		fullTickData = multiSymbolPortfolio.Strategies[0] as ExampleOrderStrategy;
    		singleSymbolPortfolio = multiSymbolPortfolio.Strategies[1] as Portfolio;
    		fourTicksPerBar = singleSymbolPortfolio.Strategies[0] as ExampleOrderStrategy;
    		exampleSimple = singleSymbolPortfolio.Strategies[1] as ExampleSimpleStrategy;
		}
		
		[TestFixtureTearDown]
		public void FixtureTearDown() {
			TimeStamp.ResetUtcOffset();
		}
		#endregion
		
		[Test] 
		public void TestSingleSymbolPortfolio() {
			double expectedEquity = 0;
			expectedEquity += fourTicksPerBar.Performance.Equity.CurrentEquity;
			expectedEquity -= fourTicksPerBar.Performance.Equity.StartingEquity;
			expectedEquity += exampleSimple.Performance.Equity.CurrentEquity;
			expectedEquity -= exampleSimple.Performance.Equity.StartingEquity;
			
			double portfolioEquity = singleSymbolPortfolio.Performance.Equity.CurrentEquity;
			portfolioEquity -= singleSymbolPortfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expectedEquity,4),Math.Round(portfolioEquity,4));
			Assert.AreEqual(Math.Round(-297800.00D,4),Math.Round(portfolioEquity,4));
		}
		
		[Test] 
		public void TestMultiSymbolPortfolio() {
			double expectedEquity = 0;
			expectedEquity += fullTickData.Performance.Equity.CurrentEquity;
			expectedEquity -= fullTickData.Performance.Equity.StartingEquity;
			expectedEquity += singleSymbolPortfolio.Performance.Equity.CurrentEquity;
			expectedEquity -= singleSymbolPortfolio.Performance.Equity.StartingEquity;
			
			double portfolioEquity = multiSymbolPortfolio.Performance.Equity.CurrentEquity;
			portfolioEquity -= multiSymbolPortfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expectedEquity,4),Math.Round(portfolioEquity,4));
			Assert.AreEqual(Math.Round(-372600.00,4),Math.Round(portfolioEquity,4));
		}
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
			Assert.AreEqual(472,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareAllPairs() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			for( int i=0; i<fourTicksRTs.Count && i<fullTicksRTs.Count; i++) {
				TransactionPair fourRT = fourTicksRTs[i];
				TransactionPair fullRT = fullTicksRTs[i];
				double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
				double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
				Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
				Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
			}
		}
		
		[Test]
		public void RoundTurn1() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i=1;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void RoundTurn2() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i=2;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void LastRoundTurn() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i = fourTicksRTs.Current;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			TimeStamp fourExitTime = fourRT.ExitTime;
			TimeStamp fullExitTime = fullRT.ExitTime;
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
			Assert.AreEqual(fourExitTime,fourRT.ExitTime,"Exit Time for Trade #" + i);
			Assert.AreEqual(fullExitTime,fullRT.ExitTime,"Exit Time for Trade #" + i);
			Assert.AreEqual(new TimeStamp("1989-12-29 15:59:00.050"),fullRT.ExitTime,"Exit Time for Trade #" + i);
		}
		
		[Test]
		public void CompareBars0() {
			CompareChart(fullTickData,GetChart(fullTickData.SymbolDefault));
		}
		
		[Test]
		public void CompareBars1() {
			CompareChart(fourTicksPerBar,GetChart(fourTicksPerBar.SymbolDefault));
		}
	}
}

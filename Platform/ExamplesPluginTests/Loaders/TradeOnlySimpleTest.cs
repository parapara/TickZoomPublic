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
using ZedGraph;

namespace Loaders
{
	[TestFixture]
	public class TradeOnlySimpleTest : StrategyTest
	{
		#region SetupTest
		Strategy strategy;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2009,8,4);
    		starter.DataFolder = "TestData";
    		starter.ProjectProperties.Starter.Symbols = "TXF";
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
			
    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
    		
    		// Run the loader.
			ExampleSimpleLoader loader = new ExampleSimpleLoader();
    		starter.Run(loader);

    		// Get the stategy
    		strategy = loader.TopModel as ExampleSimpleStrategy;
		}
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual( 21,strategy.Performance.TransactionPairs.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "2009-08-02 23:14:01.001", 6950.000,
			                 "2009-08-02 23:21:01.001",6945.000);
		}

		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "2009-08-02 23:21:01.001", 6945.000,
			                 "2009-08-02 23:29:01.001", 6955.000);
		}
		
		[Test]
		public void VerifyBarData() {
			Bars bars = strategy.Bars;
			Assert.AreEqual( 153, bars.BarCount);
		}
		
		[Test]
		public void VerifyChartData() {
			Assert.AreEqual(1,ChartCount);
			ChartControl chart = GetChart(0);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		Assert.AreEqual(153,pane.CurveList[0].Points.Count);
		}
		
		[Test]
		public void CompareChart() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}

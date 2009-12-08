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
	public class ExampleSimpleTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(typeof(ExampleSimpleTest));
		StrategyCommon strategy;
		private string symbols = "Daily4Sim";
		
		public string Symbols {
			get { return symbols; }
			set { symbols = value; }
		}
		
		public virtual Starter CreateStarter() {
			return new HistoricalStarter();			
		}
			
		[TestFixtureSetUp]
		public virtual void RunStrategy() {
			try {
				TimeStamp.SetCustomUtcOffset(-4);
				Starter starter = CreateStarter();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "TestData";
	    		starter.ProjectProperties.Starter.Symbols = symbols;
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				starter.ProjectProperties.Engine.RealtimeOutput = false;
				
	    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
	    		
	    		// Run the loader.
				ExampleSimpleLoader loader = new ExampleSimpleLoader();
	    		starter.Run(loader);
	
	    		// Get the stategy
	    		strategy = loader.TopModel as ExampleSimpleStrategy;
			} catch( Exception ex) {
				log.Error( "Setup failed.", ex);
				throw;
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown() {
			TimeStamp.ResetUtcOffset();
		}
		
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual( 378,strategy.Performance.TransactionPairs.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "1983-04-06 09:00:00.000", 29.90,
			                 "1983-04-18 09:00:00.000",30.560);

		}

		[Test]
		public void LastTradeTest() {
			VerifyPair( strategy, strategy.Performance.Trades.Count-1, "1989-12-29 09:00:00.000", 21.67,
			                 "1989-12-29 09:00:00.003",21.82);
		}
		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "1983-04-18 09:00:00.000", 30.56,
			                 "1983-04-19 09:00:00.000", 30.700);
		}
		
		[Test]
		public void VerifyBarData() {
			Bars days = strategy.Data.Get(Intervals.Day1);
			Assert.AreEqual( 1696, days.BarCount);
		}
		
		[Test]
		public void VerifyChartData() {
			Assert.AreEqual(1,ChartCount);
			ChartControl chart = GetChart(0);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		Assert.AreEqual(1696,pane.CurveList[0].Points.Count);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}

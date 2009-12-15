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
	public class ExampleDualSymbolTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
		Portfolio portfolio;
		string symbols = "FullTick,Daily4Sim";
		
		public virtual Starter CreateStarter() {
			return new HistoricalStarter();
		}
		
		[TestFixtureSetUp]
		public virtual void RunStrategy() {
			try {
				Starter starter = CreateStarter();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "TestData";
	    		starter.ProjectProperties.Starter.Symbols = symbols;
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				starter.ProjectProperties.Engine.RealtimeOutput = false;
				
				// Set up chart
		    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = null;
	
				// Run the loader.
				ExampleDualSymbolLoader loader = new ExampleDualSymbolLoader();
	    		starter.Run(loader);
	
	 			ShowChartCallback showChartCallback = new ShowChartCallback(HistoricalShowChart);
	 			showChartCallback();
	 
	 			// Get the stategy
	    		portfolio = loader.TopModel as Portfolio;
	    		fullTickData = portfolio.Strategies[0] as ExampleOrderStrategy;
	    		fourTicksPerBar = portfolio.Strategies[1] as ExampleOrderStrategy;
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw;
			}
		}
		#endregion
		
		[Test]
		public void CheckPortfolio() {
			Assert.AreEqual(-139600, portfolio.Performance.Equity.CurrentEquity);
		}
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
			Assert.AreEqual(472,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareAllRoundTurns() {
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
		public void CompareBars0() {
			CompareChart(fullTickData,GetChart(fullTickData.SymbolDefault));
		}
		
		[Test]
		public void CompareBars1() {
			CompareChart(fourTicksPerBar,GetChart(fourTicksPerBar.SymbolDefault));
		}
		
		public string Symbols {
			get { return symbols; }
			set { symbols = value; }
		}
	}

}

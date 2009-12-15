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
	public class CompareSimulatedTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Starter fullTickStarter;
		Starter fourTickStarter;
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			try {
				fullTickStarter = new HistoricalStarter(false);
				
				// Set run properties as in the GUI.
				fullTickStarter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		fullTickStarter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		fullTickStarter.DataFolder = "TestData";
	    		fullTickStarter.ProjectProperties.Starter.Symbols = "FullTick";
				fullTickStarter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				
				// Run the loader.
				ExampleOrdersLoader loader = new ExampleOrdersLoader();
	    		fullTickStarter.Run(loader);
	
	    		// Get the stategy
	    		fullTickData = loader.TopModel as ExampleOrderStrategy;
	    		
	    		/// <summary>
	    		/// Now run the other strategy to compare results.
	    		/// </summary>
	    		
				fourTickStarter = new HistoricalStarter(false);
				
				// Set run properties as in the GUI.
				fourTickStarter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		fourTickStarter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		fourTickStarter.DataFolder = "TestData";
	    		fourTickStarter.ProjectProperties.Starter.Symbols = "Daily4Sim";
				fourTickStarter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				
				// Run the loader.
				ExampleSimulatedLoader simulatedLoader = new ExampleSimulatedLoader();
	    		fourTickStarter.Run(simulatedLoader);
	
	    		// Get the stategy
	    		fourTicksPerBar = simulatedLoader.TopModel as ExampleOrderStrategy;
			} catch( Exception ex) {
				Console.Out.WriteLine(ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
				throw;
			}
    		
		}
		
		[TestFixtureTearDown]
		public void FixtureTearDown() {
			fourTickStarter.Release();
			fullTickStarter.Release();
		}
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareTrades() {
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
		
	}
}

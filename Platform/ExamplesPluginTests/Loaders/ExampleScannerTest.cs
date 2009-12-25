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

namespace Loaders
{
	[TestFixture]
	public class ExampleScannerTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Strategy strategy;
		ExampleScannerStrategy scanner;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,5,28);
    		starter.DataFolder = "TestData";
    		starter.ProjectProperties.Starter.Symbols = "FullTick,Daily4Sim"; 
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
			
			// Run the loader.
			ExampleScannerLoader loader = new ExampleScannerLoader();
    		starter.Run(loader);

    		// Get the stategy
    		scanner = loader.TopModel as ExampleScannerStrategy;
    		strategy = scanner.Markets[0];
		}
		#endregion
		
		[Test]
		public void RoundTurn0() {
			VerifyRoundTurn( 0, "1983-04-06 15:59:00.001", 29.900,
			                 "1983-04-12 15:59:00.001",30.25);
		}
		
		[Test]
		public void RoundTurn1() {
			VerifyRoundTurn( 1, "1983-04-13 15:59:00.001", 30.600,
			                 "1983-04-14 15:59:00.001", 30.82);
		}
		
		[Test]
		public void RoundTurn2() {
			VerifyRoundTurn( 2, "1983-04-18 15:59:00.001", 30.560,
			                 "1983-04-19 15:59:00.001",30.700);
		}
		
		[Test]
		public void CheckSymbols() {
			Assert.AreEqual(scanner.Markets.Count,2);
			Assert.AreEqual(scanner.Markets[0].SymbolDefault,"FullTick");
			Assert.AreEqual(scanner.Markets[1].SymbolDefault,"Daily4Sim");
		}
		
		#region Verify Pairs
		public void VerifyRoundTurn(int pairNum,
		                       string expectedEntryTime,
		                     double expectedEntryPrice,
		                      string expectedExitTime,
		                     double expectedExitPrice)
		{
    		TransactionPairs pairs = strategy.Performance.ComboTrades;
    		Assert.Greater(pairs.Count,pairNum);
    		TransactionPair pair = pairs[pairNum];
    		TimeStamp expEntryTime = new TimeStamp(expectedEntryTime);
    		Assert.AreEqual( expEntryTime, pair.EntryTime, "Pair " + pairNum + " Entry");
    		Assert.AreEqual( expectedEntryPrice, pair.EntryPrice, "Pair " + pairNum + " Entry");
    		
    		Assert.AreEqual( new TimeStamp(expectedExitTime), pair.ExitTime, "Pair " + pairNum + " Exit");
    		Assert.AreEqual( expectedExitPrice, pair.ExitPrice, "Pair " + pairNum + " Exit");
    		
    		double direction = pair.Direction;
		}
	}
	#endregion
}

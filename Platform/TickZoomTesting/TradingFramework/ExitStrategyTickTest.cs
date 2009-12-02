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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;

//using mscoree;







#if TESTING
namespace TickZoom.TradingFramework
{

	
	[TestFixture]
	public class ExitStrategyTickTest : MarshalByRefObject
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(ExitStrategyTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		ExitStrategyMock exitStrategy;
		
    	[TestFixtureSetUp]
    	public virtual void Init() {
    		TimeStamp.SetToUtcTimeZone();
			log.Notice("Setup ExitStrategyTest");
			TickProcessing();
    	}
    	
    	[TestFixtureTearDown]
    	public void Dispose() {
    		TimeStamp.ResetUtcOffset();
    	}
		
		public void TickProcessing() {
			Strategy random = new RandomCommon();
			exitStrategy = new ExitStrategyMock(random);
			exitStrategy.IntervalDefault = Intervals.Day1;
			random.IntervalDefault = Intervals.Day1;
			random.ExitStrategy = exitStrategy;
			random.Performance.IntervalDefault = Intervals.Day1;
			Starter starter = new HistoricalStarter();
			starter.EndCount = 2047;
			starter.ProjectProperties.Starter.Symbols = "USD_JPY_YEARS";
			starter.DataFolder = "TestData";
			starter.Run(random);
			
			Assert.AreEqual(exitStrategy,random.ExitStrategy);
			Assert.AreEqual(exitStrategy.tradeSignalTest,random.ExitStrategy.Position);
		}
	
		[Test]
		public void LongEntry() {
			TimeStamp expected = new TimeStamp(2004,1,2,9,53,24,931);
			Assert.Greater(exitStrategy.signalChanges.Count,4,"Number of signal Changes");
			Assert.AreEqual(expected,exitStrategy.signalChanges[2],"Long entry time");
			Assert.AreEqual(1,exitStrategy.signalDirection[2],"Long entry signal");
		}
			
		[Test]
		public void FlatEntry() {
			TimeStamp expected = new TimeStamp(2004,1,2,9,57,27,901);
			Assert.Greater(exitStrategy.signalDirection.Count,5,"count of signal direction");
			Assert.AreEqual(0,exitStrategy.signalDirection[3],"Flat entry signal");
			Assert.AreEqual(expected,exitStrategy.signalChanges[3],"Flat entry time");
		}
			
		[Test]
		public void ShortEntry() {
			TimeStamp expected = new TimeStamp(2004,1,2,10,40,04,991);
			Assert.Greater(exitStrategy.signalDirection.Count,10,"count of signal direction");
			Assert.AreEqual(-1,exitStrategy.signalDirection[8],"Short entry signal");
			Assert.AreEqual(expected,exitStrategy.signalChanges[8],"Short entry time");
		}
	}
}
#endif

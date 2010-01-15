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
	public class WrongSideOrderTest : MarshalByRefObject
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(WrongSideOrderTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		ExitStrategyMock exitStrategy;
		
	    [TestFixtureSetUp]
	    public virtual void Init() {
	    	TimeStamp.SetToUtcTimeZone();
			log.Notice("Setup ExitStrategyTest");
			StopTestProcessing();
	    }
	    	
	    [TestFixtureTearDown]
	    public void Dispose() {
	    	TimeStamp.ResetUtcOffset();
	    }
		
		public void StopTestProcessing() {
			RandomMock random = new RandomMock();
			exitStrategy = new ExitStrategyMock(random);
			random.ExitStrategy = exitStrategy;
			
			Starter starter = new HistoricalStarter();
			starter.StartCount = 484500;
			starter.EndCount = starter.StartCount + 11000;
			starter.ProjectProperties.Starter.Symbols = "USD_JPY_YEARS";
			starter.DataFolder = "TestData";
			starter.Run(random);
			
			exitStrategy.TickConsoleWrite();
		}

	    [Test]
		public void LongStopTest() {
			TimeStamp expected = new TimeStamp("2005-02-10 09:57:58.364");
			Assert.AreEqual(0,exitStrategy.signalDirection[41],"Long stop exit");
			Assert.AreEqual(expected,exitStrategy.signalChanges[41],"Long stop exit");
		}
	
		[Test]
		public void ShortStopTest() {
			TimeStamp expected = new TimeStamp("2005-02-09 09:52:18.097");
			Assert.AreEqual(0,exitStrategy.signalDirection[13],"Short stop exit");
			Assert.AreEqual(expected,exitStrategy.signalChanges[13],"Short stop exit");
		}
		
	}
}
#endif

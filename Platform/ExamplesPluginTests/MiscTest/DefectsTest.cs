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

using Loaders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Logging;

#if REALTIME

#endif


namespace MiscTest
{
	[TestFixture]
	public class DefectsTest : StrategyTest
	{
	    string storageFolder;
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	    public delegate void ShowChartDelegate(ChartControl chart);
		
	    public DefectsTest() {
    		storageFolder = Factory.Settings["AppDataFolder"];
   			if( storageFolder == null) {
       			throw new ApplicationException( "Must set AppDataFolder property in app.config");
   			}
	    }
		[SetUp]
		public void TestSetup() {
			TimeStamp.SetCustomUtcOffset(-4);
		}
		
		[TearDown]
		public void TearDown() {
			TimeStamp.ResetUtcOffset();
		}
		
		[Test]
		public void Ticket123()
		{
			Starter starter = new HistoricalStarter();
    		starter.DataFolder = "TestData";
    		starter.ProjectProperties.Starter.Symbols = "spyTestBars";
			Interval intervalDefault = Intervals.Minute1;
			starter.ProjectProperties.Starter.IntervalDefault = intervalDefault;
			
			// No charting setup for these tests. Running without charts.
    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
		
			ModelLoaderInterface loader = new MQ_BadFakeTickLoader();
    		starter.Run(loader);
    		Portfolio strategy = loader.TopModel as Portfolio;
    		
    		MQ_BadFakeTick_0 mq0 = (MQ_BadFakeTick_0) strategy.Strategies[0];
    		MQ_BadFakeTick_1 mq1 = (MQ_BadFakeTick_1) strategy.Strategies[1];
    		MQ_BadFakeTick_2 mq2 = (MQ_BadFakeTick_2) strategy.Strategies[2];
    		Assert.AreEqual(26.00,mq0.Performance.ComboTrades.CalcProfitLoss(0));
    		Assert.AreEqual(-27.5,mq1.Performance.ComboTrades.CalcProfitLoss(0));
    		Assert.AreEqual(-14.5,mq2.Performance.ComboTrades.CalcProfitLoss(0));
    		Assert.AreEqual(-9.00,strategy.Performance.ComboTrades.CalcProfitLoss(0));
    		Assert.AreEqual(-16.00,strategy.Performance.Equity.CurrentEquity-strategy.Performance.Equity.StartingEquity);
 
		}
	}
}


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
using TickZoom.TickUtil;

#if TESTING
namespace TickZoom.TradingFramework
{
	[TestFixture]
	public class PortfolioSupportTest
	{
		Portfolio portfolio;
		
		[TestFixtureSetUp]
		public void Init()
		{
	        // If DataSet's timeframe's don't exactly match the ones
	        // already in the tickArray then the tick bits won't match
	        // and none of the time frames will work.
//	        foreach( Period timeFrame in tickArray.TimeFrames) {
//				data.AddBarPeriod( timeFrame);
//	        }
//	        data.InitializeTick(tickArray[0]);
		}
		[TestFixtureTearDown]
		public void Dispose()
		{
		}
		
		public void Constructor()
		{
			portfolio = new Portfolio();
		}
		
		[Test]
		public void InitializeTick() 
		{
			Constructor();
			Strategy strategy = new Strategy();
			strategy.IntervalDefault = Intervals.Day1;
			portfolio.IntervalDefault = Intervals.Day1;
			portfolio.OnBeforeInitialize();
			Assert.AreSame(strategy,strategy.ExitStrategy.Chain.Next.Model,"Strategy property");
		}
	
	}
}
#endif
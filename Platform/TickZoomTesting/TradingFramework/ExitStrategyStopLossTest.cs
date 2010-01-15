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
	public class ExitStrategyStopLossTest : MarshalByRefObject
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(ExitStrategyStopLossTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		ExitStrategyMock exitStrategy;
		
    	[SetUp]
    	public virtual void Init() {
    		TimeStamp.SetToUtcTimeZone();
			log.Notice("Setup ExitStrategyTest");
    	}
    	
    	[TearDown]
    	public void Dispose() {
    		TimeStamp.ResetUtcOffset();
    	}
		
		public void ExitTickProcessing(int stop, int target) {
			Strategy random = new RandomCommon();
			exitStrategy = new ExitStrategyMock(random);
			random.ExitStrategy = exitStrategy;
			exitStrategy.StopLoss = stop;
			exitStrategy.TargetProfit = target;
			
			Starter starter = new HistoricalStarter();
			starter.StartCount = 484500;
			starter.EndCount = starter.StartCount + 21000;
			starter.ProjectProperties.Starter.Symbols = "USD_JPY_YEARS";
			starter.DataFolder = "TestData";
			starter.Run(random);
			
//			strategy.TickConsoleWrite();

//			Trade Times BEFORE applying stops or target
//			0: 1: time: 7/22/2004 8:16:37 AM bid: 109500 ask: 109500
//			1: 0: time: 7/22/2004 8:17:22 AM bid: 109500 ask: 109500
//			2: 1: time: 7/22/2004 8:53:46 AM bid: 109500 ask: 109500
//			3: 0: time: 7/22/2004 9:13:55 AM bid: 109440 ask: 109440
//			4: 1: time: 7/22/2004 9:52:09 AM bid: 109510 ask: 109510
//			5: 0: time: 7/22/2004 10:15:41 AM bid: 109480 ask: 109480
//			6: 1: time: 7/22/2004 10:17:15 AM bid: 109460 ask: 109460
//			7: 0: time: 7/22/2004 10:25:51 AM bid: 109430 ask: 109430
//			8: -1: time: 7/22/2004 10:39:44 AM bid: 109300 ask: 109300
//			9: 0: time: 7/22/2004 10:50:38 AM bid: 109440 ask: 109440
//			10: -1: time: 7/22/2004 11:56:52 AM bid: 109650 ask: 109650
//			11: 0: time: 7/23/2004 8:59:47 AM bid: 110060 ask: 110060
//			12: 1: time: 7/23/2004 9:15:11 AM bid: 110040 ask: 110040
//			13: -1: time: 7/23/2004 9:27:48 AM bid: 110030 ask: 110030
//			14: 1: time: 7/23/2004 9:50:06 AM bid: 110040 ask: 110040
//			15: -1: time: 7/23/2004 10:06:45 AM bid: 110030 ask: 110030
//			16: 1: time: 7/23/2004 10:12:18 AM bid: 110010 ask: 110010
//			17: 0: time: 7/23/2004 10:13:04 AM bid: 109980 ask: 109980
//			18: -1: time: 7/23/2004 10:22:03 AM bid: 110030 ask: 110030
//			19: 1: time: 7/23/2004 10:27:12 AM bid: 110020 ask: 110020
//			20: 0: time: 7/23/2004 10:40:50 AM bid: 109990 ask: 109990
//			21: 1: time: 7/23/2004 10:46:57 AM bid: 109990 ask: 109990
//			22: -1: time: 7/23/2004 11:34:50 AM bid: 110060 ask: 110060
//			23: 1: time: 7/23/2004 11:40:56 AM bid: 109970 ask: 109970
//			24: -1: time: 7/23/2004 11:57:13 AM bid: 110050 ask: 110050
//			25: 1: time: 7/23/2004 11:59:46 AM bid: 110080 ask: 110080
//			26: 0: time: 7/23/2004 12:00:06 PM bid: 110100 ask: 110100
//			27: -1: time: 7/26/2004 8:13:19 AM bid: 109490 ask: 109490
//			28: 0: time: 7/26/2004 8:35:04 AM bid: 109590 ask: 109590
//			29: -1: time: 7/26/2004 8:35:58 AM bid: 109620 ask: 109620
//			30: 0: time: 7/26/2004 9:38:02 AM bid: 109750 ask: 109750
//			31: 1: time: 7/26/2004 9:55:23 AM bid: 109790 ask: 109790
//			32: 0: time: 7/26/2004 9:57:19 AM bid: 109780 ask: 109780
//			33: 1: time: 7/26/2004 9:57:53 AM bid: 109770 ask: 109770
//			34: -1: time: 7/26/2004 10:14:47 AM bid: 109890 ask: 109890
//			35: 1: time: 7/26/2004 11:06:06 AM bid: 109960 ask: 109960
//			36: 0: time: 7/26/2004 11:09:28 AM bid: 110000 ask: 110000
//			37: -1: time: 7/26/2004 11:16:47 AM bid: 109960 ask: 109960
//			38: 0: time: 7/26/2004 11:29:24 AM bid: 110030 ask: 110030
//			39: 1: time: 7/26/2004 11:39:28 AM bid: 110070 ask: 110070
//			40: 0: time: 7/27/2004 8:13:56 AM bid: 109790 ask: 109790
		}
		
		[Test]
		public void Stop_Exit() {
			ExitTickProcessing(50,0);
			TimeStamp expected = new TimeStamp("2005-02-08 12:59:26.683");
			Assert.AreEqual(0d,exitStrategy.signalDirection[3]);
			Assert.AreEqual(expected,exitStrategy.signalChanges[3]);
		}
		
//		public static void ListAppDomains() {
//			IList<AppDomain> list = GetAppDomains();
//			Console.WriteLine();
//			Console.WriteLine();
//			for( int i=0; i<list.Count; i++) {
//				Console.WriteLine("AppDomains: " + list[i].FriendlyName);
//			}
//			Console.WriteLine();
//		}

//	    public static IList<AppDomain> GetAppDomains()
//	    {
//	        IList<AppDomain> _IList = new List<AppDomain>();
//	        IntPtr enumHandle = IntPtr.Zero;
//	        CorRuntimeHostClass host = new mscoree.CorRuntimeHostClass();
//	
//	        try
//	        {
//	            host.EnumDomains(out enumHandle);
//	            object domain = null;
//	
//	            while (true)
//	            {
//	                host.NextDomain(enumHandle, out domain);
//	                if (domain == null) break;
//	                AppDomain appDomain = (AppDomain)domain;
//	                _IList.Add(appDomain);
//	            }
//	            return _IList;
//	        }
//	        catch (Exception e)
//	        {
//	            Console.WriteLine(e.ToString());
//	            return null;
//	        }
//	        finally
//	        {
//	            host.CloseEnum(enumHandle);
//	            Marshal.ReleaseComObject(host);
//	        }
//	    }
		[Test]
		public void Target_Exit() {
			ExitTickProcessing(0,100);
			TimeStamp expected = new TimeStamp("2005-02-09 10:22:07.358");
			Assert.AreEqual(expected,exitStrategy.signalChanges[16],"Target_Exit");
			Assert.AreEqual(-1d,exitStrategy.signalDirection[16],"Target_Exit");
		}
	}
}
#endif

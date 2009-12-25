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
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MBTProvider;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.MBTrading;
using TickZoom.TickUtil;

namespace TickZoom.Test
{
	[TestFixture]
	public class EquityMBTrading
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(EquityMBTrading));
		private static readonly bool debug = log.IsDebugEnabled;		
		Provider provider;
		protected SymbolInfo symbol;
		
		[TestFixtureSetUp]
		public virtual void Init()
		{
			string appData = Factory.Settings["AppDataFolder"];
			File.Delete( appData + @"\Logs\MBTradingTests.log");
			File.Delete( appData + @"\Logs\MBTradingService.log");
  			symbol = Factory.Symbol.LookupSymbol("MSFT");
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
		}
		
		[Test]
		public void DemoConnectionTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
  			provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			if(debug) log.Debug("===StartSymbol===");
			VerifyFeed verify = new VerifyFeed();
			provider.Start(verify);
  			 provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.Stop(verify);	
		}
		
		[Test]		
		public void TestAssemblyName() {
			AssemblyName assemblyName = AssemblyName.GetAssemblyName("MBTradingTests.dll");		
			assemblyName = AssemblyName.GetAssemblyName("MBTradingService.exe");		
		}
		
		[Test]
		public void TestSeperateProcess() {
			provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			VerifyFeed verify = new VerifyFeed();
			provider.Start(verify);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.Stop(verify);
  			Process[] processes = null;
  			for(int i=0;i<20;i++) {
  				processes = Process.GetProcessesByName("MBTradingService");
  				Thread.Sleep(1000);
  				if( processes.Length == 0) break;
  			}
  			Assert.AreEqual(0,processes.Length,"Number of MBTradingService processes.");
		}
		
		[Test]		
		public void DemoStopSymbolTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
 			provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			if(debug) log.Debug("===StartSymbol===");
			VerifyFeed verify = new VerifyFeed();
			provider.Start(verify);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
			if(debug) log.Debug("===StopSymbol===");
  			provider.StopSymbol(verify,symbol);
  			count = verify.Verify(AssertTick,symbol,25);
  			Assert.AreEqual(count,0,"tick count");
  			provider.Stop(verify);
		}

		[Test]
		public void DemoReConnectionTest() {
  			provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			VerifyFeed verify = new VerifyFeed();
			provider.Start(verify);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.Stop(verify);
  			provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			verify = new VerifyFeed();
			provider.Start(verify);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
  			count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.Stop(verify);
		}

		public void AssertTick( TickIO tick, TickIO lastTick, ulong symbol) {
        	Assert.Greater(tick.Bid,0);
        	Assert.Greater(tick.Ask,0);
    		Assert.IsTrue(tick.Time>=lastTick.Time,"tick.Time > lastTick.Time");
    		Assert.AreEqual(symbol,tick.lSymbol);
		}
	}
}

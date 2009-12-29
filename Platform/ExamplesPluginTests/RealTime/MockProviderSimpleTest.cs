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
using System.Configuration;
using Loaders;
using NUnit.Framework;
using System.IO;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using ZedGraph;

namespace RealTime
	
{
#if !PROVIDER
	[TestFixture]
	public class MockProviderSimpleTest : ExampleSimpleTest {
		public override Starter CreateStarter()
		{
			return new RealTimeStarter();
		}
		[TestFixtureSetUp]
		public override void RunStrategy()
		{
			ConfigurationManager.AppSettings.Set("TestProviderCutOff-Mock4Sim","1984-12-31 00:00:00.000");
			DeleteFiles();
			base.Symbols="Mock4Sim";
			base.RunStrategy();
		}
		
		[Test]
		public void VerifyChanges() {
			TestPositionChange[] changes = ReadPositionChange();
			System.Collections.Generic.List<double> expectedChanges = strategy.Performance.PositionChanges;
			Assert.AreEqual(expectedChanges.Count,changes.Length,"length");
			for( int i=0; i<expectedChanges.Count; i++) {
				Assert.AreEqual(expectedChanges[i],changes[i].Position,"at index " + i);
			}
		}
	
		public class TestPositionChange { 
			public string Symbol;
			public double Position;
		}
		
		private TestPositionChange[] ReadPositionChange() {
			System.Collections.Generic.List<TestPositionChange> changes = new System.Collections.Generic.List<TestPositionChange>();
			string appDataFolder = Factory.Settings["AppDataFolder"];
			string path = appDataFolder + @"\MockProviderData\"+base.Symbols+"_Trades.txt";
			StreamReader re = File.OpenText(path);
			char[] seperators = new Char[] { ',' };
			while( !re.EndOfStream) {
				string line = re.ReadLine();
				string[] fields = line.Split(seperators);
				TestPositionChange change = new TestPositionChange();
				if( fields.Length > 0) {
					change.Symbol = fields[0];
				}
				if( fields.Length > 1) {
					change.Position = double.Parse(fields[1]);
				}
				changes.Add(change);
			}
			re.Close();
			return changes.ToArray();
		}
		
		private void DeleteFiles() {
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\TestServerCache\Mock4Sim_Tick.tck");
		 			File.Delete( appData + @"\TestServerCache\ESZ9_Tick.tck");
		 			File.Delete( appData + @"\TestServerCache\IBM_Tick.tck");
		 			File.Delete( appData + @"\TestServerCache\GBPUSD_Tick.tck");
					break;
				} catch( Exception) {
				}
			}
		}
	}
#endif
}

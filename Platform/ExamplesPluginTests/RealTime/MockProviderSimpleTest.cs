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
		[TestFixtureSetUpAttribute()]
		public override void RunStrategy()
		{
			ConfigurationManager.AppSettings.Set("TestProviderCutOff-Daily4Sim","1984-12-31 00:00:00.000");
			base.Symbols="Mock4Sim";
			base.RunStrategy();
		}
		private void DeleteFiles() {
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\TestServerCache\Mock4Sim.tck");
					break;
				} catch( Exception) {
				}
			}
		}
	}
#endif
}

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
 * Date: 9/22/2009
 * Time: 6:24 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion


using System;
using System.Configuration;
using NUnit.Framework;
using System.IO;
using TickZoom.Api;
using tzdata;

namespace TickZoom.Utilities
{
	[TestFixture]
	public class tzdataTest
	{
		[Test]
		public void TestFilter()
		{
	       	string storageFolder = Factory.Settings["AppDataFolder"];
	       	if( storageFolder == null) {
	       		throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       	}
			string[] args = {
				storageFolder + @"\TestData\Daily4Ticks_Tick.tck",
				storageFolder + @"\TestData\Daily4Sim_Tick.tck",
			};
			Filter filter = new Filter(args);
		}
		
		[Test]
		public void TestMigrate()
		{
	       	string storageFolder = Factory.Settings["AppDataFolder"];
	       	if( storageFolder == null) {
	       		throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       	}
	       	string origFile = storageFolder + @"\TestData\Migrate_Tick.tck";
	       	string tempFile = origFile + ".temp";
	       	string backupFile = origFile + ".back";
	       	File.Delete( backupFile);
	       	File.Delete( origFile);
	       	File.Copy(storageFolder + @"\TestData\USD_JPY_Tick.tck", origFile);
	       	
	       	string[] args = { "USD/JPY", storageFolder + @"\TestData\Migrate_Tick.tck" };
	       	
	       	Migrate migrate = new Migrate(args);
			Assert.IsTrue( File.Exists( origFile));
			Assert.IsTrue( File.Exists( backupFile));
			Assert.IsFalse( File.Exists( tempFile));
		}
	}
}

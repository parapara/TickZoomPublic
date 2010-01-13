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
 * Date: 7/23/2009
 * Time: 10:00 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.IO;
using TickZoom.Api;
using TickZoom.TickUtil;

namespace tzdata
{
	public class Migrate
	{
		public Migrate(string[] args)
		{
			if( args.Length != 2) {
				Console.Write("Migrate Usage:");
				Console.Write("tzdata migrate <symbol> <file>");
				return;
			}
			MigrateFile(args[1],args[0]);
		}
		
		private void MigrateFile(string file, string symbol) {
			if( File.Exists(file + ".back")) {
				Console.WriteLine("A backup file already exists. Please delete it first at: " + file + ".back");
				return;
			}
			TickReader reader = new TickReader();
//			reader.BulkFileLoad = true;
			reader.Initialize( file, symbol);
			
			TickWriter writer = new TickWriter(true);
			writer.KeepFileOpen = true;
			writer.Initialize( file + ".temp", symbol);
			
			TickImpl firstTick = new TickImpl();
			TickIO tickIO = new TickImpl();
			TickBinary tickBinary = new TickBinary();
			int count = 0;
			bool first = false;
			try {
				while(true) {
					reader.ReadQueue.Dequeue(ref tickBinary);
					tickIO.Inject(tickBinary);
					writer.Add(tickIO);
					if( first) {
						firstTick.Copy(tickIO);
						first = false;
					}
					count++;
				}
			} catch( QueueException ex) {
				if( ex.EntryType != EntryType.EndHistorical) {
					throw new ApplicationException("Unexpected QueueException: " + ex);
				}
			}
			Console.WriteLine(reader.Symbol + ": Migrated " + count + " ticks from " + firstTick.Time + " to " + tickIO.Time );
			TickReader.CloseAll();
			writer.Close();
			File.Move( file, file + ".back");
			File.Move( file + ".temp", file);
		}
	}
}

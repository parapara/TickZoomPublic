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
	/// <summary>
	/// Description of Query.
	/// </summary>
	public class Query
	{
		public Query(string[] args)
		{
			if( args.Length != 1) {
				Console.Write("Query requires a file name argument");
			}
			string arg = args[0];
			if( Directory.Exists( arg)) {
				DirectoryInfo di = new DirectoryInfo(arg);
				FileInfo[] files = di.GetFiles("*");
				foreach(FileInfo fi in files)
				{
				    ReadFile(fi.FullName);
				}
			} else {
				ReadFile(arg);
			}
		}
		
		public void ReadFile(string filePath) {
			TickReader reader = new TickReader();
			reader.Initialize(filePath);
			TickQueue queue = reader.ReadQueue;
			TickImpl firstTick = new TickImpl();
			TickImpl lastTick = new TickImpl();
			TickImpl prevTick = new TickImpl();
			long count = 0;
			long dups = 0;
			TickIO tickIO = new TickImpl();
			TickBinary tickBinary = new TickBinary();
			queue.Dequeue(ref tickBinary);
			tickIO.init(tickBinary);
			count++;
			firstTick.Copy( tickIO);
			prevTick.Copy( tickIO);
			try {
				while(true) {
					queue.Dequeue(ref tickBinary);
					tickIO.init(tickBinary);
					count++;
					if( tickIO.Bid == prevTick.Bid && tickIO.Ask == prevTick.Ask) {
						dups++;
					}
					prevTick.Copy(tickIO);
				}
			} catch( CollectionTerminatedException) {
				
			}
			lastTick.Copy( tickIO);
			Console.WriteLine(reader.Symbol + ": " + count + " ticks from " + firstTick.Time + " to " + lastTick.Time + " " + dups + " duplicates");
			TickReader.CloseAll();
		}
	}
}

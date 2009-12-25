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
	public class Filter
	{
		public Filter(string[] args)
		{
			if( args.Length != 2 && args.Length != 4) {
				Console.Write("Filter Usage:");
				Console.Write("tzdata fromdir todir [starttimestamp endtimestamp]");
				Console.Write("tzdata fromfile tofile [starttimestamp endtimestamp]");
				return;
			}
			string from = args[0];
			string to = args[1];
			TimeStamp startTime;
			TimeStamp endTime;
			if( args.Length > 2) {
				startTime = new TimeStamp(args[2]);
				endTime = new TimeStamp(args[3]);
			} else {
				startTime = TimeStamp.MinValue;
				endTime = TimeStamp.MaxValue;
			}
			if( Directory.Exists( from) ) {
				DirectoryInfo di = new DirectoryInfo(from);
				FileInfo[] files = di.GetFiles("*");
				Directory.CreateDirectory(to);
				foreach(FileInfo fi in files)
				{
					string outputPath = to + Path.DirectorySeparatorChar + fi.Name;
				    FilterFile(fi.FullName,outputPath,startTime,endTime);
				}
			} else {
				FilterFile(from,to,startTime,endTime);
			}
		}
		
		private void FilterFile(string inputPath, string outputPath, TimeStamp startTime, TimeStamp endTime) {
			TickReader reader = new TickReader();
			TickWriter writer = new TickWriter(true);
			writer.KeepFileOpen = true;
			writer.Initialize( outputPath);
			reader.Initialize( inputPath);
			TickQueue inputQueue = reader.ReadQueue;
			TickImpl firstTick = new TickImpl();
			TickImpl lastTick = new TickImpl();
			TickImpl prevTick = new TickImpl();
			long count = 0;
			long fast = 0;
			long dups = 0;
			TickIO tickIO = new TickImpl();
			TickBinary tickBinary = new TickBinary();
			inputQueue.Dequeue(ref tickBinary);
			tickIO.init(tickBinary);
			count++;
			firstTick.Copy(tickIO);
			firstTick.IsSimulateTicks = true;
			prevTick.Copy(tickIO);
			prevTick.IsSimulateTicks = true;
			if( tickIO.Time >= startTime) {
				writer.Add(firstTick);
			}
			try {
				while(true) {
					inputQueue.Dequeue(ref tickBinary);
					tickIO.init(tickBinary);
					
					count++;
					if( tickIO.Time >= startTime) {
						if( tickIO.Time > endTime) break;
//						if( tickIO.Bid == prevTick.Bid && tickIO.Ask == prevTick.Ask) {
//							dups++;
//						} else {
//							Elapsed elapsed = tickIO.Time - prevTick.Time;
							prevTick.Copy(tickIO);
							prevTick.IsSimulateTicks = true;
//							if( elapsed.TotalMilliseconds < 5000) {
//								fast++;
//							} else {
								writer.Add(prevTick);
//							}	
//						}
					}
				}
			} catch( QueueException ex) {
				if( ex.EntryType != EntryType.EndHistorical) {
					throw new ApplicationException("Unexpected QueueException: " + ex);
				}
			}
			lastTick.Copy( tickIO);
			Console.WriteLine(reader.Symbol + ": " + count + " ticks from " + firstTick.Time + " to " + lastTick.Time + " " + dups + " duplicates, " + fast + " less than 50 ms");
			TickReader.CloseAll();
			writer.Close();
		}
	}
}

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
 * Date: 6/7/2009
 * Time: 11:34 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using TickZoom.Api;

namespace tzdata
{
	class Program
	{
		public static void Main(string[] args)
		{
			if( args.Length == 0) {
				Console.Write("tzdata Usage:");
				Console.Write("tzdata migrate <symbol> <file>");
				return;
			}
			List<string> taskArgs = new List<string>(args);
			taskArgs.RemoveAt(0); // Remove the command string.

			if( args[0] == "migrate") {
				new Migrate(taskArgs.ToArray());
			}
			if( args[0] == "filter") {
				new Filter(taskArgs.ToArray());
			}
			if( args[0] == "query") {
				new Query(taskArgs.ToArray());
			}
			
		}
	}
}
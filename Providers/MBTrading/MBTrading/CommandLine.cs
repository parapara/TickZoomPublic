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
using TickZoom.Api;

namespace MBTProvider
{
	public class CommandLine
	{
		ServiceConnection connection;
		
		public CommandLine()
		{
		}
		
		/// <summary>
		/// Run this service.
		/// </summary>
		public void OnRun(string[] args)
		{
        	if( args.Length != 1) {
        		throw new ApplicationException("Command line must have one argument of the port number on which to listen.");
        	}
        	connection.SetAddress("127.0.0.1",Convert.ToUInt16(args[0]));
        	connection.OnRun();
		}
		
		public ServiceConnection Connection {
			get { return connection; }
			set { connection = value; }
		}
	}
}

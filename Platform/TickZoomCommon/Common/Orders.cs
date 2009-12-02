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
 * Date: 12/1/2009
 * Time: 8:43 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Orders.
	/// </summary>
	public class Orders
	{
		EnterTiming enter;
		ExitTiming exit;
		
		public Orders(EnterCommon enter, ExitCommon exit)
		{
			this.enter = new EnterTiming(enter);
			this.exit = new ExitTiming(exit);
		}
		
		public EnterTiming Enter {
			get { return enter; }
		}
		
		public ExitTiming Exit {
			get { return exit; }
		}
		
		public class EnterTiming {
			EnterCommon now;
			EnterCommon nextBar;
			
			public EnterTiming( EnterCommon enter) {
				this.now = enter;
				this.nextBar = enter;
				nextBar.orders = now.orders;
			}
			
			public EnterCommon Now {
				get { return now; }
			}
			
			public EnterCommon NextBar {
				get { return nextBar; }
			}
		}
		
		public class ExitTiming {
			ExitCommon now;
			ExitCommon nextBar;
			
			public ExitTiming( ExitCommon exit) {
				this.now = exit;
				this.nextBar = exit;
			}
			
			public ExitCommon Now {
				get { return now; }
			}
			
			public ExitCommon NextBar {
				get { return nextBar; }
			}
		}
	}
}

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
	public class OrderHandlers
	{
		EnterTiming enter;
		ExitTiming exit;
		
		public OrderHandlers(EnterCommon enterNow, EnterCommon enterNextBar, ExitCommon exitNow, ExitCommon exitNextBar)
		{
			this.enter = new EnterTiming(enterNow,enterNextBar);
			this.exit = new ExitTiming(exitNow,exitNextBar);
		}
		
		public EnterTiming Enter {
			get { return enter; }
		}
		
		public ExitTiming Exit {
			get { return exit; }
		}
		
		public class EnterTiming {
			EnterCommon activeNow;
			EnterCommon nextBar;
			
			public EnterTiming( EnterCommon now, EnterCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public EnterCommon ActiveNow {
				get { return activeNow; }
			}
			
			public EnterCommon NextBar {
				get { return nextBar; }
			}
		}
		
		public class ExitTiming {
			ExitCommon activeNow;
			ExitCommon nextBar;
			
			public ExitTiming( ExitCommon now, ExitCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public ExitCommon ActiveNow {
				get { return activeNow; }
			}
			
			public ExitCommon NextBar {
				get { return nextBar; }
			}
		}
	}
}

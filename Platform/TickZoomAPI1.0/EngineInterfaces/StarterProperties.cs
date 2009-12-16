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
 * Date: 4/4/2009
 * Time: 8:36 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of StarterProperties.
	/// </summary>
	public interface StarterProperties
	{
		[Obsolete("In loaders, lease use SymbolProperties instead so you can override properties from the dictionary. The read only SymbolInfo is used everywhere else.",true)]
		SymbolInfo[] SymbolInfo {
			get;
		}
		
		ISymbolProperties[] SymbolProperties {
			get;
		}
		
		TimeStamp StartTime {
			get;
			set;
		}
		
		TimeStamp EndTime {
			get;
			set;
		}
		
		Interval IntervalDefault {
			get;
			set;
		}
		
		string Symbols {
			set;
		}
		
		void CopyProperties(object obj);
	}
}

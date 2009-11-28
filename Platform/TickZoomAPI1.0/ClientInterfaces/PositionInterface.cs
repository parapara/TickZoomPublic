#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace TickZoom.Api
{
	[Obsolete("Please use PositionInterface instead.",true)]
	public interface Position : PositionInterface {
		
	}
	
	public interface PositionInterface {
		[Obsolete("Please use Current instead.",true)]
		double Signal {
			get;
			set;
		}
		
		double Current {
			get;
		}
		
		void Change( double position);
		
		void Change( double position, double price, TimeStamp time);
		
		[Obsolete("Please use Price instead.",true)]
		double SignalPrice {
			get;
		}
		
		double Price {
			get;
		}
		
		[Obsolete("Please use Time instead.",true)]
		TimeStamp SignalTime {
			get;
		}
		
		TimeStamp Time {
			get;
		}
		
		bool HasPosition {
			get;
		}
		
		bool IsFlat {
			get;
		}
		
		bool IsLong {
			get;
		}
		
		bool IsShort {
			get;
		}
		
		double Size {
			get;
		}
		
	}
}

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
using System.IO;

namespace TickZoom.Api
{

	/// <summary>
	/// Description of TickDOM.
	/// </summary>
	public interface TickIO : Tick, ReadWritable<TickBinary>
	{
		void init(TickIO tick, byte contentMask);
		
		void init(TimeStamp utcTime, double dBid, double dAsk);

		void init(TimeStamp utcTime, double price, int size);

		void init(TimeStamp utcTime, byte side, double price, int size);
		
		void init(TimeStamp utcTime, byte side, double dPrice, int size, double dBid, double dAsk);

		void init(TimeStamp utcTime, byte side, double price, int size, double dBid, double dAsk, ushort[] bidSize, ushort[] askSize);
		
		long lBid {
			get;
			set;
		}
		
		long lAsk {
			get;
			set;
		}
		
		long lPrice {
			get;
			set;
		}
		
		string Symbol {
			get;
			set;
		}
		
		bool IsRealTime {
			get;
			set;
		}
		
		bool IsSimulateTicks {
			get;
			set;
		}
	}
}

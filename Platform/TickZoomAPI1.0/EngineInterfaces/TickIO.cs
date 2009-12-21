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
	/// Description of TickIO
	/// </summary>
	public interface TickIO : Tick, ReadWritable<TickBinary>
	{
		/// <summary>
		/// Initializes the tick by clearing it out and setting any defaults.
		/// This method must be called every time you create another tick
		/// with SetTime(), SetTrade(), SetQuote(), or SetDepth().
		/// </summary>
		void Initialize();
		
		/// <summary>
		/// Sets the time of the tick. 
		/// </summary>
		/// <param name="utcTime">Must be the UTC time.</param>
		void SetTime(TimeStamp utcTime);
		
		/// <summary>
		/// Sets quote data for the tick
		/// </summary>
		/// <param name="dBid">The Bid price.</param>
		/// <param name="dAsk">The Ask price.</param>
		void SetQuote(double dBid, double dAsk);

		/// <summary>
		/// Sets last trade data for this tick.
		/// </summary>
		/// <param name="price">The price of the trade.</param>
		/// <param name="size">The size of the trade.</param>
		void SetTrade(double price, int size);

		/// <summary>
		/// Sets last trade data for this tick with side of trade information.
		/// </summary>
		/// <param name="side">The side of the bid/ask spread which absorbed this trade. Only useful for advanced data feed analysis.</param>
		/// <param name="price">The price of the trade.</param>
		/// <param name="size">The size of the trade.</param>
		void SetTrade(TradeSide side, double price, int size);
		
		/// <summary>
		/// Sets the Depth of Market data for the tick.
		/// </summary>
		/// <param name="bidSize">An array of bid sizes for 5 levels of depth of market.</param>
		/// <param name="askSize">An array of ask sizes for 5 levels of depth of market.</param>
		void SetDepth(ushort[] bidSize, ushort[] askSize);
		
		/// <summary>
		/// Makes a copy of the 'other' tick onto this one but only the data specified in the
		/// contentMask. So even if the tick contains, for example, trade, quote, and depth 
		/// information, if contentMask only specifies trade then the rest will not be copied.
		/// </summary>
		/// <param name="other">The other tick to copy from.</param>
		/// <param name="contentMask">Controls what to copy from the other tick</param>
		void Copy(TickIO other, byte contentMask);
		
		long lBid {
			get;
		}
		
		long lAsk {
			get;
		}
		
		long lPrice {
			get;
		}
		
		string Symbol {
			get;
		}
		
		ulong lSymbol {
			get;
		}
		
		bool IsSimulateTicks {
			get;
		}
		
#region Obsolete
		/// <summary>
		/// Obsolete: Please use Copy instead.
		/// </summary>
		[Obsolete("Please use Copy instead.",true)]
		void init(TickIO tick, byte contentMask);
		
		/// <summary>
		/// Obsolete: This property is unused and will be removed eventually.
		/// </summary>
		[Obsolete("This property is unused and will be removed eventually.",true)]
		bool IsRealTime {
			get;
		}
		
		/// <summary>
		/// Obsolete: Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead
		/// </summary>
		[Obsolete("Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead.",true)]
		void init(TimeStamp utcTime, double dBid, double dAsk);

		/// <summary>
		/// Obsolete: Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead
		/// </summary>
		[Obsolete("Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead",true)]
		void init(TimeStamp utcTime, double price, int size);

		/// <summary>
		/// Obsolete: Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead
		/// </summary>
		[Obsolete("Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead",true)]
		void init(TimeStamp utcTime, byte side, double price, int size);

		/// <summary>
		/// Obsolete: Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead
		/// </summary>
		[Obsolete("Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead",true)]
		void init(TimeStamp utcTime, byte side, double dPrice, int size, double dBid, double dAsk);

		/// <summary>
		/// Obsolete: Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead
		/// </summary>
		[Obsolete("Please use SetTime() and either SetQuote(), SetTrade(), SetDepth() instead",true)]
		void init(TimeStamp utcTime, byte side, double price, int size, double dBid, double dAsk, ushort[] bidSize, ushort[] askSize);
#endregion

	}
}

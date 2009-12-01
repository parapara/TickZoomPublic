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
 * Date: 4/2/2009
 * Time: 8:16 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;

namespace TickZoom.Api
{
	public interface SymbolInfo
	{
		/// <summary>
		/// The actual text value of the symbol.
		/// </summary>
		string Symbol {
			get;
		}
	
		/// <summary>
		/// The universal symbol ties different symbols together from different
		/// data providers and brokers which actually represent the same instrument.
		/// In that case, they will all have the same UnivesalSymbol.
		/// </summary>
		SymbolInfo UniversalSymbol {
			get;
		}
	
		/// <summary>
		/// The binary identifier is a unique number assigned to every symbol
		/// each time the system starts. Therefore this number can change any time
		/// a symbol is added or removed from the dictionary upon the next time it
		/// restarts. For that reason it is useless to use this in user strategy code.
		/// It's purpose internally to TickZoom is to greatly increase performance
		/// by refering to a native number rather than a string to represet symbols.
		/// </summary>
		ulong BinaryIdentifier {
			get;
		}
		
		/// <summary>
		/// The time of day that the primary session for this symbol starts.
		/// </summary>
		Elapsed SessionStart {
			get;
			set;
		}
		
		/// <summary>
		/// The time of day that the primary session for this symbol ends.
		/// </summary>
		Elapsed SessionEnd {
			get;
			set;
		}
		
		/// <summary>
		/// Returns a fractional value for the minimum price move for the symbol.
		/// For example: And U.S. stock's minimum tick will be 0.01
		/// </summary>
		double MinimumTick {
			get;
		}
		
		/// <summary>
		/// The currency value of a full point of a symbol in its denominated currency.
		/// You can simply multiple FullPointValue by the any price of the symbol to get the
		/// full currency value of that price.
		/// </summary>
		double FullPointValue {
			get;
		}
		
		void CopyProperties(object obj);
	
		[Obsolete("Please create your data with the IsSimulateTicks flag set to true instead of this property.",true)]
		bool SimulateTicks {
			get;
		}
		
		/// <summary>
		/// Sets the divisor for lots when collecting Depth of Market data for
		/// every tick. This helps to standardize across different markets which 
		/// use different sizes of orders to indicate 1 full lot. So for stocks this
		/// should ordinarily be set to 100 whereas for Forex the standard lot size
		/// is 10000.
		/// </summary>
		int Level2LotSize {
			get;
		}
		
		/// <summary>
		/// This increment in price between each level of Depth of Market
		/// which you wish to collect.
		/// </summary>
		double Level2Increment {
			get;
		}
		
 		/// <summary>
 		/// Eliminate reporting of orders on Level II less
		/// than a the lot size minimum.
 		/// </summary>
 		int Level2LotSizeMinimum {
			get;
		}
	}
}
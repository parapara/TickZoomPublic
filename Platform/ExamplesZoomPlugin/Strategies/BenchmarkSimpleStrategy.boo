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
 * GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */

#endregion

#region Namespaces


#endregion

namespace TickZoom

import System
import TickZoom.Common

class BenchmarkSimpleStrategy(StrategyCommon):

	def constructor():
		pass

	
	override def OnInitialize():
		Performance.GraphTrades = true
		//			Performance.Equity.GraphEquity = true;
		PositionSize.Size = 10000

	
	override def OnIntervalClose() as bool:
		// Example log message.
		//Log.WriteLine( "close: " + Ticks[0] + " " + Minutes.Close[0] + " " + Minutes.Time[0]);
		
		Enter.BuyLimit(Bars.High[0])
		Enter.SellLimit(Bars.Low[0])
		return true


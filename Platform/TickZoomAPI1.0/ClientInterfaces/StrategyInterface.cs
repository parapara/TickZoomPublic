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
 * Date: 3/16/2009
 * Time: 2:22 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;

namespace TickZoom.Api
{
	public interface StrategyInterface : ModelInterface
	{
		string OnOptimizeResults();
		[Obsolete("Please, implement OnGetFitness() method instead.")]
		double Fitness {
			get;
		}
		PositionInterface Position {
			get;
		}
		ResultInterface Result {
			get;
		}
		double OnGetFitness();
		bool OnWriteReport(string folder);
		string OnGetOptimizeResult(Dictionary<string,object> optimizeValues);
	 	IList<LogicalOrder> LogicalOrders {
        	get;
		}
	}
}

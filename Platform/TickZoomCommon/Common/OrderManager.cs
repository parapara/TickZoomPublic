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
 * Date: 5/18/2009
 * Time: 12:54 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of OrderManager.
	/// </summary>
	public class OrderManager : StrategySupport, OrderManagerInterface
	{
		List<LogicalOrder> orders = new List<LogicalOrder>();
		
		public OrderManager(Strategy strategy) : base(strategy) {
		}
		
		public void Add(LogicalOrder order)
		{
			orders.Add(order);
		}
		
		public void Remove(LogicalOrder order)
		{
			orders.Remove(order);
		}
		
	 	public IList<LogicalOrder> Orders {
        	get { return (IList<LogicalOrder>) orders; }
		}
		
		public bool AreExitsActive {
			get { return Strategy.Position.HasPosition; }
		}
		
		public bool AreEntriesActive {
			get { return true; }
		}
		
		public override PositionInterface Position {
			get { return Strategy.Position; }
		}
	}
}

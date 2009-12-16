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
 * Date: 9/20/2009
 * Time: 3:41 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using TickZoom.Common;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of SymbolCategory.
	/// </summary>
	public class SymbolCategory : IEnumerable<SymbolProperties>
	{
		string name;
		SymbolProperties @default;
		List<SymbolCategory> categories = new List<SymbolCategory>();
		List<SymbolProperties> symbols = new List<SymbolProperties>();
		
		public SymbolCategory(SymbolProperties symbolProperties)
		{
			@default = symbolProperties;
		}
		
		public SymbolCategory()
		{
			@default = new SymbolProperties();
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public List<SymbolCategory> Categories {
			get { return categories; }
		}
		
		public List<SymbolProperties> Symbols {
			get { return symbols; }
		}
		
		public SymbolProperties Default {
			get { return @default; }
			set { @default = value; }
		}
		
		public IEnumerator<SymbolProperties> GetEnumerator()
		{
			foreach( SymbolProperties properties in symbols) {
				yield return properties;
			}
			foreach( SymbolCategory category in categories) {
				foreach( SymbolProperties properties in category ) {
					yield return properties;
				}
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

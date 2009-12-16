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
 * Time: 12:23 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Common
{
	public class SymbolLibrary 
	{
		Dictionary<string,SymbolProperties> symbolMap;
		Dictionary<ulong,SymbolProperties> universalMap;
		public SymbolLibrary() {
			symbolMap = new Dictionary<string, SymbolProperties>();
			SymbolDictionary dictionary = SymbolDictionary.Create("universal",SymbolDictionary.UniversalDictionary);
			IEnumerable<SymbolProperties> enumer = dictionary;
			foreach( SymbolProperties symbolProperties in dictionary) {
				symbolMap[symbolProperties.Symbol] = symbolProperties;
			}
			dictionary = SymbolDictionary.Create("user",SymbolDictionary.UserDictionary);
			foreach( SymbolProperties symbolProperties in dictionary) {
				symbolMap[symbolProperties.Symbol] = symbolProperties;
			}
			ulong universalIdentifier = 1;
			universalMap = new Dictionary<ulong, SymbolProperties>();
			foreach( var kvp in symbolMap) {
				kvp.Value.BinaryIdentifier = universalIdentifier;
				universalMap.Add(universalIdentifier,kvp.Value);
				universalIdentifier ++;
			}
		}
		
		public SymbolProperties GetSymbolProperties(string symbol) {
			SymbolProperties symbolProperties;
			if( symbolMap.TryGetValue(symbol.Trim(),out symbolProperties)) {
				return symbolProperties;
			} else {
				throw new ApplicationException( "Sorry, symbol " + symbol + " was not found in any symbol dictionary.");
			}
		}
		
		public SymbolInfo LookupSymbol(string symbol) {
			return GetSymbolProperties(symbol);
		}
	
		public SymbolInfo LookupSymbol(ulong universalIdentifier) {
			SymbolProperties symbolProperties;
			if( universalMap.TryGetValue(universalIdentifier,out symbolProperties)) {
				return symbolProperties;
			} else {
				throw new ApplicationException( "Sorry, universal id " + universalIdentifier + " was not found in any symbol dictionary.");
			}
		}
	}
}

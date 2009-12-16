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
 * Date: 9/21/2009
 * Time: 10:29 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion


using System;
using System.IO;
using NUnit.Framework;
using TickZoom.Common;

namespace TickZoom.StarterTest 
{
	[TestFixture]
	public class DictionaryTest
	{
		[Test]
		public void LoadMBTrading()
		{
			string fileName = @"..\..\Platform\TickZoomTesting\Startup\dictionary.tzdict";
			SymbolDictionary dictionary = SymbolDictionary.Create(new StreamReader(fileName));
			foreach( SymbolProperties properties in dictionary) {
				InstrumentImpl instrument = InstrumentImpl.Get(properties.Symbol);
				Assert.AreEqual( instrument.DepthIncrement, properties.Level2Increment);
				Assert.AreEqual( instrument.LotSize, properties.Level2LotSize);
				Assert.AreEqual( instrument.LotSizeDomLimit, properties.Level2LotSizeMinimum);
			}
		}
		[Test]
		public void LoadMBTUSDJPY()
		{
			string fileName = @"..\..\Platform\TickZoomTesting\Startup\dictionary.tzdict";
			SymbolDictionary dictionary = SymbolDictionary.Create(new StreamReader(fileName));
			SymbolProperties properties = dictionary.Get("USD/JPY");
			InstrumentImpl instrument = InstrumentImpl.Get("USD/JPY");
			Assert.AreEqual( instrument.DepthIncrement, properties.Level2Increment);
			Assert.AreEqual( instrument.LotSize, properties.Level2LotSize);
			Assert.AreEqual( instrument.LotSizeDomLimit, properties.Level2LotSizeMinimum);
		}
	}
}

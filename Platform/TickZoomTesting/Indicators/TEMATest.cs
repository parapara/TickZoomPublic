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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;

namespace TickZoom.Indicators
{
	[TestFixture]
	public class TEMATest
	{
		private static readonly Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		TEMA tema;
		TestBars bars;
		
		[TearDown]
		public void TearDown() {
			
		}
		
		[SetUp]
		public void Setup() {
			bars = Factory.Engine.TestBars(Intervals.Day1);
			tema = new TEMA(bars.Close,14);
			Assert.IsNotNull(tema, "constructor");
			tema.IntervalDefault = Intervals.Day1;
			for(int j=0; j<tema.Chain.Dependencies.Count; j++) {
				ModelInterface formula = tema.Chain.Dependencies[j].Model;
				formula.Bars = bars;
				formula.OnBeforeInitialize();
				formula.OnInitialize();
			}
			tema.Bars = bars;
			tema.OnBeforeInitialize();
			tema.OnInitialize();
		}
		
		[Test]
		public void Values()
		{
			for( int i = 0; i < data.Length; i++) {
				// open, high, low, close all the same.
				bars.AddBar( data[i], data[i], data[i], data[i], 0);
				for(int j=0; j<tema.Chain.Dependencies.Count; j++) {
					ModelInterface formula = tema.Chain.Dependencies[j].Model;
					formula.OnBeforeIntervalOpen();
					formula.OnIntervalClose();
				}
				tema.OnBeforeIntervalOpen();
				tema.OnIntervalClose();
				Assert.AreEqual(result[i],Math.Round(tema[0]),"current result at " + i);
				if( i > 1) Assert.AreEqual(result[i-1],Math.Round(tema[1]),"result 1 back at " + i);
				if( i > 2) Assert.AreEqual(result[i-2],Math.Round(tema[2]),"result 2 back at " + i);
			}
		}
		
		private double[] data = new double[] {
			10000,
			10100,
			10040,
			10200,
			10760,
			11190,
			11300,
			12030,
			12360,
			12150,
			12440,
			12910,
			13270,
			12550,
			11890,
			12350,
			11930,
			11900,
			11370,
			10820,
			10720,
			11570,
			12520,
			13290,
			13590,
			13850,
			13500,
			13810,
			14430,
			13800,
			14140,
			13850,
			13210,
			13480,
			14140,
			14250,
			13600,
			13160,
			12940,
			13670,
			13770,
			13150,
			12990,
			12360,
			12580,
			13220,
			12220,
			11800,
			12230,
			11580,
			10680,
			9940,
			10300,
			11030,
			11790,
			11890
		};

		private int[] result = new int[] {
			10000,
			10035,
			10040,
			10099,
			10339,
			10666,
			10946,
			11404,
			11852,
			12102,
			12374,
			12725,
			13095,
			13100,
			12852,
			12809,
			12614,
			12444,
			12122,
			11682,
			11315,
			11338,
			11690,
			12226,
			12731,
			13193,
			13407,
			13663,
			14056,
			14115,
			14260,
			14252,
			14006,
			13902,
			14044,
			14179,
			14044,
			13780,
			13498,
			13539,
			13603,
			13434,
			13250,
			12894,
			12707,
			12797,
			12522,
			12176,
			12076,
			11785,
			11265,
			10632,
			10302,
			10333,
			10644,
			10931
		};
		
	}
}
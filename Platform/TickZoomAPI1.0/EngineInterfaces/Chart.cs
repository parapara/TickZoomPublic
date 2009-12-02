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
using System.ComponentModel;
using System.Drawing;

namespace TickZoom.Api
{

	public interface Chart
	{
		void InitializeTick( Tick tick);
		/// <summary>
		/// DrawTrade allows the Charting implementation to draw the trade on the
		/// chart using the order information in LogicalOrder. 
		/// </summary>
		/// <param name="order">The order that caused this trade.</param>
		/// <param name="resultingPosition">Current number of positions. Note, this is
		/// different from the position size because the position sizing gets applied by
		/// the PositionSize object which can be driven by various logic and usually by
		/// portfolio asset allocation.</param>
		int DrawTrade( LogicalOrder order, double fillPrice,  double resultingPosition);
//		[Obsolete("Use the Update() method instead.",true)]
		void AddBar( Bars updateSeries, Bars displaySeries);
//		void Update();
		int DrawText(string text, Color color, int bar, double y, Positioning orient);
		int DrawLine( Color color, int bar1, double y1, int bar2, double y2, LineStyle style);
		[Obsolete("Please use the overloaded DrawArrow() instead.",true)]
		int DrawArrow( Color color, float size, int bar1, double y1, int bar2, double y2);
		/// <summary>
		/// draws an arrow pointing in direction with the tip 
		/// of the arrow at bar, price, at the specified size.
		/// </summary>
		/// <param name="direction">which way to point the arrow.</param>
		/// <param name="color">color of the arrow</param>
		/// <param name="size">size of the arrow</param>
		/// <param name="bar">bar number to draw the arrow as x axis</param>
		/// <param name="price">price where to draw the arrow as the y axis</param>
		/// <returns></returns>
		int DrawArrow( ArrowDirection direction, Color color, float size, int bar, double price);
		void ChangeLine( int lineId, Color color, int bar1, double y1, int bar2, double y2, LineStyle style);
		int DrawBox( Color color, int bar, double y);
		int DrawBox( Color color, int bar1, double y, double width, double height);
		void WriteLine(string text);
		void Show();
		void Hide();
		void AudioNotify( Audio clip);
		bool ShowPriceGraph {
			get ;
			set ;
		}
		
		bool IsDynamicUpdate {
			get ;
			set ;
		}
		
		StrategySupportInterface StrategyForTrades {
			get ;
			set ;
		}
		
		string Symbol {
			get;
			set;
		}
		
//		Data Data {
//			get;
//			set;
//		}
//		
		Bars UpdateBars {
			get;
			set;
		}

		Bars DisplayBars {
			get;
			set;
		}
		
		Bars ChartBars {
			get;
			set;
		}
		
		Interval IntervalChartDisplay {
			get;
			set;
		}
		
		Interval IntervalChartBar {
			get;
			set;
		}
		
		Interval IntervalChartUpdate {
			get;
			set;
		}
		
		List<IndicatorInterface> Indicators {
			get;
		}
	}
}

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
using System.Drawing;

using TickZoom.Api;


namespace TickZoom.Common
{

	/// <summary>
	/// Description of Signal.
	/// </summary>
	public class PositionCommon : PositionInterface
	{
		protected Color signalColor = Color.Blue;
		protected double signal = 0;
		protected TimeStamp signalTime;
		protected double signalPrice = 0;
		protected ModelInterface model;
		protected string symbol = "default";
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PositionCommon(ModelInterface model)
		{
			this.model = model;
		}
		
		public virtual double Signal {
			get { return signal; }
			set { 
				#if TRACE
				if( value != signal) {
					if( trace) log.Trace(model.Name+".Signal("+value+")");
					if( trace) log.TickOn();
				} else {
					if( trace) log.TickOff();
				}
				#endif
				if( signal != value) {
					signalTime = model.Data.Ticks[0].Time;
					signalPrice = model.Data.Ticks[0].Bid;
					signal = value;
				}
			}
		}
		
		public string Symbol {
			get { return symbol; }
		}		
		
		public Color Color {
			get { return signalColor; }
			set { signalColor = value; }
		}

		public double SignalPrice {
			get { return signalPrice; }
		}
		
		public TimeStamp SignalTime {
			get { return signalTime; }
		}
		
		public bool IsFlat {
			get { return signal == 0; }
		}
		
		public bool HasPosition {
			get { return signal != 0; }
		}
		
		public bool IsLong {
			get { return signal > 0; }
		}

		public bool IsShort {
			get { return signal < 0; }
		}

		public double Size {
			get { return Math.Abs(signal); }
		}
	}
}

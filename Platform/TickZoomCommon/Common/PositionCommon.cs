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
		protected double current = 0;
		protected TimeStamp time;
		protected double price = 0;
		protected ModelInterface model;
		protected string symbol = "default";
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PositionCommon(ModelInterface model)
		{
			this.model = model;
		}
		
		[Obsolete("Please use Current instead.",true)]
		public virtual double Signal {
			get { return current; }
			set { }
		}
		
		public virtual double Current {
			get { return current; }
		}
		
		public void Change( double position) {
			double price;
			if( position > current) {
				price = model.Data.Ticks[0].Ask;
			} else if( position < current) {
				price = model.Data.Ticks[0].Bid;
			} else {
				price = 0;
//				throw new ApplicationException("Position.Change called with the same position. You must pass a different position value than Position.Current in order to call Change().");
			}
			Change( position, price, model.Data.Ticks[0].Time);
		}
		
		public virtual void Change( double position, double price, TimeStamp time) {
			#if TRACE
			if( value != current) {
				if( trace) log.Trace(model.Name+".Signal("+value+")");
			}
			#endif
			if( current != position) {
				this.time = time;
				this.price = price;
				this.current = position;
			}
		}
		
		public void Copy( PositionInterface other) {
			Change(other.Current,other.Price,other.Time);
		}
		
		public string Symbol {
			get { return symbol; }
		}		
		
		public Color Color {
			get { return signalColor; }
			set { signalColor = value; }
		}

		[Obsolete("Please use Price instead.",true)]
		public double SignalPrice {
			get { return price; }
		}
		
		public double Price {
			get { return price; }
		}
		
		[Obsolete("Please use Time instead.",true)]
		public TimeStamp SignalTime {
			get { return time; }
		}
		
		public TimeStamp Time {
			get { return time; }
		}
		
		public bool IsFlat {
			get { return current == 0; }
		}
		
		public bool HasPosition {
			get { return current != 0; }
		}
		
		public bool IsLong {
			get { return current > 0; }
		}

		public bool IsShort {
			get { return current < 0; }
		}

		public double Size {
			get { return Math.Abs(current); }
		}
	}
}

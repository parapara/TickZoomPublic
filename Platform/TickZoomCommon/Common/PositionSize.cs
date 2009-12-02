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
using System.Diagnostics;
using System.Drawing;
using TickZoom.Api;


namespace TickZoom.Common
{
	/// <summary>
	/// Description of StrategySupport.
	/// </summary>
	[Obsolete("Please use PositionSize instead.",true)]
	public class PositionSizeCommon : PositionSize
	{
		public PositionSizeCommon(Strategy strategy) : base(strategy) {
		}
	}
	public class PositionSize : StrategySupport
	{
		double size = 1;
		double previousSignal = 0;
		StrategySupportInterface strategy;
		
		public PositionSize(Strategy strategy) : base(strategy) {
		}
		
		public override void OnInitialize()
		{
			if( IsTrace) Log.Trace(FullName+".Initialize()");
			strategy = Chain.Next.Model as StrategySupportInterface;
		}
	
		public sealed override bool OnProcessTick(Tick tick)
		{
			if( IsTrace) Log.Trace("ProcessTick() Previous="+strategy+" Previous.Signal="+strategy.Position.Signal);

			// Is this a new position?
			if( previousSignal != strategy.Position.Signal ) {
				// Pass 
				previousSignal = strategy.Position.Signal;
				Position.Signal = strategy.Position.Signal * size;
			}
			return true;
		}

		/// <summary>
		/// Sets the size of a position. Every time you start a new position, this property will used to set the position size.
		/// So you may change it during your strategy to increase of decrease the size of new positions.
		/// </summary>
		public double Size {
			get { return size; }
			set { size = value; }
		}
		
		public override string ToString()
		{
			return FullName;
		}
	}
}

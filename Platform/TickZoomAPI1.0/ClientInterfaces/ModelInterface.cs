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
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

namespace TickZoom.Api
{
	public interface ModelInterface
	{
		Context Context {
			get;
			set;
		}
		
		Chain Chain {
			get;
			set;
		}
		
		DrawingInterface Drawing {
			get;
			set;
		}
	
		Chart Chart {
			get;
			set;
		}
		
		Data Data {
			get;
			set;
		}
		
		string SymbolDefault {
			get;
			set;
		}
		
		Bars Bars {
			get;
			set;
		}
		
		IList<Interval> UpdateIntervals {
			get;
		}
		
		[Category("Strategy Settings")]
		Interval IntervalDefault {
			get;
			set;
		}
		
		void OnProperties(ModelProperties properties);
		
		void OnBeforeInitialize();
		
		void OnInitialize();
		
		void OnStartHistorical();
			
		bool OnBeforeIntervalOpen();
		
		bool OnBeforeIntervalOpen(Interval interval);
		
		bool OnIntervalOpen();
		
		bool OnIntervalOpen(Interval interval);
		
		bool OnProcessTick(Tick tick);
		
		bool OnBeforeIntervalClose();
		
		bool OnBeforeIntervalClose(Interval interval);
		
		bool OnIntervalClose();
		
		bool OnIntervalClose(Interval interval);
	
		void OnEndHistorical();
		
		string Name {
			get;
			set;
		}
	
		[Obsolete("Use FullName property instead.")]
		string LogName {
			get;
		}
		
		string FullName {
			get;
		}
		
		bool IsOptimizeMode {
			get;
			set;
		}
		
	}
}

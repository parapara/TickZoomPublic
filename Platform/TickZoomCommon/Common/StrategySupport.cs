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
using System.IO;
using System.Reflection;
using System.Text;

using TickZoom.Api;


namespace TickZoom.Common
{
	/// <summary>
	/// Description of StrategySupport.
	/// </summary>
	public class StrategySupport : Model, StrategySupportInterface
	{
		PositionInterface position;
		Strategy strategy;
		private readonly Log instanceLog;
		private readonly bool instanceNotice;
		private readonly bool instanceDebug;
		private readonly bool instanceTrace;
		private string fullName;

		public StrategySupport(Strategy strategy)
		{
			instanceLog = Factory.Log.GetLogger(this.GetType()+"."+strategy.Name);
			instanceNotice = instanceLog.IsNoticeEnabled;
			instanceDebug = instanceLog.IsDebugEnabled;
			instanceTrace = instanceLog.IsTraceEnabled;
			this.strategy = strategy;
			position = new PositionCommon(this);
			fullName = strategy.Name + "." + Name;
		}
		
		public string ToStatistics(Dictionary<string,string> optimizeValues)
		{
			throw new NotImplementedException("Operation not valid for this type of object");
		}
		
		public override string SymbolDefault {
			get { return strategy.SymbolDefault; }
			set { /* ignore */ }
		}
		
		public virtual PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
		public Strategy Strategy {
			get { return strategy; }
		}
		
		public Log Log {
			get { return instanceLog; }
		}
		
		public bool IsNotice {
			get { return instanceNotice; }
		}
		
		public bool IsDebug {
			get { return instanceDebug; }
		}
		
		public bool IsTrace {
			get { return instanceTrace; }
		}
		
		public override string FullName {
			get { return fullName; }
		}
	}
}

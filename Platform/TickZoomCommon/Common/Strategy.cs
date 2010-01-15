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
	public class Strategy : Model, StrategyInterface 
	{
		PositionInterface position;
		private static readonly Log log = Factory.Log.GetLogger(typeof(Strategy));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		private readonly Log instanceLog;
		private readonly bool instanceDebug;
		private readonly bool instanceTrace;
		private Result result;
		List<LogicalOrder> logicalOrders = new List<LogicalOrder>();
		
		OrderManager orderManager;
		OrderHandlers orderHandlers;
		ExitCommon exitActiveNow;
		EnterCommon enterActiveNow;
		ExitCommon exitNextBar;
		EnterCommon enterNextBar;
		Performance performance;
		PositionSize positionSize;
		ExitStrategy exitStrategy;
		
		public Strategy()
		{
			instanceLog = Factory.Log.GetLogger(this.GetType()+"."+Name);
			instanceDebug = instanceLog.IsDebugEnabled;
			instanceTrace = instanceLog.IsTraceEnabled;
			position = new PositionCommon(this);
			if( trace) log.Trace("Constructor");
			Chain.Dependencies.Clear();
			isStrategy = true;
			result = new Result(this);
			/// <summary>
			/// 
			/// </summary>
			
			exitActiveNow = new ExitCommon(this);
			enterActiveNow = new EnterCommon(this);
			exitNextBar = new ExitCommon(this);
			exitNextBar.Orders = exitActiveNow.Orders;
			exitNextBar.IsNextBar = true;
			enterNextBar = new EnterCommon(this);
			enterNextBar.Orders = enterActiveNow.Orders;
			enterNextBar.IsNextBar = true;
			orderHandlers = new OrderHandlers(enterActiveNow,enterNextBar,exitActiveNow,exitNextBar);
			
			// Interceptors.
			orderManager = new OrderManager(this);
			performance = new Performance(this);
		    positionSize = new PositionSize(this);
		    exitStrategy = new ExitStrategy(this);
		}
		
		public override void OnConfigure()
		{
			exitActiveNow.OnInitialize();
			enterActiveNow.OnInitialize();
			exitNextBar.OnInitialize();
			enterNextBar.OnInitialize();
			exitNextBar.OnInitialize();
			base.OnConfigure();
			
			AddInterceptor(orderManager);
			AddInterceptor(performance.Equity);
			AddInterceptor(performance);
			AddInterceptor(positionSize);
			AddInterceptor(exitStrategy);
		}
		
		public override void OnEvent(EventContext context, EventType eventType, object eventDetail)
		{
			base.OnEvent(context, eventType, eventDetail);
			if( eventType == EventType.Tick) {
				if( context.Position == null) {
					context.Position = new PositionCommon(this);
				}
				context.Position.Copy(Position);
			}
		}
		
		[Obsolete("Please, use OnGetOptimizeResult() instead.",true)]
		public virtual string ToStatistics() {
			throw new NotImplementedException();
		}
		
		public virtual string OnGetOptimizeResult(Dictionary<string,object> optimizeValues)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Fitness,");
			sb.Append(OnGetFitness());
			foreach( KeyValuePair<string,object> kvp in optimizeValues) {
				sb.Append(",");
				sb.Append(kvp.Key);
				sb.Append(",");
				sb.Append(kvp.Value);
			}
			return sb.ToString();
		}
		
		public override bool OnWriteReport(string folder)
		{
			return performance.WriteReport(Name,folder);
		}

		[Browsable(true)]
		[Category("Strategy Settings")]		
		public override Interval IntervalDefault {
			get { return base.IntervalDefault; }
			set { base.IntervalDefault = value; }
		}
		
		[Browsable(false)]
		public Strategy Next {
			get { return Chain.Next.Model as Strategy; }
		}
		
		[Browsable(false)]
		public Strategy Previous {
			get { return Chain.Previous.Model as Strategy; }
		}
		
		[Browsable(false)]
		public override string Name {
			get { return base.Name; }
			set { base.Name = value; }
		}

		public OrderHandlers Orders {
			get { return orderHandlers; }
		}
		
		[Obsolete("Please user Orders.Exit instead.",true)]
		public ExitCommon ExitActiveNow {
			get { return exitActiveNow; }
		}
		
		[Obsolete("Please user Orders.Enter instead.",true)]
		public EnterCommon EnterActiveNow {
			get { return enterActiveNow; }
		}
		
		[Category("Strategy Settings")]
		public ExitStrategy ExitStrategy {
			get { return exitStrategy; }
			set { exitStrategy = value; }
		}
		
		[Category("Strategy Settings")]
		public PositionSize PositionSize {
			get { return positionSize; }
			set { positionSize = value; }
		}

		[Category("Strategy Settings")]
		public Performance Performance {
			get { return performance; }
			set { performance = value;}
		}

		public virtual double OnGetFitness() {
			EquityStats stats = Performance.Equity.CalculateStatistics();
			return stats.Daily.SortinoRatio;
		}
		
		public OrderManager OrderManager {
			get { return orderManager; }
		}

		public Log Log {
			get { return instanceLog; }
		}
		
		public bool IsDebug {
			get { return instanceDebug; }
		}
		
		public bool IsTrace {
			get { return instanceTrace; }
		}
		
		public virtual void OnEnterTrade() {
			
		}
		
		public virtual void OnExitTrade() {
			
		}
		
		public PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
	 	public IList<LogicalOrder> LogicalOrders {
        	get { return (IList<LogicalOrder>) logicalOrders; }
		}
		
		public ResultInterface Result {
			get { return result; }
		}
	}
	
	[Obsolete("Please user Strategy instead.",true)]
	public class StrategyCommon : Strategy {
		
	}
}
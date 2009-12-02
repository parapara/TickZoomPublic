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

		[Browsable(false)]
		public PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
		private static readonly Log log = Factory.Log.GetLogger(typeof(Strategy));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		private readonly Log instanceLog;
		private readonly bool instanceDebug;
		private readonly bool instanceTrace;
		
		OrderManager orderManager;
		ExitCommon exit;
		EnterCommon enter;
		Chain exitStrategyChain;
		Chain positionSizeChain;
		Chain performanceChain;

		public Strategy()
		{
			instanceLog = Factory.Log.GetLogger(this.GetType()+"."+Name);
			instanceDebug = instanceLog.IsDebugEnabled;
			instanceTrace = instanceLog.IsTraceEnabled;
			position = new PositionCommon(this);
			if( trace) log.Trace("Constructor");
			log.Indent();
			Chain.Dependencies.Clear();
			isStrategy = true;
			orderManager = new OrderManager(this);
			Chain.InsertAfter(orderManager.Chain);
			exit = new ExitCommon(this);
			Chain.Dependencies.Add(exit.Chain);
			enter = new EnterCommon(true,true,this);
			Chain.Dependencies.Add(enter.Chain);
			Model exitStrategy = new ExitStrategy(this);
			exitStrategyChain = Chain.InsertBefore(exitStrategy.Chain);
			Model positionSizeStrategy = new PositionSize(this);
			positionSizeChain = exitStrategy.Chain.InsertBefore(positionSizeStrategy.Chain);
			Model performance = new Performance(this);
			performanceChain = performance.Chain;
			positionSizeStrategy.Chain.InsertBefore(performance.Chain);
			log.Outdent();
		}
		
		
		public void propogateName() {
			if( exitStrategyChain!=null) {
				exitStrategyChain.Model.Name = Name;
			}
			if( performanceChain != null) {
				performanceChain.Model.Name = Name;
			}
			if( positionSizeChain != null) {
				positionSizeChain.Model.Name = Name;
			}
			if( exit != null) {
				exit.Chain.Model.Name = Name;
			}
			if( enter != null) {
				enter.Chain.Model.Name = Name;
			}
		}
		
		public override void OnBeforeInitialize()
		{
			base.OnBeforeInitialize();
		}
		
		public sealed override bool OnBeforeIntervalOpen() {
			return base.OnBeforeIntervalOpen();
		}
		
		public sealed override bool OnBeforeIntervalOpen(Interval interval) {
			return base.OnBeforeIntervalOpen(interval);
		}
		
		public sealed override bool OnBeforeIntervalClose() {
			return base.OnBeforeIntervalClose();
		}
		
		public sealed override bool OnBeforeIntervalClose(Interval interval) {
			return base.OnBeforeIntervalClose(interval);
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
			Performance performance = (Performance) performanceChain.Model;
			return performance.WriteReport(Name,folder);
		}

		public virtual double OnCalculateProfitLoss(double position, double entry, double exit) {
			throw new NotImplementedException("The performance object ignores this method unless you override and provide your own implementation.");
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
		
		[Browsable(false)]
		public ExitCommon Exit {
			get { return exit; }
		}
		
		[Browsable(false)]
		public EnterCommon Enter {
			get { return enter; }
			set { if( trace) log.Trace( FullName+" Performance - Replacing " + performanceChain.Model.FullName + " with " + value.FullName);
				  performanceChain = performanceChain.Replace(value.Chain);
			}
		}
		
		[Category("Strategy Settings")]
		public ExitStrategy ExitStrategy {
			get { return (ExitStrategy) exitStrategyChain.Model; }
			set { if( trace) log.Trace( FullName+" Exits - Replacing " + exitStrategyChain.Model.FullName + " with " + value.FullName);
				  exitStrategyChain = exitStrategyChain.Replace(value.Chain);
			}
		}
		
		[Category("Strategy Settings")]
		public PositionSize PositionSize {
			get { return (PositionSize) positionSizeChain.Model; }
			set { if( trace) log.Trace( FullName+" PositionSize - Replacing " + positionSizeChain.Model.FullName + " with " + value.FullName);
				  positionSizeChain = positionSizeChain.Replace(value.Chain);
			}
		}

		[Category("Strategy Settings")]
		public Performance Performance {
			get { return (Performance) performanceChain.Model; }
			set { if( trace) log.Trace( FullName+" Performance - Replacing " + performanceChain.Model.FullName + " with " + value.FullName);
				  Performance.RemoveChildren();
				  performanceChain = performanceChain.Replace(value.Chain);
			}
		}

		[Obsolete("Use OnGetFitness() in a strategy instead.",true)]
		public override double Fitness {
			get { return Performance.Fitness; }
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
		
	}
	
	[Obsolete("Please user Strategy instead.",true)]
	public class StrategyCommon : Strategy {
		
	}
}
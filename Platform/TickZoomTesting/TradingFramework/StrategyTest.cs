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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.EngineTests
{
	/// <summary>
	/// Description of Formula.
	/// </summary>
	public class StrategyTest : ModelInterface
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(StrategyTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		string name;
		string symbolDefault;
		Chain chain;
		bool isStrategy = false;
		bool isIndicator = false;
		Interval intervalDefault = Intervals.Default;
		PositionInterface position;
		DrawingInterface drawing;

		List<Interval> updateIntervals = new List<Interval>();
		Data data;
		Chart chart;
		Context context;
		
		public StrategyTest()
		{
			position = new PositionCommon(this);
			drawing = new DrawingCommon(this);
			indicator =  Doubles();
			
			if( trace) log.Trace(GetType().Name+".new");
			name = GetType().Name;
			chain = Factory.Engine.Chain(this);
		}
		
		public void RequestUpdate( Interval interval) {
			updateIntervals.Add(interval);
		}
		
		public IList<Interval> UpdateIntervals {
			get { return updateIntervals; }
		}
		
		
		public ModelInterface NextFormulax {
			get { return Chain.Next.Model; }
		}
		
		public virtual bool OnWriteReport(string var) {
			return false;
		}

		public Elapsed SignalElapsed {
			get { return Ticks[0].Time - Position.SignalTime; }
		}
		
		public Interval IntervalDefault {
			get { return intervalDefault; }
			set { intervalDefault = value; }
		}
		
		public void AddIndicator( Model model) {
			chain.Dependencies.Add(model.Chain);
		}

		public Chart Chart {
			get { return chart; }
			set { chart = value; }
		}
		
		public override string ToString()
		{
			return name;
		}
		
		#region Convenience methods to access bar data
		public Data Data {
			get { return data; }
			set { data = value; }
		}
		
		
		Bars years = null;
		public Bars Years {
			get {
				if( years == null) years = data.Get(Intervals.Year1);
				return years;
			}
		}
		
		Bars months = null;
		public Bars Months {
			get {
				if( months == null) months = data.Get(Intervals.Month1);
				return months;
			}
		}
		
		Bars weeks = null;
		public Bars Weeks {
			get {
				if( weeks == null) weeks = data.Get(Intervals.Week1);
				return weeks;
			}
		}
		
		Bars days = null;
		public Bars Days {
			get {
				if( days == null) days = data.Get(Intervals.Day1);
				return days;
			}
		}
		
		Bars hours = null;
		public Bars Hours {
			get {
				if( hours == null) hours = data.Get(Intervals.Hour1);
				return hours;
			}
		}
		
		Bars minutes = null;
		public Bars Minutes {
			get {
				if( minutes == null) minutes = data.Get(Intervals.Minute1);
				return minutes;
			}
		}
		
		Bars sessions = null;
		public Bars Sessions {
			get {
				if( sessions == null) sessions = data.Get(Intervals.Session1);
				return sessions;
			}
		}
		
		Bars range5 = null;
		public Bars Range5 {
			get {
				if( range5 == null) range5 = data.Get(Intervals.Session1);
				return range5;
			}
		}
		
		Ticks ticks = null;
		public Ticks Ticks {
			get { 
				if( ticks == null) ticks = data.Ticks;
				return ticks; }
		}
	
		Bars bars = null;
		public Bars Bars {
			get { return bars; }
			set { bars = value; }
		}
		#endregion

		public Context Context {
			get { return context; }
			set { context = value; }
		}
		public virtual void OnBeforeInitialize() {
   			if( trace) log.Trace(GetType().Name+".BeforeInitialize() - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual void OnInitialize() {
   			if( trace) log.Trace(GetType().Name+".Initialize() - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual void OnStartHistorical() {
   			if( trace) log.Trace(GetType().Name+".StartHistorical() - NotImplemented");
			throw new NotImplementedException(); }
			
		public virtual bool OnBeforeIntervalOpen() {
   			if( trace) log.Trace(GetType().Name+".BeforeIntervalOpen() - NotImplemented");
			throw new NotImplementedException(); }

		public virtual bool OnBeforeIntervalOpen(Interval interval) {
   			if( trace) log.Trace(GetType().Name+".BeforeIntervalOpen("+interval+") - NotImplemented");
			throw new NotImplementedException(); }

		public virtual bool OnIntervalOpen() {
   			if( trace) log.Trace(GetType().Name+".IntervalOpen() - NotImplemented");
			throw new NotImplementedException(); }

		public virtual bool OnIntervalOpen(Interval interval) {
   			if( trace) log.Trace(GetType().Name+".IntervalOpen("+interval+") - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual bool OnProcessTick(Tick tick) {
   			if( trace) log.Trace(GetType().Name+".ProcessTick("+tick+") - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual bool OnBeforeIntervalClose() {
   			if( trace) log.Trace(GetType().Name+".BeforeIntervalClose() - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual bool OnBeforeIntervalClose(Interval interval) {
   			if( trace) log.Trace(GetType().Name+".BeforeIntervalClose("+interval+") - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual bool OnIntervalClose() {
   			if( trace) log.Trace(GetType().Name+".IntervalClose() - NotImplemented");
			throw new NotImplementedException(); }
		
		public virtual bool OnIntervalClose(Interval interval) {
   			if( trace) log.Trace(GetType().Name+".IntervalClose("+interval+") - NotImplemented");
			throw new NotImplementedException(); }

		public virtual void OnEndHistorical() {
   			if( trace) log.Trace(GetType().Name+".EndHistorical() - NotImplemented");
			throw new NotImplementedException();
		}
		
		public virtual bool OnSave( string fileName) {
			return true;
		}
		
		public void AddDependency( ModelInterface formula) {
			chain.Dependencies.Add(formula.Chain);
		}

		public bool IsStrategy {
			get { return isStrategy; }
			set { isStrategy = value; }
		}
		
		public bool IsIndicator{
			get { return isIndicator; }
			set { isIndicator= value; }
		}
		
		public virtual string Name {
			get { return name; }
			set { name = value; }
		}

		public Chain Chain {
			get { return chain; }
			set { chain = value; }
		}
		
		public string LogName {
			get { return name.Equals(GetType().Name) ? name : name+"."+GetType().Name; }
		}

		public string FullName {
			get { throw new NotImplementedException(); }
		}
		
		Doubles indicator;
		public int Count {
			get { return indicator.Count; }
		}
		
		public int BarCount {
			get { return indicator.BarCount; }
		}
		
		public double this[int position]
		{
			get { return indicator[position]; }
			set { indicator[position] = value; }
		}
		
		public Integers Integers() {
			return Factory.Engine.Integers();
		}
		
		public Integers Integers(int capacity) {
			return Factory.Engine.Integers(capacity);
		}
		
		public Doubles Doubles() {
			return Factory.Engine.Doubles();
		}
		
		public Doubles Doubles(int capacity) {
			return Factory.Engine.Doubles(capacity);
		}
		
		public Doubles Doubles(object obj) {
			return Factory.Engine.Doubles(obj);
		}
		
		public Series<T> Series<T>() {
			return Factory.Engine.Series<T>();
		}
		
		public void Add(double value)
		{
			indicator.Add(value);
		}
		
		public void Clear()
		{
			indicator.Clear();
		}
		
		public PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
		public DrawingInterface Drawing {
			get { return drawing; }
			set { drawing = value; }
		}
		
		public virtual double Fitness {
			get { return 0; }
		}
		
		public virtual string OnOptimizeResults()	{ return ""; }
		
		
		public bool WriteReport(string folder)
		{
			throw new NotImplementedException();
		}
		
		public string ToStatistics()
		{
			return "";
		}
		
		public void OnProperties(ModelProperties properties)
		{
			throw new NotImplementedException();
		}
		
		
		public bool IsOptimizeMode {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		
		public string SymbolDefault {
			get { return symbolDefault; }
			set { symbolDefault = value; }
		}
	}
}

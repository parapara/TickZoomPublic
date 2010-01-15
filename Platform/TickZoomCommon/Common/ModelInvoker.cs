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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

using TickZoom.Api;

#if JUNK
namespace TickZoom.Common
{
	public abstract class ModelEvent : EventInterceptor {
		protected ModelEvents model;
		protected EventType eventType;
		public ModelEvent(ModelEvents model) {
			this.model = model;
		}
		public abstract void Intercept(EventContext context, object eventDetail);
		public ModelEvents Model {
			get { return model; }
		}
		public EventType EventType {
			get { return eventType; }
		}
	}
	
	public class OpenEvent : ModelEvent {
		public OpenEvent(ModelEvents model) : base(model) {
			eventType = EventType.Open;
		}
		public sealed override void Intercept(EventContext context, object eventDetail) {
			if( eventDetail == null) {
				model.OnBeforeIntervalOpen();
				model.OnIntervalOpen();
			} else {
				model.OnIntervalOpen((Interval)eventDetail);
			}
		}
	}
	
	public class CloseEvent : ModelEvent {
		public CloseEvent(ModelEvents model) : base(model) {
			eventType = EventType.Close;
		}
		public sealed override void Intercept(EventContext context,object eventDetail) {
			if( eventDetail == null) {
				model.OnBeforeIntervalClose();
				model.OnIntervalClose();
			} else {
				model.OnIntervalClose((Interval)eventDetail);
			}
			context.Invoke(eventDetail);
		}
	}
	
	public class EndHistoricalEvent : ModelEvent {
		public EndHistoricalEvent(ModelEvents model) : base(model) {
			eventType = EventType.EndHistorical;
		}
		public sealed override void Intercept(EventContext context,object eventDetail) {
			model.OnEndHistorical();
		}
	}
	
	public class TickEvent : ModelEvent {
		public TickEvent(ModelEvents model) : base(model) {
			eventType = EventType.Tick;
		}
		public sealed override void Intercept(EventContext context,object eventDetail) {
			model.OnProcessTick((Tick)eventDetail);
		}
	}
}
#endif
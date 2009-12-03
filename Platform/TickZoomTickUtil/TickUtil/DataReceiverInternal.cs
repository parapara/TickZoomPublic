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
using TickZoom.Api;

//using TickZoom.Api;

namespace TickZoom.TickUtil
{
	public class DataReceiverDefault : Receiver {
	   		static readonly Log log = Factory.Log.GetLogger(typeof(DataReceiverDefault));
	   		static readonly bool debug = log.IsDebugEnabled;
		TickQueue readQueue = Factory.TickUtil.TickQueue(typeof(DataReceiverDefault));
        Provider sender;
		private ReceiverState receiverState = ReceiverState.Ready;
		
		public ReceiverState OnGetReceiverState(SymbolInfo symbol) {
			return receiverState;
		}
		
		public DataReceiverDefault(Provider sender) {
			this.sender = sender;
			readQueue.StartEnqueue = Start;
		}
		
		private void Start() {
			sender.Start(this);
		}
		
		public bool CanReceive {
			get { return readQueue.CanEnqueue; }
		}
		
		public void OnSend(ref TickBinary o) {
			readQueue.EnQueue(ref o);
		}

	    public void OnPosition(SymbolInfo symbol, double signal)
	    {
	        readQueue.EnQueue(EntryType.PositionChange, symbol);
	    }

	    public void OnRealTime(SymbolInfo symbol) {
			try {
				readQueue.EnQueue(EntryType.StartRealTime, symbol);
			} catch( QueueException) {
				log.Warn("Already terminated.");
			}
		}
		
		public void OnHistorical(SymbolInfo symbol) {
			// ignore for tickreader
		}
	
		public void OnStop() {
			try {
	    		readQueue.EnQueue(EntryType.Terminate, (SymbolInfo) null);
			} catch( QueueException) {
				log.Warn("Already terminated.");
			}
		}
	    
	    public void OnError(string error) {
	    	try {
	    		readQueue.EnQueue(EntryType.Error, error);
	    	} catch( QueueException) {
	    		log.Warn("Already terminated.");
	    	}
	    }
		
		public TickQueue ReadQueue {
			get { return readQueue; }
		}
		
		public void OnEndHistorical(SymbolInfo symbol)
		{
			try {
				readQueue.EnQueue(EntryType.EndHistorical, symbol);
			} catch( QueueException) {
				log.Warn("QueueException. Already terminated.");
			}
		}
		
		public void OnEndRealTime(SymbolInfo symbol)
		{
			try {
				readQueue.EnQueue(EntryType.EndRealTime, symbol);
			} catch( QueueException) {
				log.Warn("Already terminated.");
			}
		}
	}
}

#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 8/7/2009
 * Time: 7:32 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion


using System;
using System.Threading;
using TickZoom.Api;
using TickZoom.TickUtil;

namespace TickZoom.Test
{
	public class VerifyFeed : Receiver {
		private static readonly Log log = Factory.Log.GetLogger(typeof(VerifyFeed));
		private static readonly bool debug = log.IsDebugEnabled;
		private TickQueue tickQueue = Factory.TickUtil.TickQueue(typeof(VerifyFeed));
		
		public ReceiverState OnGetReceiverState(SymbolInfo symbol) {
			return ReceiverState.Ready;
		}
		
		public VerifyFeed() {
			tickQueue.StartEnqueue = Start;
		}
		
		public void Start() {
		}
        public long Verify(Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol, int timeout) {
			return Verify(2, assertTick, symbol, timeout);
		}
    	TickImpl lastTick = new TickImpl();
    	int countLog = 0;
    	TickBinary tickBinary = new TickBinary();
    	TickImpl tick = new TickImpl();
        public long Verify(int expectedCount, Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol, int timeout) {
			if( debug) log.Debug("VerifyFeed");
            int startTime = Environment.TickCount;
            count = 0;
			while( Environment.TickCount - startTime < timeout * 1000 ) {
        		if( !tickQueue.CanDequeue) Thread.Sleep(100);
        		if( tickQueue.CanDequeue) {
        			if( HandleTick(expectedCount, assertTick,symbol)) {
        				break;
        			}
        		}
        	}
            tickQueue.Terminate();
            return count;
		}

		private bool HandleTick(int expectedCount, Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol) {
			try { 
            	tickQueue.Dequeue(ref tickBinary);
            	tick.Copy(tickBinary);
				if( debug && countLog < 5)
				{
					log.Debug("Received a tick " + tick);
					countLog++;
				}
	            startTime = Environment.TickCount;
            	count++;
            	if( count > 0) {
            		assertTick(tick,lastTick,symbol.BinaryIdentifier);
            	}
            	lastTick.Copy(tick);
            	if( count >= expectedCount) return true;
			} catch( QueueException ex) {
				switch( ex.EntryType) {
					case EntryType.EndHistorical:
					case EntryType.StartRealTime:
					case EntryType.EndRealTime:
						break;
					case EntryType.Terminate:
						return true;
					default:
	            		throw new ApplicationException("Unexpected QueueException: " + ex.EntryType);
				}
			}
    		return false;
		}
		
       	long count = 0;
       	Task task;
       	int startTime;
		public void StartTimeTheFeed() {
            startTime = Environment.TickCount;
           	count = 0;
           	countLog = 0;
           	task = Factory.Parallel.Loop(this,TimeTheFeedTask);
        }
           	
        public int EndTimeTheFeed() {
  			task.Join();
            int endTime = Environment.TickCount;
            int elapsed = endTime - startTime;
            log.Notice("Processed " + count + " ticks in " + elapsed + "ms or " + (count*1000/elapsed) + "ticks/sec");
			Factory.TickUtil.TickQueue("Stats").LogStats();
			return elapsed/1000;
		}		
	
		public bool TimeTheFeedTask() {
			try {
       			if( !tickQueue.CanDequeue) return false;
            	tickQueue.Dequeue(ref tickBinary);
       			tick.Copy(tickBinary);
				if( debug && count < 5)
				{
					log.Debug("Received a tick " + tick);
					countLog++;
				}
				count++;
				if( count%1000000 == 0) {
					log.Notice("Read " + count + " ticks");
				}
				return true;
           	} catch( QueueException ex) {
       			if( EntryType.EndHistorical != ex.EntryType) {
       				throw new ApplicationException( "Unexpected QueueException: " + ex.EntryType);
       			}
            	log.Debug("Queue Terminated");
            	Factory.Parallel.CurrentTask.Stop();
            	return false;
        	}
		}
       	
		public void OnRealTime(SymbolInfo symbol) {
		}
		
		public void OnHistorical(SymbolInfo symbol) {
		}
		
		public bool CanReceive {
			get { return tickQueue != null && tickQueue.CanEnqueue; }
		}
	 		
		public void OnSend(ref TickBinary o)
		{
			try {
				tickQueue.EnQueue(ref o);
			} catch( QueueException) {
				// Queue already terminated.
			}
		}

	    public void OnPosition(SymbolInfo symbol, double signal)
	    {
	        throw new NotImplementedException();
	    }

	    public void OnStop()
		{
			try {
	    		tickQueue.EnQueue(EntryType.Terminate, (SymbolInfo) null);
			} catch( QueueException) {
				// Queue already terminated.
			}
		}
		
	    public void OnError( string error) {
	    	log.Error( error);
	    	tickQueue.Terminate();
	    }
		public void Close() {
			tickQueue.Terminate();
		}
		
		public void OnEndHistorical(SymbolInfo symbol)
		{
			tickQueue.EnQueue(EntryType.EndHistorical, symbol);
		}
		
		public void OnEndRealTime(SymbolInfo symbol)
		{
       		try {
				tickQueue.EnQueue(EntryType.EndRealTime, symbol);
       		} catch ( QueueException) {
       			// Queue was already ended.
       		}
//			Diagram.StateChange(this,receiverState);
		}
	}
}

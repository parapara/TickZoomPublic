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
using System.IO;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.TickUtil
{

	/// <summary>
	/// Description of TickArray.
	/// </summary>
	public class TickWriter
	{
		BackgroundWorker backgroundWorker;
   		int maxCount = 0;
   		SymbolInfo symbol = null;
		string fileName = null;
		Thread appendThread = null;
		protected TickQueue writeQueue;
		private static readonly Log log = Factory.Log.GetLogger(typeof(TickWriter));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		bool keepFileOpen = false;
		bool eraseFileToStart = false;
		bool logProgress = false;
		FileStream fs = null;
		MemoryStream memory = null;
		bool isInitialized = false;

		public TickWriter(bool eraseFileToStart)
		{
			this.eraseFileToStart = eraseFileToStart;
			writeQueue = Factory.TickUtil.TickQueue(typeof(TickWriter));
			writeQueue.StartEnqueue = Start;
		}
		
		public void Start() {
			
		}
		
		bool CancelPending {
			get { return backgroundWorker !=null && backgroundWorker.CancellationPending; }
		}
		
		public void Initialize(string _folder, SymbolInfo _symbol) {
			symbol = _symbol;
       		string storageFolder = Factory.Settings["AppDataFolder"];
       		if( storageFolder == null) {
       			throw new ApplicationException( "Must set AppDataFolder property in app.config");
       		}
       		
       		List<char> invalidChars = new List<char>(Path.GetInvalidPathChars());
       		invalidChars.Add('\\');
       		invalidChars.Add('/');
       		string symbolStr = _symbol.Symbol;
       		foreach( char invalid in invalidChars) {
       			symbolStr = symbolStr.Replace(new string(invalid,1),"");
       		}
       		string fileNameRoot = storageFolder + "\\" + _folder + "\\" + symbolStr + "_Tick";
			fileName = fileNameRoot+".tck";
			Initialize( fileName);
		}
		
		public void Initialize(string filePath) {
    		log.Notice("TickWriter fileName: " + fileName);
    		this.fileName = filePath;
			Directory.CreateDirectory( Path.GetDirectoryName(FileName));
			string baseName = Path.GetFileNameWithoutExtension(filePath);
			if( this.symbol == null) {
				this.symbol = Factory.Symbol.LookupSymbol(baseName.Replace("_Tick",""));
			}
			if( eraseFileToStart) {
    			File.Delete( fileName);
    			log.Notice("TickWriter file was erased to begin writing.");
    		}
			if( keepFileOpen) {
    			fs = new FileStream(fileName, FileMode.Append);
   				log.Debug("keepFileOpen - Open()");
    			memory = new MemoryStream();
			}
     		if( !CancelPending ) {
				StartAppendThread();
			}
			isInitialized = true;
		}

		protected virtual void StartAppendThread() {
			string baseName = Path.GetFileNameWithoutExtension(fileName);
	        appendThread = new Thread(AppendDataLoop);
	        appendThread.Name = baseName + " writer";
	        appendThread.IsBackground = true;
	        appendThread.Start();
		}
		
		TickBinary tick = new TickBinary();
		TickIO tickIO = new TickImpl();
		protected virtual void AppendDataLoop() {
			try { 
				while( AppendData());
			} catch( Exception ex) {
				log.Error( ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}
		
		protected virtual bool AppendData() {
			try {
				writeQueue.Dequeue(ref tick);
				tickIO.init(tick);
				if( trace) {
					log.Symbol = tickIO.Symbol;
					log.TimeStamp = tickIO.Time;
					log.Trace("Writing to file: " + tickIO);
				}
				WriteToFile(memory, tickIO);
	    		return true;
		    } catch (QueueException ex) {
				if( ex.EntryType == EntryType.Terminate) {
					log.Debug("Exiting, queue terminated.");
					if( fs != null) {
						fs.Close();
	    				log.Debug("Terminate - Close()");
					}
					return false;
				} else {
					Exception exception = new ApplicationException("Queue returned unexpected: " + ex.EntryType);
					writeQueue.Terminate(exception);
					throw ex;
				}
			} catch( Exception ex) {
				writeQueue.Terminate(ex);
				if( fs != null) {
					fs.Close();
				}
				throw;
    		}
		}
		
		private int origSleepSeconds = 3;
		private int currentSleepSeconds = 3;
		private void WriteToFile(MemoryStream memory, ReadWritable<TickBinary> tick) {
			int errorCount = 0;
			int count=0;
			do {
			    try { 
					if( !keepFileOpen) {
		    			fs = new FileStream(fileName, FileMode.Append);
			    		log.Debug("!keepFileOpen - Open()");
		    			memory = new MemoryStream();
					}
			    	tick.ToWriter(memory);
			    	CompressTick(tick.Extract(),memory);
			    	fs.Write(memory.GetBuffer(),0,(int)memory.Position);
			    	memory.SetLength(0);
			    	memory.Position = 0;
					if( !keepFileOpen) {
			    		fs.Close();
			    		log.Debug("!keepFileOpen - Close()");
			    		fs = null;
			    	}
		    		if( errorCount > 0) {
				    	log.Notice(symbol + ": Retry successful."); 
		    		}
		    		errorCount = 0;
		    		currentSleepSeconds = origSleepSeconds;
			    } catch(IOException e) { 
	    			errorCount++;
			    	log.Debug(symbol + ": " + e.Message + "\nPausing " + currentSleepSeconds + " seconds before retry."); 
			    	Factory.Parallel.Yield();
			    	if( currentSleepSeconds < 300) {
				    	currentSleepSeconds *= 2;
				    	currentSleepSeconds = Math.Min(currentSleepSeconds,60);
			    	}
			    } 
				count++;
			} while( errorCount > 0);
		}
		
		protected virtual void CompressTick(TickBinary tick, MemoryStream memory) {
		}
		
//		public void Terminate() {
//			writeQueue.Terminate();
//		}

		public void Add(TickIO tickIO) {
			if( !isInitialized) {
				throw new ApplicationException("Please initialized TickWriter first.");
			}
			TickBinary tick = tickIO.Extract();
			writeQueue.EnQueue(ref tick);
		}
		
		public bool CanReceive {
			get {
				return writeQueue.CanEnqueue;
			}
		}
		
		public virtual void Close() {
			if( !isInitialized) {
				throw new ApplicationException("Please initialized TickWriter first.");
			}
			if( debug) log.Debug("Entering Close()");
    		if( appendThread != null) {
				writeQueue.EnQueue(EntryType.Terminate, symbol);
				appendThread.Join();
				writeQueue = null;
			} else {
				throw new ApplicationException("AppendThread was null");
			}
			if( keepFileOpen) {
	    		fs.Close();
	    		log.Debug("keepFileOpen - Close()");
	    		fs = null;
	    	}
			if( debug) log.Debug("Exiting Close()");
		}
		
		public bool LogTicks = false;
		
		void progressCallback( string text, Int64 current, Int64 final) {
			if( backgroundWorker != null && backgroundWorker.WorkerReportsProgress) {
				backgroundWorker.ReportProgress(0, (object) new ProgressImpl(text,current,final));
			}
		}
		
 		public BackgroundWorker BackgroundWorker {
			get { return backgroundWorker; }
			set { backgroundWorker = value; }
		}
		
		public string FileName {
			get { return fileName; }
		}
	    
		public SymbolInfo Symbol {
			get { return symbol; }
		}
		
		public bool LogProgress {
			get { return logProgress; }
			set { logProgress = value; }
		}
   		
		public int MaxCount {
			get { return maxCount; }
			set { maxCount = value; }
		}
		
		public bool KeepFileOpen {
			get { return keepFileOpen; }
			set { keepFileOpen = value; }
		}
		
		public TickQueue WriteQueue {
			get { return writeQueue; }
		}
	}
}

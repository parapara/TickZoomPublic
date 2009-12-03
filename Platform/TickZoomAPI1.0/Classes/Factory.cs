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
 * Date: 8/9/2009
 * Time: 1:12 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.ComponentModel;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Factory.
	/// </summary>
	public static class Factory
	{
		private static EngineFactory engineFactory;
		private static readonly Log log = Factory.Log.GetLogger(typeof(Factory));
		private static object locker;
		private static LogManager logManager;
		private static TickUtilFactory tickUtilFactory;		
		private static Parallel parallel;		
		private static ProviderFactory provider;
		private static SymbolFactory symbolFactory;
		private static StarterFactory starterFactory;
		
		static Factory() {
			locker = new object();
		}
		
		private static object Locker {
			get { if( locker == null) {
					locker = new object();
				}
				return locker;
			}
		}
	
		public static EngineFactory Engine {
			get { 
				if( engineFactory == null) {
					lock(Locker) {
						if( engineFactory == null) {
							string profilerFlag = Factory.Settings["TickZoomProfiler"];
				       		string engineName;
				       		if( "true".Equals(profilerFlag)) {
				       			engineName = "TickZoomProfiler";
				       			try { 
									engineFactory = (EngineFactory) FactorySupport.Load( typeof(EngineFactory), engineName);
				       			} catch( Exception ex) {
				       				log.Notice( "ERROR: In the config file, TickZoomProfiler is set to \"true\"");
				       				log.Notice( "However, an error occurred while loading TickZoomProfiler engine.");
				       				log.Notice( "Please check the TickZoom.log for further detail.");
				       				log.Debug( "TickZoomProfiler load ERROR: " + ex);
				       				throw;
				       			}
				       		}
				       		
				       		if( engineFactory == null) {
				       			engineName = "TickZoomEngine";
								engineFactory = (EngineFactory) FactorySupport.Load( typeof(EngineFactory), engineName);
				       		}
						}
					}
				}
				return engineFactory;
			}
		}
		
		public static LogManager Log {
			get { 
				if( logManager == null) {
					lock(Locker) {
						if( logManager == null) {
							logManager = (LogManager) FactorySupport.Load(typeof(LogManager), "TickZoomLogging" );
							logManager.Configure();
						}
						
					}
				}
				return logManager;
			}
		}
		
		public static Parallel Parallel {
			get {
				if( parallel == null) {
					lock(Locker) {
						if( parallel == null) {
							parallel = Engine.Parallel();
						}
					}
				}
				return parallel;
			}
		}
		
		public static SymbolFactory Symbol {
			get {
				if( symbolFactory == null) {
					lock(Locker) {
						if( symbolFactory == null) {
							symbolFactory = (SymbolFactory) FactorySupport.Load( typeof(SymbolFactory), "TickZoomStarters" );
						}
					}
				}
				return symbolFactory;
			}
		}
		
		public static StarterFactory Starter {
			get {
				if( starterFactory == null) {
					lock(Locker) {
						if( starterFactory == null) {
							starterFactory = (StarterFactory) FactorySupport.Load( typeof(StarterFactory), "TickZoomStarters" );
						}
					}
				}
				return starterFactory;
			}
		}
		
		public static ProviderFactory Provider {
			get {
				if( provider == null) {
					lock(Locker) {
						if( provider == null) {
							provider = (ProviderFactory) FactorySupport.Load( typeof(ProviderFactory), "ProviderCommon" );
						}
					}
				}
				return provider;
			}
		}
		
		public static TickUtilFactory TickUtil {
			get {
				if( tickUtilFactory == null) {
					lock(Locker) {
						if( tickUtilFactory == null) {
							tickUtilFactory = (TickUtilFactory) FactorySupport.Load( typeof(TickUtilFactory), "TickZoomTickUtil" );
						}
					}
				}
				return tickUtilFactory;
			}
		}

	    public static bool AutoUpdate(BackgroundWorker bw) {
	    	bool retVal = false;
			log.Notice( "Attempting AutoUpdate...");
			AutoUpdate updater = new AutoUpdate();
			updater.BackgroundWorker = bw;
			
			if( updater.UpdateAll() ) {
				retVal = true;
			}
   			return retVal;
   		}
		
		public static Settings Settings {
			get {
				return new Settings();
			}
		}
	}
	

}

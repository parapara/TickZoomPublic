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

using TickZoom.Api;

namespace TickZoom.Common
{
	public class Portfolio : Strategy, PortfolioInterface
	{
		List<Strategy> strategies = new List<Strategy>();
		List<StrategyWatcher> watchers = new List<StrategyWatcher>();
		PortfolioType portfolioType = PortfolioType.None;
		double closedEquity = 0;
		double openEquity;
		
		public Portfolio()
		{
		    Performance.GraphTrades = false;
		}
	
		public sealed override void OnBeforeInitialize() {
			do {
				// Count all the unique symbols used by dependencies and
				// get a list of all the strategies.
				strategies = new List<Strategy>();
				Dictionary<string,List<Strategy>> symbolMap = new Dictionary<string,List<Strategy>>();
				for( int i=0; i<Chain.Dependencies.Count; i++) {
					Chain chain = Chain.Dependencies[i];
					Strategy strategy = null;
					for( Chain link = chain.Tail; link.Model != null; link = link.Previous) {
						strategy = link.Model as Strategy;
						if( strategy != null) {
							break;
						}
						chain = chain.Next;
					}
					if( strategy != null) {
						List<Strategy> tempStrategies;
						if( symbolMap.TryGetValue(strategy.SymbolDefault, out tempStrategies)) {
							tempStrategies.Add(strategy);
						} else {
							tempStrategies = new List<Strategy>();
							tempStrategies.Add(strategy);
							symbolMap[strategy.SymbolDefault] = tempStrategies;
						}
						strategies.Add(strategy);
					}
				}
				if(symbolMap.Count == 1) {
					portfolioType = PortfolioType.SingleSymbol;
				} else if( symbolMap.Count == strategies.Count) {
					portfolioType = PortfolioType.MultiSymbol;
				} else {
					// Remove all dependencies which have more than one obect.
					for( int i=Chain.Dependencies.Count-1; i>=0; i--) {
						Chain chain = Chain.Dependencies[i];
						if( chain.Root != chain.Tail) {
							Chain.Dependencies.RemoveAt(i);
						}
					}
					// There is a mixture of multi symbols and multi strategies per symbol.
					// Insert additional Portfolios for each symbol.
					foreach( var kvp in symbolMap) {
						string symbol = kvp.Key;
						List<Strategy> tempStrategies = kvp.Value;
						if( tempStrategies.Count > 1) {
							Portfolio portfolio = new Portfolio();
							portfolio.Name = "Portfolio-"+symbol;
							portfolio.SymbolDefault = symbol;
							foreach( var strategy in tempStrategies) {
								portfolio.Chain.Dependencies.Add(strategy.Chain);
							}
							Chain.Dependencies.Add( portfolio.Chain);
						} else {
							Strategy strategy = tempStrategies[0];
							Chain.Dependencies.Add( strategy.Chain);
						}
					}
				}
			} while( portfolioType == PortfolioType.None);
			
			// Create strategy watchers
			foreach( var strategy in strategies) {
				watchers.Add( new StrategyWatcher(strategy));
			}
		}	
		
		private class StrategyWatcher {
			private double previousPosition = 0;
			private PositionInterface position;
			
			public StrategyWatcher(Strategy strategy) {
				this.position = strategy.Performance.Position;	
			}
			
			public bool PositionChanged {
				get { return previousPosition != position.Current; }
			}
			
			public void Refresh() {
				previousPosition = position.Current;
			}
			
			public PositionInterface Position {
				get { return position; }
			}
		}
	
		public override bool OnProcessTick(Tick tick)
		{
			if( portfolioType == PortfolioType.SingleSymbol) {
				double internalSignal = 0;
				double totalPrice = 0;
				int changeCount = 0;
				foreach( var watcher in watchers) {
					internalSignal += watcher.Position.Current;
					if( watcher.PositionChanged) {
						totalPrice += watcher.Position.Price;
						changeCount++;
						watcher.Refresh();
					}
				}
				if( changeCount > 0) {
					double averagePrice = (totalPrice / changeCount).Round();
					Position.Change(internalSignal,averagePrice,Ticks[0].Time);
				}
				return true;
			} else if( portfolioType == PortfolioType.MultiSymbol) {
				double tempClosedEquity = 0;
				double tempOpenEquity = 0;
				foreach( var strategy in strategies) {
					tempOpenEquity += strategy.Performance.Equity.OpenEquity;
					tempClosedEquity += strategy.Performance.Equity.ClosedEquity;
					tempClosedEquity -= strategy.Performance.Equity.StartingEquity;
				}
				if( tempClosedEquity != closedEquity) {
					double change = tempClosedEquity - closedEquity;
					Performance.Equity.OnChangeClosedEquity(change);
					closedEquity = tempClosedEquity;
				}
				if( tempOpenEquity != openEquity) {
					openEquity = tempOpenEquity;
				}
				return true;
			} else {
				throw new ApplicationException("PortfolioType was never set.");
			}
		}
		
		public double GetOpenEquity() {
			double tempOpenEquity = 0;
			foreach( var strategy in strategies) {
				tempOpenEquity += strategy.Performance.Equity.OpenEquity;
			}
			return tempOpenEquity;
		}
		
		/// <summary>
		/// Shortcut to look at the data of and control any dependant strategies.
		/// </summary>
		public List<Strategy> Strategies {
			get { return strategies; }
		}
		
		public List<Strategy> Markets {
			get { return strategies; }
		}
		
		public PortfolioType PortfolioType {
			get { return portfolioType; }
			set { portfolioType = value; }
		}
	}

	[Obsolete("Please use Portfolio instead.",true)]
	public class PortfolioCommon : Portfolio {
		
	}
}

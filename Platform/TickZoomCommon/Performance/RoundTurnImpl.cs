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
 * Date: 9/14/2009
 * Time: 10:20 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of RoundTurnImpl.
	/// </summary>
	public class TransactionPairImpl : TransactionPair
	{
		TransactionPairBinary binary;
		
		public TransactionPairImpl()
		{
		}
		
		public TransactionPairImpl(TransactionPairBinary binary)
		{
			this.binary = binary;
		}
		
		public void init( TransactionPairBinary binary) {
			this.binary = binary;
		}
		
		public double Direction {
			get {
				return binary.Direction;
			}
		}
		
		public double EntryPrice {
			get {
				return binary.EntryPrice;
			}
		}
		
		public double ExitPrice {
			get {
				return binary.ExitPrice;
			}
		}
		
		[Obsolete("Please use TransactionPairs.GetProfitLoss() instead.",true)]
		public double ProfitLoss {
			get { return 0.0; }
		}
		
		public double MaxFavorable {
			get {
				return binary.MaxPrice;
			}
		}
		
		public double MaxAdverse {
			get {
				return binary.MinPrice;
			}
		}
		
		public int EntryBar {
			get {
				return binary.EntryBar;
			}
		}
		
		public int ExitBar {
			get {
				return binary.ExitBar;
			}
		}
		
		public TickZoom.Api.TimeStamp EntryTime {
			get {
				return binary.EntryTime;
			}
		}
		
		public TickZoom.Api.TimeStamp ExitTime {
			get {
				return binary.ExitTime;
			}
		}
		
		public bool Completed {
			get {
				return binary.Completed;
			}
		}
		
		public void SetProperties(string parameters)
		{
			binary.SetProperties(parameters);
		}
		
		public void ChangeSize(double newSize, double price)
		{
			binary.ChangeSize(newSize,price);
		}
		
		public string ToStringHeader()
		{
			return binary.ToStringHeader();
		}
		
		public override string ToString()
		{
			return binary.ToString();
		}
	}
}

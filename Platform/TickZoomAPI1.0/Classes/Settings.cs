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
using System.Configuration;
using System.IO;

namespace TickZoom.Api
{
	public class Settings {
		public string this[string name]
		{
			get { 
				if( name == "AppDataFolder") { return GetAppDataFolder(); }
				string retVal = ConfigurationManager.AppSettings[name];
				return retVal;
			}
		}
		
		private string GetAppDataFolder() {
			ConfigFile config = new ConfigFile("TickZoom");
			string retVal = config.GetValue("AppDataFolder");
			if( retVal != null) {
				if( Directory.Exists(retVal)) {
					return retVal;
				} else {
					retVal = null;
				}
			}
			bool setValue = false;
			if( retVal == null) {
				setValue = true;
				retVal = ScanForFolder();
			}
			if( retVal == null) {
				retVal = ScanForLargestDrive();
			}
			if( retVal == null) {
				throw new ApplicationException("Failed to find a drive to put TickZoomData");
			}
			if( setValue) {
				config.SetValue("AppDataFolder",retVal);
			}
			return retVal;
		}
		
		private string ScanForFolder() {
			foreach( DriveInfo drive in DriveInfo.GetDrives()) {
				string path = drive.Name + "TickZoom";
				if( Directory.Exists(path)) {
					return path;
				}
			}
			return null;
		}
		
		private string ScanForLargestDrive() {
			long maxSpace = 0;
			DriveInfo maxDrive = null;
			foreach( DriveInfo drive in DriveInfo.GetDrives()) {
				try {
					if( drive.AvailableFreeSpace > maxSpace) {
						maxDrive = drive;
						maxSpace = drive.AvailableFreeSpace;
					}
				} catch( IOException) {
					// Ignore. Try another drive.
				}
			}
			if( maxDrive != null) {
				return maxDrive + "TickZoom";
			} else {
				return null;
			}
		}
		
		private const string defaultName = "TickZoom";
	}
}

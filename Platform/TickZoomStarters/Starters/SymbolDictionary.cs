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
 * Date: 9/20/2009
 * Time: 12:23 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Common
{

    
	/// <summary>
	/// Description of SymbolDictionary.
	/// </summary>
	public class SymbolDictionary : IEnumerable<SymbolInfoCommon>
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(SymbolDictionary));
		private static object locker = new object();
		private SymbolInfoCommon @default;
		private List<SymbolCategory> categories = new List<SymbolCategory>();
		
		public SymbolDictionary()
		{
			@default = new SymbolInfoCommon();
		}

		public static SymbolDictionary Create(string name, string defaultContents) {
			lock( locker) {
				string storageFolder = Factory.Settings["AppDataFolder"];
				string dictionaryPath = storageFolder + @"\Dictionary\"+name+".tzdict";
				Directory.CreateDirectory(Path.GetDirectoryName(dictionaryPath));
				SymbolDictionary dictionary;
				if( File.Exists(dictionaryPath) ) {
					using( StreamReader streamReader = new StreamReader(dictionaryPath)) {
						dictionary = SymbolDictionary.Create( streamReader);
					}
					return dictionary;
				} else {
					string contents = BeautifyXML(defaultContents);
			        using (StreamWriter sw = new StreamWriter(dictionaryPath)) 
			        {
			            // Add some text to the file.
			            sw.Write( contents);
			        }
			        Thread.Sleep(1000);
					dictionary = SymbolDictionary.Create( new StreamReader(dictionaryPath));
				}
				return dictionary;
			}
		}
		
		
		public static SymbolDictionary Create(TextReader projectXML) {
			lock( locker) {
				SymbolDictionary project = new SymbolDictionary();
				project.Load(projectXML);
				return project;
			}
		}
		
		private static string BeautifyXML(string xml)
		{
			using( StringReader reader = new StringReader(xml)) {
				XmlDocument doc = new XmlDocument();
				doc.Load( reader);
			    StringBuilder sb = new StringBuilder();
			    XmlWriterSettings settings = new XmlWriterSettings();
			    settings.Indent = true;
			    settings.IndentChars = "  ";
			    settings.NewLineChars = "\r\n";
			    settings.NewLineHandling = NewLineHandling.Replace;
			    using( XmlWriter writer = XmlWriter.Create(sb, settings)) {
				    doc.Save(writer);
			    }
			    return sb.ToString();
			}
		}
			
		public void Load(TextReader projectXML) {
			
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			
			using (XmlReader reader = XmlReader.Create(projectXML))
			{
				try {
					bool process = true;
					// Read nodes one at a time  
					while( process)  
					{  
						reader.Read();
					    // Print out info on node  
					    switch( reader.NodeType) {
					    	case XmlNodeType.Element:
					    		if( "category".Equals(reader.Name) ) {
				    				SymbolCategory category = new SymbolCategory();
						    		HandleCategory(category,reader);
						    		categories.Add(category);
					    		} else {
					    			Error(reader,"unexpected tag " + reader.Name );
					    		}
					    		projectXML.Close();
					    		process = false;
					    		break;
					    }
					}  				
				} catch( Exception ex) {
					Error( reader, ex.ToString());
					projectXML.Close();
				}
			}
			projectXML.Close();
			projectXML.Dispose();
		}
		
		private void HandleCategory(SymbolCategory category, XmlReader reader) {
			string tagName = reader.Name;
			category.Name = reader.GetAttribute("name");
			log.Debug("Handle category " + category.Name);
			if( reader.IsEmptyElement) { return; }
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			string name = reader.GetAttribute("name");
			    			string value = reader.GetAttribute("value");
			    			HandleProperty(category.Default, reader.GetAttribute("name"), reader.GetAttribute("value"));
							log.Debug("Property " + name + " = " + value);
			    		} else if( "category".Equals(reader.Name)) {
			    			SymbolCategory subCategory = new SymbolCategory(category.Default.Copy());
			    			HandleCategory(subCategory,reader);
			    			category.Categories.Add(subCategory);
			    		} else if( "symbol".Equals(reader.Name)) {
			    			string name = reader.GetAttribute("name");
			    			string universal = reader.GetAttribute("universal");
			    			SymbolInfoCommon symbol = category.Default.Copy();
		    				symbol.Symbol = name;
		    				if( universal != null) {
//		    					symbol.UniversalSymbol = universal;
		    				}
			    			HandleSymbol(symbol,reader);
			    			category.Symbols.Add(symbol);
			    		} else {
			    			Error(reader,"unexpected tag " + reader.Name );
			    		}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + tagName + " was expected instead of end of " + reader.Name);
			    		}
			    		break;
			    } 
			}
			Error(reader,"Unexpected end of file");
			return;
		}
		
		
		private void HandleSymbol(object obj, XmlReader reader) {
			string tagName = reader.Name;
			log.Debug("Handle " + obj.GetType().Name);
			if( reader.IsEmptyElement) { return; }			
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			HandleProperty(obj, reader.GetAttribute("name"), reader.GetAttribute("value"));
			    		} else {
			    			Error(reader,"End of " + tagName + " was expected instead of end of " + reader.Name);
			    		}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + reader.Name + " tag in xml was unexpected");
			    		}
			    		break;
			    }
			}
			Error(reader,"Unexpected end of file");
		}
		
		private void HandleProperty( object obj, string name, string str) {
			PropertyInfo property = obj.GetType().GetProperty(name);
			if( property == null) {
				throw new ApplicationException( obj.GetType() + " does not have the property: " + name);
			}
			Type propertyType = property.PropertyType;
			object value = TickZoom.Api.Converters.Convert(propertyType,str);
			property.SetValue(obj,value,null);
			log.Debug("Property " + property.Name + " = " + value);
		}
		
		private void Error( XmlReader reader, string msg) {
			IXmlLineInfo lineInfo = reader as IXmlLineInfo;
			string lineStr = "";
			if( lineInfo != null) {
				lineStr += " on line " + lineInfo.LineNumber + " at position " + lineInfo.LinePosition;
			}
			log.Debug(msg + lineStr);
			throw new ApplicationException(msg + lineStr);
		}
		
		public SymbolInfoCommon Get(string symbol) {
			foreach( SymbolInfoCommon properties in this) {
				if( symbol == properties.Symbol) {
					return properties;
				}
			}
			throw new ApplicationException("Symbol " + symbol + " was not found in the dictionary.");
		}
		
		
		public IEnumerator<SymbolInfoCommon> GetEnumerator()
		{
			foreach( SymbolCategory category in categories) {
				foreach( SymbolInfoCommon properties in category) {
					yield return properties;
				}
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
#region UNIVERSAL_DICTIONARY
		public static string UniversalDictionary = @"<?xml version=""1.0"" encoding=""utf-16""?>
<category name=""Universal"">
  <category name=""Stock"">
    <property name=""Level2LotSize"" value=""100"" />
    <property name=""Level2LotSizeMinimum"" value=""1"" />
    <property name=""Level2Increment"" value=""0.10"" />
    <property name=""FullPointValue"" value=""1"" />
    <property name=""MinimumTick"" value=""0.01"" />
   	<property name=""SessionStart"" value=""08:00:00"" />
   	<property name=""SessionEnd"" value=""16:30:00"" />
    <symbol name=""MSFT"" />
    <symbol name=""IBM"" />
    <category name=""Testing"">
      <symbol name=""Design"" />
      <symbol name=""FullTick"" />
      <symbol name=""Daily4Ticks"" />
      <symbol name=""MockFull"" />
      <symbol name=""Mock4Ticks"" />
      <symbol name=""Mock4Sim"" />
      <symbol name=""Daily4Sim"" />
      <symbol name=""TXF"" />
      <symbol name=""spyTestBars"" />
    </category>
  </category>
  <category name=""Forex"">
    <property name=""Level2LotSize"" value=""10000"" />
    <property name=""Level2LotSizeMinimum"" value=""100"" />
    <property name=""Level2Increment"" value=""10"" />
    <property name=""FullPointValue"" value=""1"" />
    <property name=""MinimumTick"" value=""0.01"" />
    <category name=""4 Pip"">
      <symbol name=""USD/CHF"" universal=""USDCHF"" />
      <symbol name=""USD/CAD"" universal=""USDCAD"" />
      <symbol name=""AUD/USD"" universal=""AUDUSD"" />
      <symbol name=""USD/NOK"" universal=""USDNOK"" />
      <symbol name=""EUR/USD"" universal=""EURUSD"" />
      <symbol name=""USD/SEK"" universal=""USDSEK"" />
      <symbol name=""USD/DKK"" universal=""USDDKK"" />
      <symbol name=""GBP/USD"" universal=""GBPUSD"" />
      <symbol name=""EUR/CHF"" universal=""EURCHF"" />
      <symbol name=""EUR/GBP"" universal=""EURGBP"" />
      <symbol name=""EUR/NOK"" universal=""EURNOK"" />
      <symbol name=""EUR/SEK"" universal=""EURSEK"" />
      <symbol name=""GBP/CHF"" universal=""GBPCHF"" />
      <symbol name=""NZD/USD"" universal=""NZDUSD"" />
      <symbol name=""AUD/CHF"" universal=""AUDCHF"" />
      <symbol name=""AUD/CAD"" universal=""AUDCAD"" />
    </category>
    <category name=""2 Pip"">
      <symbol name=""USD/JPY"" />
      <category name=""Testing"">
        <symbol name=""USD_JPY"">
	    	<property name=""SessionStart"" value=""06:00:00"" />
	    	<property name=""SessionEnd"" value=""15:00:00.000"" />
		</symbol>
		<symbol name=""USD_JPY_YEARS"" />
        <symbol name=""USD_JPY_Volume"" />
        <symbol name=""TST_TST"" />
        <symbol name=""TST_VR2"" />
        <symbol name=""TST_VR3"" />
        <symbol name=""TST_VR4"" />
        <symbol name=""TST_VR5"" />
        <symbol name=""TST_VR6"" />
        <symbol name=""TST_VR7"" />
      </category>
      <symbol name=""CHF/JPY"" universal=""CHFJPY"" />
      <symbol name=""EUR/JPY"" universal=""EURJPY"" />
      <symbol name=""GBP/JPY"" universal=""GBPJPY"" />
      <symbol name=""AUD/JPY"" universal=""AUDJPY"" />
      <symbol name=""CAD/JPY"" universal=""CADJPY"" />
    </category>
  </category>
  <category name=""Futures"">
    <property name=""Level2LotSize"" value=""1"" />
    <property name=""Level2LotSizeMinimum"" value=""1"" />
    <property name=""Level2Increment"" value=""1"" />
    <property name=""FullPointValue"" value=""50"" />
    <property name=""MinimumTick"" value=""0.25"" />
      <symbol name=""/ESZ9"" />
  </category>
</category>";
#endregion

#region USER_DICTIONARY
		public static string UserDictionary = @"<?xml version=""1.0"" encoding=""utf-16""?>
<category name=""MB Trading""> 
     <category name=""Stock"">
        <property name=""FullPointValue"" value=""1"" />
        <property name=""MinimumTick"" value=""0.01"" />
        <category name=""Testing"">
            <symbol name=""FullTick""/> 
            <symbol name=""Daily4Sim""/>
            <symbol name=""TXF""/>
      <symbol name=""spyTestBars""/>
		</category>
     </category>
</category>";
#endregion
	}
}

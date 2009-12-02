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
 * Date: 3/15/2009
 * Time: 2:50 AM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Windows.Forms;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of Portfolio.
	/// </summary>
	public class PortfolioNode : TreeNode
	{
		public PortfolioNode(string name,ModelProperties properties) : base(name) {
			Tag = GetModel(name,properties);
		}
		public PortfolioNode(string name) : base(name) {
			Tag = GetModel(name,null);
		}
		
		public void Add( string name) {
			PortfolioNode node = new PortfolioNode(name);
			Nodes.Add(node);
		}
		
		public void Add( ModelProperties properties) {
			PortfolioNode node = new PortfolioNode(properties.Type,properties);
			node.Name = properties.Name;
			Nodes.Add(node);
		}
		
		private void LoadIndicators(ModelInterface model) {
			for( int i=0; i<model.Chain.Dependencies.Count; i++) {
				IndicatorInterface indicator = model.Chain.Dependencies[i].Model as IndicatorInterface;
				if( indicator != null) {
					Add( indicator.GetType().Name);
				}
			}
			
		}
		
		private object GetModel(string name,ModelProperties properties) {
			ModelInterface model = Plugins.Instance.GetModel(name);
			PropertyTable propertyTable = new PropertyTable(model);
			Starter starter = new DesignStarter();
			starter.Run(model);
			starter.Wait();
			propertyTable.UpdateAfterInitialize();
			if( properties != null) {
				model.OnProperties(properties);
				propertyTable.UpdateAfterProjectFile();
				string[] keys = properties.GetModelKeys();
				for( int i=0; i<keys.Length; i++) {
					ModelProperties nestedProperties = properties.GetModel(keys[i]);
					if( nestedProperties.ModelType == ModelType.Indicator) {
						Add( nestedProperties);
					}
				}
			} else {
				LoadIndicators(model);
			}
			return propertyTable;
		}
		
	}
}

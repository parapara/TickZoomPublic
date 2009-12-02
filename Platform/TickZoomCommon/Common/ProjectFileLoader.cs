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
 * Date: 4/2/2009
 * Time: 9:42 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Reflection;
using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of ProjectModelLoader.
	/// </summary>
	public class ProjectFileLoader : ModelLoaderCommon
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public ProjectFileLoader() {
			category = "TickZOOM";
			name = "Project File";
//			IsVisibleInGUI = false;
		}
		
		public override void OnInitialize(ProjectProperties properties) {
			FindVariables(properties.Model);
		}
		
		private void FindVariables(ModelProperties properties) {
			string[] propertyKeys = properties.GetPropertyKeys();
			for( int i=0; i<propertyKeys.Length; i++) {
				ModelProperty property = properties.GetProperty(propertyKeys[i]);
				if( property.Optimize) {
					AddVariable(property);
				}
			}
			string[] keys = properties.GetModelKeys();
			for( int i=0; i<keys.Length; i++) {
				ModelProperties nestedProperties = properties.GetModel(keys[i]);
				FindVariables(nestedProperties);
			}
		}
		
		public override void OnLoad(ProjectProperties properties)
		{
			LoadModel( properties.Model);
		}
		
		private void LoadModel( ModelProperties properties) {
			if( !QuietMode) {
				log.Debug( properties.ModelType + " " + properties.Name + ", type = " + properties.Type);
			}
			Strategy model = CreateStrategy( properties.Type, properties.Name);
			model.OnProperties(properties);
			if( !QuietMode) {
				log.Indent();
			}
			string[] keys = properties.GetModelKeys();
			for( int i=0; i<keys.Length; i++) {
				ModelProperties nestedProperties = properties.GetModel(keys[i]);
				if( nestedProperties.ModelType == ModelType.Indicator) {
					
				} else {
					// type null for performance, positionsize, exitstrategy, etc.
					if( nestedProperties.Type == null)
					{
						HandlePropertySet( model, nestedProperties);
					} else {
						LoadModel(nestedProperties);
						Strategy nestedModel = TopModel as Strategy;
						nestedModel.OnProperties(nestedProperties);
						AddDependency( model, nestedModel);
					}
				}
			}
			if( !QuietMode) {
				log.Outdent();
			}
			TopModel = model;
		}
		
		private void HandlePropertySet( ModelInterface model, ModelProperties properties) {
			if( !QuietMode) {
				log.Debug( properties.Name);
				log.Indent();
			}
			
			if( "exitstrategy".Equals(properties.Name)) {
				Strategy strategy = (Strategy) model;
				model = strategy.ExitStrategy;
			}
			if( "performance".Equals(properties.Name)) {
				Strategy strategy = (Strategy) model;
				model = strategy.Performance;
			}
			if( "positionsize".Equals(properties.Name)) {
				Strategy strategy = (Strategy) model;
				model = strategy.PositionSize;
			}
			model.OnProperties(properties);
			if( !QuietMode) {
				log.Outdent();
			}
		}
	}
}

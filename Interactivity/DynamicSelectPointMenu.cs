﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Windows.Input; // needed for ICommand
using System.Windows;

namespace Interactivity
{
    internal class DynamicSelectPointMenu : DynamicMenu
    {
        private static Dockpane2ViewModel pane2 = FrameworkApplication.DockPaneManager.Find("Interactivity_Dockpane2") as Dockpane2ViewModel;

        internal delegate void ClickAction(Tuple<string, string, long> selectedTrippTuple);
        private const string ImagePath =
          @"pack://application:,,,/Interactivity;Component/Images/esri_PntFeature.png";
        private static IList<Tuple<string, string, long>> _menuItems
                      = new List<Tuple<string, string, long>>();


        public static void SetMenuPoints(IList<Tuple<string, string, long>> tripleTuples)
        {
            _menuItems = new List<Tuple<string, string, long>>(tripleTuples);
        }

        protected override void OnPopup()
        {
            if (_menuItems == null || _menuItems.Count == 0)
            {
                this.Add("No features found", "", false, true, true);
            }
            else
            {
                
                ClickAction theAction = OnMenuItemClicked;
                Add("Select feature:", "", false, true, true);
                foreach (var tuple in _menuItems)
                {
                    string featureName = "";
                    Task t = QueuedTask.Run(() =>
                    {
                        var mapView = MapView.Active;
                        var layers =
                          mapView.Map.GetLayersAsFlattenedList()
                            .Where(l => l.Name.Equals(tuple.Item1,
                              StringComparison.CurrentCultureIgnoreCase));
                        foreach (var featureLayer in layers.OfType<FeatureLayer>())
                        {
                            QueryFilter filter = new QueryFilter
                            {
                                WhereClause = $@"{tuple.Item2} = {tuple.Item3}"
                            };

                            using (RowCursor rc = featureLayer.Search(filter))
                            {
                                while (rc.MoveNext())
                                {
                                    using (Feature feature = (Feature)rc.Current)
                                    {
                                        featureName += feature["ShortLabel"];
                                    }
                                }
                            }
                        }
                        

                    });
                    t.Wait();

                    var layer = tuple.Item1;
                    var oid = tuple.Item3;
                    Add($"Address: {featureName}",
                        ImagePath,
                        false, true, false, theAction, tuple);

                }
            }
        }

        private static void OnMenuItemClicked(Tuple<string, string, long> selectedTrippTuple)
        {
            string layerInfo = "";
            //pane2.SelectedHouses = "Hello!";
            Task t = QueuedTask.Run(() =>
            {
                
                
                
                //Selects item chosen from menu
                var mapView = MapView.Active;
                var layers =
                  mapView.Map.GetLayersAsFlattenedList()
                    .Where(l => l.Name.Equals(selectedTrippTuple.Item1,
                      StringComparison.CurrentCultureIgnoreCase));
                foreach (var featureLayer in layers.OfType<FeatureLayer>())
                {
                    // select the features with a given OID
                    var selection = featureLayer.Select(new QueryFilter()
                    {
                        WhereClause = $@"{selectedTrippTuple.Item2} = {selectedTrippTuple.Item3}"
                    });
                    ;
                    //Finds attributes of the selected item
                    using (RowCursor rc = selection.Search())
                    {
                        while (rc.MoveNext())
                        {
                            using (Feature feature = (Feature)rc.Current)
                            {
                                layerInfo += "Address: " + feature["ShortLabel"] + "\n";
                                layerInfo += "Neighborhood: " + feature["Nbrhd"] + "\n";
                                layerInfo += "Building type: " + feature["USER_Type"] + "\n";
                                try
                                {
                                    layerInfo += "Bedrooms: " + feature["USER_Bedro"] + "\n";
                                }
                                catch
                                {
                                    layerInfo += "Bedrooms: " + feature["USER_Bdrm_"] + "\n";
                                }

                                try
                                {
                                    layerInfo += "Bathrooms: " + feature["USER_Bathr"] + "\n";
                                }
                                catch
                                {
                                    layerInfo += "Bathrooms: " + feature["USER_Bath"] + "\n";
                                }
                                layerInfo += "Rent per month: $" + feature["USER_Marke"] + "\n";
                                layerInfo += "--------------------------------------\n";
                            }
                        }
                    }
                    
                    //break;
                }
                //Adds attributes to dockpane 2
                pane2.SelectedHouses += layerInfo;
            });
            

            


        }
    }
}
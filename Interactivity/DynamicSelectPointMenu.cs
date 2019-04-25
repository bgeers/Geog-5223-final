using System;
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
            string query = "";
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


                query += $@"{selectedTrippTuple.Item2} = {selectedTrippTuple.Item3}";
                QueryFilter filter = new QueryFilter
                {

                    WhereClause = query


                };

                var selection = featureLayer.Select(filter, SelectionCombinationMethod.Add);

                //Finds attributes of the selected item
                using (RowCursor rc = featureLayer.Search(filter))
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

                            layerInfo += "Gas payed by: " + feature["USER_Gas"] + "\n";
                            layerInfo += "Electric payed by: " + feature["USER_Elect"] + "\n";
                            layerInfo += "Water payed by: " + feature["USER_Water"] + "\n";


                            if (pane2.IsAmenitiesShown == true)
                                {

                                    if (feature["USER_Micro"].ToString().Contains("Micro"))
                                    {
                                        layerInfo += "Microwave: Provided\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Microwave: Not Provided\n";
                                    }

                                    if (feature["USER_Dishw"].ToString().Contains("Dish"))
                                    {
                                        layerInfo += "Dishwasher: Provided\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Dishwasher: Not Provided\n";
                                    }

                                    if (feature["USER_Centr"].ToString().Contains("Central"))
                                    {
                                        layerInfo += "Has central air\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have central air\n";
                                    }

                                    if (feature["USER_Alarm"].ToString().Contains("Alarm"))
                                    {
                                        layerInfo += "Has alarm system\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have alarm system\n";
                                    }

                                    if (feature["USER_Porch"].ToString().Contains("Porch"))
                                    {
                                        layerInfo += "Has porch\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have porch\n";
                                    }

                                    if (feature["USER_Balco"].ToString().Contains("Balcony"))
                                    {
                                        layerInfo += "Has balcony\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have balcony\n";
                                    }

                                    if (feature["USER_Laund"].ToString().Contains("Washer"))
                                    {
                                        layerInfo += "Has washer/dryer\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have washer/dryer\n";
                                    }

                                    if (feature["USER_Coin"].ToString().Contains("Coin"))
                                    {
                                        layerInfo += "Has coin op laundry\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have coin op laundry\n";
                                    }

                                    if (feature["USER_Off_S"].ToString().Contains("Off"))
                                    {
                                        layerInfo += "Has off street parking\n";
                                    }
                                    else
                                    {
                                        layerInfo += "Does not have off street parking\n";
                                    }
                                }
                                layerInfo += "--------------------------------------\n\n";
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

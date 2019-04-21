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
    internal class MapTool1 : MapTool
    {
        private Dockpane1ViewModel pane = FrameworkApplication.DockPaneManager.Find("Interactivity_Dockpane1") as Dockpane1ViewModel;
        private static Dockpane2ViewModel pane2 = FrameworkApplication.DockPaneManager.Find("Interactivity_Dockpane2") as Dockpane2ViewModel;
        public MapTool1()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            // On mouse down, check if the mouse button pressed is:
            // the left mouse button to handle zoom in
            // or the right mouse button to handle zoom out.
            // If it is handle the event.
            if (e.ChangedButton == MouseButton.Right)
            {
                
                    e.Handled = true;
                    
                
            }
        }

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            // Get the map coordinates from the click point and change the Camera to zoom in or out.
            return QueuedTask.Run(() =>
            {
                
                if (e.ChangedButton == MouseButton.Right)
                {
                    
                    
                        if (MapView.Active.Map != null)
                        {
                            MapView.Active.Map.SetSelection(null);
                        }
                    pane2.SelectedHouses = "";
                }
                
            });
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var bottomRight = new Point();
            IList<Tuple<string, string, long>> tripleTuplePoints =
                      new List<Tuple<string, string, long>>();
            var hasSelection = await QueuedTask.Run(() =>
            {
                // geometry is a point
                var clickedPnt = geometry as MapPoint;
                if (clickedPnt == null)
                {
                    System.Windows.MessageBox.Show("Clicked point is null");
                    return false;
                }
                // pixel tolerance
                var tolerance = 10;
                //Get the client point edges
                var topLeft = new Point(clickedPnt.X - tolerance, clickedPnt.Y + tolerance);
                bottomRight = new Point(clickedPnt.X + tolerance, clickedPnt.Y - tolerance);
                //convert the client points to Map points
                var mapTopLeft = MapView.Active.ClientToMap(topLeft);
                var mapBottomRight = MapView.Active.ClientToMap(bottomRight);
                //create a geometry using these points
                Geometry envelopeGeometry = EnvelopeBuilder.CreateEnvelope(mapTopLeft, mapBottomRight);
                if (envelopeGeometry == null)
                {
                    System.Windows.MessageBox.Show("envleope is null");
                    return false;
                }
                //Get the features that intersect the sketch geometry.
                var result = ActiveMapView.GetFeatures(geometry);
                foreach (var kvp in result)
                {
                    var bfl = kvp.Key;
                    // only look at points
                    if (kvp.Key.ShapeType != esriGeometryType.esriGeometryPoint) continue;
                    var layerName = bfl.Name;
                    var oidName = bfl.GetTable().GetDefinition().GetObjectIDField();
                    foreach (var oid in kvp.Value)
                    {
                        tripleTuplePoints.Add(new Tuple<string, string, long>(layerName, oidName, oid));
                    }
                }
                return true;
            });
            
            if (hasSelection)
            {
                //System.Windows.MessageBox.Show("Has selection");
                ShowContextMenu(bottomRight, tripleTuplePoints);
            }
            
            return true;
            
        }


        private void ShowContextMenu(System.Windows.Point screenLocation,
            IList<Tuple<string, string, long>> tripleTuplePoints)
        {
            var contextMenu = FrameworkApplication.CreateContextMenu("DynamicMenu_SelectPoint",
                                  () => screenLocation);
            if (contextMenu == null) return;
            DynamicSelectPointMenu.SetMenuPoints(tripleTuplePoints);
            contextMenu.Closed += (o, e) =>
            {
                // nothing to do
                System.Diagnostics.Debug.WriteLine(e);
            };
            contextMenu.IsOpen = true;
        }

        

    }
}

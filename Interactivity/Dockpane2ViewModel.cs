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


namespace Interactivity
{
    internal class Dockpane2ViewModel : DockPane
    {
        private const string _dockPaneID = "Interactivity_Dockpane2";

        protected Dockpane2ViewModel() { }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Information";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private string _SelectedHouses;
        public string SelectedHouses
        {
            get { return _SelectedHouses; }
            set
            {
                SetProperty(ref _SelectedHouses, value, () => SelectedHouses);
            }
        }

        private bool _isAmenitiesShown = false;
        /// <summary>
        /// Checks if amenities should be shown
        /// </summary>
        public bool IsAmenitiesShown
        {
            get { return _isAmenitiesShown; }
            set { SetProperty(ref _isAmenitiesShown, value, () => IsAmenitiesShown); }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Dockpane2_ShowButton : Button
    {
        protected override void OnClick()
        {
            Dockpane2ViewModel.Show();
        }
    }
}

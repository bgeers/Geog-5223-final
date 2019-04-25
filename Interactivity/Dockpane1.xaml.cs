﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace Interactivity
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        private static Dockpane2ViewModel pane2 = FrameworkApplication.DockPaneManager.Find("Interactivity_Dockpane2") as Dockpane2ViewModel;
        
        public Dockpane1View()
        {
            InitializeComponent();

            var mv = MapView.Active;
            if (mv == null)
                return;

            var layers = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (var l in layers)
            {
                cboLayerList.Items.Add(l.Name);
                cboLayerList.SelectedIndex = 0;
            }

            lstAmenities.Items.Add("Microwave");
            lstAmenities.Items.Add("Landlord pays gas");
            lstAmenities.Items.Add("Landlord pays electric");
            lstAmenities.Items.Add("Landlord pays water");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            cboLayerList.Items.Clear();
            lstAmenities.Items.Clear();
            var mv = MapView.Active;
            if (mv == null)
                return;

            var layers = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (var l in layers)
            {
                cboLayerList.Items.Add(l.Name);
                cboLayerList.SelectedIndex = 0;
            }
            lstAmenities.Items.Add("Microwave");
            lstAmenities.Items.Add("Landlord pays gas");
            lstAmenities.Items.Add("Landlord pays electric");
            lstAmenities.Items.Add("Landlord pays water");
        }

        

        private async void btnFind_Click(object sender, RoutedEventArgs e)
        {
            string layerInfo = "";
            string text = bdrmQueryMin.Text + bdrmQueryMax.Text + bthrmQueryMin.Text + bthrmQueryMax.Text + rentQueryMin.Text + rentQueryMax.Text;
            if (text == "")
                return;
            
            // get feature class first
            var mv = MapView.Active;
            string lname = cboLayerList.SelectedItem.ToString();
            FeatureLayer fl = mv.Map.FindLayers(lname).FirstOrDefault() as FeatureLayer;

            
            string query = "";
            
            //Creates the query for bedrooms
            if(bdrmQueryMin.Text != "" && bdrmQueryMax.Text != "")
            {
                query += string.Format("USER_Bdrm_ >= '{0}' AND USER_Bdrm_ <= '{1}'", bdrmQueryMin.Text, bdrmQueryMax.Text);
            }
            else if(bdrmQueryMin.Text != "")
            {
                query += string.Format("USER_Bdrm_ >= '{0}'", bdrmQueryMin.Text);
            }
            else if(bdrmQueryMax.Text != "")
            {
                query += string.Format("USER_Bdrm_ <= '{0}'", bdrmQueryMax.Text);
            }
            System.Windows.MessageBox.Show(query);
            //Adds bathrooms to query if present
            if (query != "" && (bthrmQueryMin.Text != "" || bthrmQueryMax.Text != ""))
            {
                query += " AND ";
            }

            if (bthrmQueryMin.Text != "" && bthrmQueryMax.Text != "")
            {
                query += string.Format("USER_Bath >= {0} AND USER_Bath <= {1}", bthrmQueryMin.Text, bthrmQueryMax.Text);
            }
            else if (bthrmQueryMin.Text != "")
            {
                query += string.Format("USER_Bath >= {0}", bthrmQueryMin.Text);
            }
            else if (bthrmQueryMax.Text != "")
            {
                query += string.Format("USER_Bath <= {0}", bthrmQueryMax.Text);
            }
            System.Windows.MessageBox.Show(query);
            //Adds rent range to query if present
            if (query != "" && (rentQueryMin.Text != "" || rentQueryMax.Text != ""))
            {
                
                query += " AND ";
                
            }

            if (rentQueryMin.Text != "" && rentQueryMax.Text != "")
            {
                query += string.Format("USER_Marke >= {0} AND USER_Marke <= {1}", rentQueryMin.Text, rentQueryMax.Text);
            }
            else if (rentQueryMin.Text != "")
            {
                query += string.Format("USER_Marke >= {0}", rentQueryMin.Text);
            }
            else if (rentQueryMax.Text != "")
            {
                query += string.Format("USER_Marke <= {0}", rentQueryMax.Text);
            }
            System.Windows.MessageBox.Show(query);


            //Adds microwaves to query if present
            if (lstAmenities.SelectedItems.Contains("Microwave"))
            {
                query += " AND USER_Micro = 'Microhood'";
            }
            if(lstAmenities.SelectedItems.Contains("Landlord pays gas"))
            {
                query += " AND USER_Gas = 'own'";
            }
            if (lstAmenities.SelectedItems.Contains("Landlord pays electric"))
            {
                query += " AND USER_Elect = 'own'";
            }
            if (lstAmenities.SelectedItems.Contains("Landlord pays water"))
            {
                query += " AND USER_Water = 'own'";
            }
            System.Windows.MessageBox.Show(query);

            QueryFilter filter = new QueryFilter
            {
                WhereClause = query
            };

            // MessageBox.Show(query);

            
            // select the features
            await QueuedTask.Run(() =>
            {
                try
                {
                    var selection = fl.Select(filter, SelectionCombinationMethod.Add);
                    //Finds attributes of the selected item
                    using (RowCursor rc = fl.Search(filter))
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
                                if (feature["USER_Micro"] != "")
                                {
                                    layerInfo += "Microwave: Provided\n";
                                }
                                layerInfo += "Gas payed by: " + feature["USER_Gas"] + "\n";
                                layerInfo += "Electric payed by: " + feature["USER_Elect"] + "\n";
                                layerInfo += "Water payed by: " + feature["USER_Water"] + "\n";
                                layerInfo += "--------------------------------------\n";
                            }
                        }
                    }

                    //break;
                    pane2.SelectedHouses += layerInfo;
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.Message);
                }
            });
            
            //Adds attributes to dockpane 2
            
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Shapes;

namespace ModbusRTU_Viewer
{
    /// <summary>
    /// Interaktionslogik für Informations.xaml
    /// Author: Alexander Hulbe
    /// </summary>
    public partial class Informations : Page
    {

        // Declare Variables
        public dynamic config = null;
        List<String> headers = new List<String>();
        public DataModel dataModel;
        public int slaves = 0;
        public System.IO.Ports.StopBits stopBits;
        public int Baudrate;
        public System.IO.Ports.Parity parity;

        List<DisplayRow> rows = new List<DisplayRow>();

        // Default Constructor for Information Object
        public Informations(string path)
        {
            InitializeComponent();
            LoadJson(path);
        }

        // Load JSON Data
        public void LoadJson(string path)
        {
            // Read JSON File
            using (StreamReader r = new StreamReader(path))
            {
                // Load files into Variable
                string json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject(json);

                Console.WriteLine("{0}", config);

                // Get Headers for Columns to Display Data
                var len_headers = config.Headers.Count;
                foreach (string item in config.Headers)
                {
                    var head = item.Replace("\"", "");
                    headers.Add(head);
                    GridViewColumn col = new GridViewColumn();
                    col.Width = (App.Current.MainWindow.Width - 22) / len_headers ;
                    col.Header = head;
                    View.Columns.Add(col);
                }

                // Get Amount of Slaves
                slaves = config.Slaves_Count;
                // Get Baudrate from Config file
                Baudrate = (int)config.Baudrate;
                // Set correct StopBits
                dynamic _stopBits = "";
                switch ((double)config.StopBits.Value)
                {
                    case 0:
                        _stopBits = "None";
                        break;
                    case 1:
                        _stopBits = "One";
                        break;
                    case 2:
                        _stopBits = "Two";
                        break;
                    case 1.5:
                        _stopBits = "OnePointFive";
                        break;
                    default:
                        _stopBits = "One";
                        break;
                }
                _stopBits = _stopBits.Replace("\"", "");
                Enum.TryParse<System.IO.Ports.StopBits>(_stopBits, out System.IO.Ports.StopBits stopBits);
                this.stopBits = stopBits;
                
                // Set correct Parity 
                var _parity = config.Parity.Value.Replace("\"", "");
                Enum.TryParse<System.IO.Ports.Parity>(_parity, out System.IO.Ports.Parity parity);
                this.parity = parity;


                Console.WriteLine("{0}", config.DataModel);

                // Set DataType of Value
                var _datatype = config.DataModel.DataType.Value.Replace("\"", "");
                Enum.TryParse<DataModel.DataType>(_datatype, out DataModel.DataType dataType);
                // Set Type of Register
                var _type = config.DataModel.Type.Value.Replace("\"", "");
                Enum.TryParse<DataModel.Type>(_type, out DataModel.Type type);
                

                // Create DataModell dependend on DataType
                switch (dataType)
                {
                    case DataModel.DataType.String:
                        dataModel = new DataModel(
                            config.DataModel.Name.Value,
                            config.DataModel.Address.Value,
                            type,
                            (int)config.DataModel.count_of_Addresses.Value,
                            dataType,
                            (int)config.DataModel.Length
                            );
                        dataModel.RegisterType = config.DataModel.Type.Value.ToLower();
                        break;
                    default:
                        // Check if Datamodel has a Attribute called "Format"
                        if (config.DataModel["Format"] != null)
                        {
                            dataModel = new DataModel(
                                config.DataModel.Name.Value,
                                config.DataModel.Address.Value,
                                type,
                                (int)config.DataModel.count_of_Addresses.Value,
                                dataType,
                                config.DataModel.Format.Value
                                );

                        }
                        else
                        {
                            dataModel = new DataModel(
                                config.DataModel.Name.Value,
                                config.DataModel.Address.Value,
                                type,
                                (int)config.DataModel.count_of_Addresses.Value,
                                dataType,
                                new Unit(config.DataModel.Unit.Value)
                                );
                        }
                        break;
                }
                // Add RegisterType tp DataModel
                dataModel.RegisterType = config.DataModel.Type.Value.ToLower();

                // Bind Data to Columns
                for (int i = 0; i < View.Columns.Count; i++)
                {
                    View.Columns[i].DisplayMemberBinding = new Binding(DisplayRow.getAttributes()[i]);
                }
            }

        }

        // Add a new Row to Listview to display another Device
        public bool addRow(DisplayRow row)
        {

            foreach (var item in rows)
            {
                if (row.SlaveAddress == item.SlaveAddress)
                {
                    if (row.RawValue != item.RawValue)
                    {
                        item.RawValue = row.RawValue;
                        item.Value = row.Value;
                        item.Registers = row.Registers;
                        item.Name = row.Name;
                        item.Unit = row.Unit;
                        ListView.ItemsSource = rows;
                        ListView.Items.Refresh();
                        return false;
                    }
                    return false;
                }
            }

            rows.Add(row);
            ListView.ItemsSource = rows;
            ListView.Items.Refresh();

            return false;
        }

        // EventHandler to Remove Glow in ListView
        private void GridViewColumnHeader_Loaded(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader columnHeader = sender as GridViewColumnHeader;
            Border HeaderBorder = columnHeader.Template.FindName("HeaderBorder", columnHeader) as Border;
            if (HeaderBorder != null)
            {
                HeaderBorder.Background = HeaderBorder.Background;
            }
            Border HeaderHoverBorder = columnHeader.Template.FindName("HeaderHoverBorder", columnHeader) as Border;
            if (HeaderHoverBorder != null)
            {
                HeaderHoverBorder.BorderBrush = HeaderHoverBorder.BorderBrush;
            }
            Rectangle UpperHighlight = columnHeader.Template.FindName("UpperHighlight", columnHeader) as Rectangle;
            if (UpperHighlight != null)
            {
                UpperHighlight.Visibility = UpperHighlight.Visibility;
            }
            Thumb PART_HeaderGripper = columnHeader.Template.FindName("PART_HeaderGripper", columnHeader) as Thumb;
            if (PART_HeaderGripper != null)
            {
                PART_HeaderGripper.Background = PART_HeaderGripper.Background;
            }
        }

    }
}

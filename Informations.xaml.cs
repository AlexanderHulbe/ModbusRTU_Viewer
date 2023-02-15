using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModbusRTU_Viewer
{
    /// <summary>
    /// Interaktionslogik für Informations.xaml
    /// </summary>
    public partial class Informations : Page
    {
        public dynamic config = null;
        List<String> headers = new List<String>();
        public DataModel dataModel;
        public int slaves = 0;
        public System.IO.Ports.StopBits stopBits;
        public int Baudrate;
        public System.IO.Ports.Parity parity;

        List<DisplayRow> rows = new List<DisplayRow>();
        

        public Informations(string path)
        {
            InitializeComponent();
            LoadJson(path);
        }

        public void LoadJson(string path)
        {

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject(json);

                Console.WriteLine("{0}", config);


                var len_headers = config.Headers.Count;
                foreach (string item in config.Headers)
                {
                    var head = item.Replace("\"", "");
                    headers.Add(head);
                    GridViewColumn col = new GridViewColumn();
                    col.Width = App.Current.MainWindow.Width / len_headers - 10;
                    col.Header = head;
                    View.Columns.Add(col);
                }


                slaves = config.Slaves_Count;
                Baudrate = (int)config.Baudrate;
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
                
                var _parity = config.Parity.Value.Replace("\"", "");
                Enum.TryParse<System.IO.Ports.Parity>(_parity, out System.IO.Ports.Parity parity);
                this.parity = parity;


                Console.WriteLine("{0}", config.DataModel);

                var _datatype = config.DataModel.DataType.Value.Replace("\"", "");
                Enum.TryParse<DataModel.DataType>(_datatype, out DataModel.DataType dataType);
                var _type = config.DataModel.Type.Value.Replace("\"", "");
                Enum.TryParse<DataModel.Type>(_type, out DataModel.Type type);

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
                            dataModel.RegisterType = config.DataModel.Type.Value.ToLower();
                            for (int i = 0; i < View.Columns.Count; i++)
                            {
                                View.Columns[i].DisplayMemberBinding = new Binding(DisplayRow.getAttributes()[i]);
                            }

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
                            dataModel.RegisterType = config.DataModel.Type.Value.ToLower();
                            for (int i = 0; i < View.Columns.Count; i++)
                            {
                                View.Columns[i].DisplayMemberBinding = new Binding(DisplayRow.getAttributes()[i]);
                            }
                        }
                        break;
                }
            }

        }

        public bool HasProperty( object obj, string propertyName)
        {
            Type t = obj.GetType();
            PropertyInfo p = t.GetProperty(propertyName);
            if (p == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool addRow(DisplayRow row)
        {
            rows.Add(row);
            ListView.ItemsSource = rows;

            return false;
        }

    }
}

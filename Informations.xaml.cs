using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
                    col.Width = App.Current.MainWindow.Width / len_headers;
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
                            config.DataModel.Name,
                            config.DataModel.Address,
                            type,
                            (int)config.DataModel.count_of_Addresses,
                            dataType,
                            (int)config.DataModel.Length
                            );
                        break;
                    default:
                        dataModel = new DataModel(
                            config.DataModel.Name.Value,
                            config.DataModel.Address.Value,
                            type,
                            (int)config.DataModel.count_of_Addresses.Value,
                            dataType,
                            config.DataModel.Unit.Value
                            );
                        break;
                }
            }

        }

    }
}

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
        dynamic config = null;
        List<String> headers = new List<String>();
        DataModel dataModel;
        int slaves = 0;

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
                            (int) config.DataModel.count_of_Addresses,
                            dataType,
                            (int) config.DataModel.Length
                            );
                        break;
                    default:
                        dataModel = new DataModel(
                            config.DataModel.Name.Value,
                            config.DataModel.Address.Value,
                            type,
                            (int) config.DataModel.count_of_Addresses.Value,
                            dataType,
                            config.DataModel.Unit.Value
                            );
                        break;
                }
            }
            
        }

    }
}

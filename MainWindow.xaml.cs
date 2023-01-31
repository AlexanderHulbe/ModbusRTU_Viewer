using EasyModbus;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace ModbusRTU_Viewer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<int> Baudraten = new List<int>()
                    {
                        4800,
                        9600,
                        19200,
                        38400,
                        57600,
                        115200,
                        128000,
                        250000,
                        256000
                    };
        Informations info;
        List<ModbusClient> clients = new List<ModbusClient>();
        bool firstConfigLoad = true;

        public MainWindow()
        {
            InitializeComponent();
            DD_Baudraten.ItemsSource = Baudraten;
        }

        private void btn_loadConfig_Click(object sender, RoutedEventArgs e)
        {

            if (!firstConfigLoad)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName != null)
                {
                    
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        info = new Informations("D:\\Data\\repos\\ModbusRTU_Viewer\\config.json");
                    }
                    else
                    {
                        info = new Informations(openFileDialog.FileName);
                    }
                    
                    Frame.Content = info;
                    DD_Baudraten.SelectedValue = info.Baudrate;

                    
                    addLineToDataField(info.config);

                    connectToClients("COM3");

                    readData();
                }
            }
            else
            {
                Console.WriteLine("No File Selected");
                DataField.Text += "\nUNVALID JSON CONFIG";
            }
        }

        private void connectToClients(dynamic port)
        {
            for (int i = Int32.Parse(slave_addr_start.Text); i <= info.slaves; i++)
            {
                ModbusClient modbusClient = new ModbusClient(port);
                modbusClient.UnitIdentifier = (byte) i;
                modbusClient.Baudrate = info.Baudrate;
                modbusClient.Parity = info.parity;
                modbusClient.StopBits = info.stopBits;
                modbusClient.Connect();
                clients.Add(modbusClient);
            }
        }

        private void readData()
        {
            int addr = Int32.Parse(info.dataModel.Address);
            int count = info.dataModel.Count_of_Addresses;


            foreach (var client in clients)
            {
                var data = client.ReadHoldingRegisters(addr, count);
                for (int i = 0; i < count; i++)
                {
                    Console.WriteLine("{0} {1}", addr + i, data[i]);
                }

            }
        }

        private void DD_Baudraten_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(info.Baudrate != (int)DD_Baudraten.SelectedItem)
                info.Baudrate = (int)DD_Baudraten.SelectedItem;
        }

        private void addLineToDataField(dynamic data)
        {
            var timestamp = "[" + DateTime.Now.ToString() + "]: ";
            Console.WriteLine(timestamp);
            timestamp += data;
            Console.WriteLine(timestamp);
            DataField.Text += "\n"+timestamp;
        }
    }
}

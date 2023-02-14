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

        public List<String> ComPorts;

        Informations info;
        List<ModbusClient> clients = new List<ModbusClient>();
        bool firstConfigLoad = true;
        bool isConfigLoaded = false;
        String port;

        public MainWindow()
        {
            ComPorts = GetComPorts();
            InitializeComponent();
            //DD_Baudraten.ItemsSource = Baudraten;
            DD_ComPorts.ItemsSource = ComPorts;
        }

        private List<String> GetComPorts()
        {
            // Get a list of serial port names.
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            Console.WriteLine("The following serial ports were found:");

            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }

            return new List<string>(ports);
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
                    
                    info = new Informations(openFileDialog.FileName);
                    Frame.Content = info;
                    //DD_Baudraten.SelectedValue = info.Baudrate;


                    addLineToDataField(info.config);
                    isConfigLoaded = true;

                    if (!String.IsNullOrEmpty(port))
                    {
                        connectToClients(port);
                        readData();
                        firstConfigLoad = false;
                    }

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

            string registers = "";

            for (int i = 0; i < count; i++)
            {
                registers += (addr + i).ToString() + " + ";
            }
            registers = registers.TrimEnd(' ');
            registers = registers.TrimEnd('+');
            registers = registers.TrimEnd(' ');
            foreach (var client in clients)
            {
                List<String> data = new List<String>();
                
                for (int i = 0; i < count; i++)
                {
                    var hex = client.ReadHoldingRegisters(addr + i, 1)[0].ToString("X4");
                    while (hex.Length > 4)
                    {
                        hex = hex.Substring(1);
                    }
                    if (!String.IsNullOrEmpty(hex))
                    {

                        Console.WriteLine("Addr: " + (addr + i) + " => " + hex);
                        addLineToDataField("Slave " + client.UnitIdentifier + " Addr: " + (addr + i) + " => " + hex);
                        data.Add(hex);
                    }
                }
                String val = "";
                foreach (var item in data)
                {
                    val += item;
                }

                dynamic decVal = null;

                if (info.dataModel.format != null)
                {
                    decVal = convert(val, info.dataModel.dataType,info.dataModel.format);
                }
                else
                {
                    decVal = convert(val,info.dataModel.dataType);
                }

                

                DisplayRow row = new DisplayRow(
                    client.UnitIdentifier.ToString(),
                    info.dataModel.Name,
                    registers,
                    decVal,
                    val
                    );

                info.addRow(row);
                

                Console.WriteLine("Wert in HEX: " + val);
                addLineToDataField("Wert in HEX: " + val);
                Console.WriteLine("Wert in DEC: " + decVal);
                addLineToDataField("Wert in DEC: " + decVal);
            }
        }

        private dynamic convert(string val, DataModel.DataType datatype)
        {
            dynamic response = null;
            switch (datatype)
            {
                case DataModel.DataType.uint32:

                    response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber);
                    break;

                default:
                    Console.WriteLine("Default in convert Function!");
                    break;
            }

            return response;
        }
        private dynamic convert(string val, DataModel.DataType datatype, String Format)
        {
            dynamic response = null;
            switch (datatype)
            {
                case DataModel.DataType.uint32:

                    response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber);
                    break;

                default:
                    Console.WriteLine("Default in convert Function!");
                    break;
            }
            int pos = 0;
            while (Format.IndexOf('.') > 0)
            {
                pos += Format.IndexOf('.');
                response = response.ToString().Insert(pos, ".");
                pos += 1;
                Format = Format.Substring(Format.IndexOf('.')+1);
            }

            return response;
        }

        private void addLineToDataField(dynamic data)
        {
            var timestamp = "[" + DateTime.Now.ToString() + "]: ";
            Console.WriteLine(timestamp);
            timestamp += data;
            Console.WriteLine(timestamp);
            DataField.AppendText("\n"+timestamp);
        }

        private void DD_ComPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            port = DD_ComPorts.SelectedItem.ToString();
            addLineToDataField("COM Port ausgewählt: "+port);
            if (isConfigLoaded)
            {
                firstConfigLoad = false;
                connectToClients(port);
                readData(); 
            }
        }
    }
}

using EasyModbus;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        bool WrongPort = true;

        public MainWindow()
        {
            setup();
            ComPorts = GetComPorts();
            InitializeComponent();
            //DD_Baudraten.ItemsSource = Baudraten;
            DD_ComPorts.ItemsSource = ComPorts;
        }

        private void setup()
        {
            if (!Directory.Exists(App.pathString))
            {
                Directory.CreateDirectory(App.pathString);
            }
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

        private void resetClients()
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
                clients = new List<ModbusClient>();
            }
        }

        private void btn_loadConfig_Click(object sender, RoutedEventArgs e)
        {

            resetClients();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = App.pathString;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Config Datei auswählen";
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName != null)
                {
                    
                    info = new Informations(openFileDialog.FileName);
                    Frame.Content = info;
                    //DD_Baudraten.SelectedValue = info.Baudrate;


                    //addLineToDataField(info.config);
                    addLineToDataField("New Config Loaded",true);
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
                var alreadyCon = false;

                ModbusClient modbusClient = new ModbusClient(port);
                modbusClient.UnitIdentifier = (byte) i;
                modbusClient.Baudrate = info.Baudrate;
                modbusClient.Parity = info.parity;
                modbusClient.StopBits = info.stopBits;

                foreach (var client in clients)
                {
                    if (client.Connected && client.SerialPort.ToString() == port)
                    {
                        alreadyCon = true;
                        break;
                    }
                }
                
                if (!alreadyCon)
                {
                    try
                    {
                        modbusClient.Connect();
                        if (isAlive(modbusClient))
                        {
                            clients.Add(modbusClient);
                            WrongPort = false;
                        }
                    }
                    catch (Exception e)
                    {
                        addLineToDataField(e.Message);
                        //throw;
                    }
                }
                else
                {
                    addLineToDataField("Slave "+modbusClient.UnitIdentifier + " is already Connected!");
                    WrongPort = false;
                }
            }
        }

        private bool isAlive(ModbusClient client)
        {

            for (int i = 0; i < 130; i++)
            {
                try
                {
                    var temp = client.ReadHoldingRegisters(0 + i, 1)[0];
                    return true;
                }
                catch (Exception)
                {
                    //throw;
                }
                try
                {
                    var temp = client.ReadInputRegisters(0 + i, 1)[0].ToString("X4");
                    return true;
                }
                catch (Exception)
                {
                    //throw;
                }
                
            }

            return false;
        }

        private void readData()
        {
            if (!WrongPort)
            {
                int addr = Int32.Parse(info.dataModel.Address);
                int count = info.dataModel.Count_of_Addresses;

                string registers = "";

                for (int i = 0; i < count; i++)
                {
                    registers += (addr + i).ToString() + " + ";
                }
                registers = registers.TrimEnd(' ').TrimEnd('+').TrimEnd(' ');
                foreach (var client in clients)
                {
                    List<String> data = new List<String>();

                    for (int i = 0; i < count; i++)
                    {
                        dynamic hex = "";
                        switch (info.dataModel.RegisterType)
                        {
                            case "holdingregister":
                                var _hex = client.ReadHoldingRegisters(addr + i, 1)[0];
                                hex = _hex.ToString("X4");
                                break;
                            case "inputregister":
                                hex = client.ReadInputRegisters(addr + i, 1)[0].ToString("X4");
                                break;

                            default:
                                Console.WriteLine("Default in convert Function!");
                                addLineToDataField("Default in convert Function!");
                                break;
                        }
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
                        decVal = convert(val, info.dataModel.dataType, info.dataModel.format);
                    }
                    else
                    {
                        decVal = convert(val, info.dataModel.dataType);
                    }

                    DisplayRow row = null;

                    if (info.dataModel.Unit != null)
                    {
                        row = new DisplayRow(
                            client.UnitIdentifier.ToString(),
                            info.dataModel.Name,
                            registers,
                            decVal,
                            val,
                            new Unit(info.dataModel.Unit.Name)
                            );
                    }
                    else
                    {
                        row = new DisplayRow(
                            client.UnitIdentifier.ToString(),
                            info.dataModel.Name,
                            registers,
                            decVal,
                            val
                            );
                    }

                    info.addRow(row);


                    Console.WriteLine("Slave " + client.UnitIdentifier + " Wert in HEX: " + val);
                    addLineToDataField("Slave " + client.UnitIdentifier + " Wert in HEX: " + val);
                    Console.WriteLine("Slave " + client.UnitIdentifier + " Wert in DEC: " + decVal);
                    addLineToDataField("Slave " + client.UnitIdentifier + " Wert: " + decVal);
                }
            }
            else
            {
                addLineToDataField("Kein Modbus Gerät an Port: "+port);
            }
        }

        private dynamic convert(string val, DataModel.DataType datatype)
        {
            dynamic response = null;
            switch (datatype)
            {
                case DataModel.DataType.uint32:

                    response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber)+"";
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
            var len = Format.Length;
            while (Format.IndexOf('.') > 0)
            {
                pos += Format.IndexOf('.');
                response = response.ToString().Insert(pos, ".");
                pos += 1;
                Format = Format.Substring(Format.IndexOf('.')+1);
            }
            while (response.Length > len)
            {
                response = response.Substring(0, response.Length - 1);
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
            DataField.ScrollToEnd();
        }
        private void addLineToDataField(dynamic data,bool newLine)
        {
            string timestamp = "";
            if (newLine)
                timestamp = "\n";
            timestamp += "[" + DateTime.Now.ToString() + "]: ";
            Console.WriteLine(timestamp);
            timestamp += data;
            Console.WriteLine(timestamp);
            DataField.AppendText("\n" + timestamp);
            DataField.ScrollToEnd();
        }

        private void DD_ComPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            port = DD_ComPorts.SelectedItem.ToString();
            addLineToDataField("COM Port ausgewählt: "+port);
            WrongPort = true;
            if (isConfigLoaded)
            {
                firstConfigLoad = false;
                resetClients();
                connectToClients(port);
                readData(); 
            }
        }
    }
}

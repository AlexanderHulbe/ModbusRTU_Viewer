using EasyModbus;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace ModbusRTU_Viewer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        List<int> Baudraten = new List<int>()
                    {
                        9600,
                        19200,
                        38400,
                        57600,
                        115200,
                        460800
                    };
        List<string> Units = new List<string>()
                    {
                        "N",
                        "dN",
                        "kN",
                        "MN",
                        "GN"
                    };
        public List<String> ComPorts;

        Informations info;
        List<ModbusClient> clients = new List<ModbusClient>();
        bool firstConfigLoad = true;
        bool isConfigLoaded = false;
        String port;
        int scanrate;
        bool WrongPort = true;
        DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            setup();
            ComPorts = GetComPorts();
            InitializeComponent();
            //DD_Baudraten.ItemsSource = Baudraten;
            DD_ComPorts.ItemsSource = ComPorts;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
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
            List<string> portList;
            
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                // Get a list of serial port names.
                var portnames = SerialPort.GetPortNames();
                var port = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                portList = portnames.Select(n => n + " - " + port.FirstOrDefault(s => s.Contains(n))).ToList();

                // Display each port name to the console.
                Console.WriteLine("The following serial ports were found:");
                foreach (string s in portList)
                {
                    Console.WriteLine(s);
                }

            }

            return new List<string>(portList);
        }
        private void resetClients()
        {
            if (!firstConfigLoad)
            {
                dispatcherTimer.Stop();
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
                    var confName = info.config.Name.Value;
                    addLineToDataField(confName+" Config Loaded",true);
                    isConfigLoaded = true;

                    if (!String.IsNullOrEmpty(port))
                    {
                        connectToClients(port);
                        //readData();
                        if (!WrongPort)
                            SetupTimer();
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
            var alreadyCon = false;

            ModbusClient modbusClient = new ModbusClient(port);
            modbusClient.UnitIdentifier = (byte) Int32.Parse(slave_addr_start.Text);
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
                    output(e.Message);
                    //throw;
                }
            }
            else
            {
                clients.Add(modbusClient);
                output("Slave " + modbusClient.UnitIdentifier + " is already Connected!");
                WrongPort = false;
            } 
        }
        private bool isAlive(ModbusClient client)
        {
            int addr = Int32.Parse(info.dataModel.Address);
            try
            {
                var temp = client.ReadHoldingRegisters(addr, 1)[0];
                return true;
            }
            catch (Exception)
            {
                dispatcherTimer.Stop();
                output("Kein Modbus Gerät an Port: " + port);
                return false;
                //throw;
            }
            try
            {
                var temp = client.ReadInputRegisters(addr, 1)[0].ToString("X4");
                return true;
            }
            catch (Exception)
            {
                dispatcherTimer.Stop();
                output("Kein Modbus Gerät an Port: " + port);
                return false;
                //throw;
            }

            dispatcherTimer.Stop();
            output("Kein Modbus Gerät an Port: " + port);
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
                    registers += (addr + i).ToString() + " & ";
                }
                registers = registers.TrimEnd(' ').TrimEnd('&').TrimEnd(' ');
                for (int slave = Int32.Parse(slave_addr_start.Text); slave <= info.slaves; slave++)
                {
                    List<String> data = new List<String>();
                    clients[0].UnitIdentifier = (byte) slave;

                    for (int i = 0; i < count; i++)
                    {
                        dynamic hex = "";
                        switch (info.dataModel.RegisterType)
                        {
                            case "holdingregister":
                                dynamic _hex;
                                try
                                {
                                    _hex = clients[0].ReadHoldingRegisters(addr + i, 1)[0];
                                    hex = _hex.ToString("X4");
                                }
                                catch (Exception)
                                {
                                    output("Connection TimeOut!");
                                    dispatcherTimer.Stop();
                                    InitializeComponent();
                                    throw;
                                }
                                break;
                            case "inputregister":
                                try
                                {
                                    var _addr = UInt32.Parse(addr+"", System.Globalization.NumberStyles.HexNumber);

                                    var __addr = int.Parse(_addr+"");

                                    var temp = clients[0].ReadInputRegisters(__addr + i, 1);
                                    var _temp = temp[0];
                                    hex = _temp.ToString("X4");
                                    //hex = clients[0].ReadInputRegisters(addr + i, 1)[0].ToString("X4");
                                }
                                catch (Exception)
                                {
                                    output("Connection TimeOut!");
                                    dispatcherTimer.Stop();
                                    InitializeComponent();
                                    throw;
                                }
                                break;

                            default:
                                output("Default in convert Function!");
                                break;
                        }
                        while (hex.Length > 4)
                        {
                            hex = hex.Substring(1);
                        }
                        if (!String.IsNullOrEmpty(hex))
                        {
                            output("Slave " + clients[0].UnitIdentifier + " Addr: " + (addr + i) + " => " + hex);
                            data.Add(hex);
                        }
                    }
                    String val = "";
                    String disval = "";
                    foreach (var item in data)
                    {
                        val += item;
                        disval += item + " ";
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
                            clients[0].UnitIdentifier.ToString(),
                            info.dataModel.Name,
                            registers,
                            decVal,
                            disval,
                            new Unit(info.dataModel.Unit.Name)
                            );
                    }
                    else
                    {
                        row = new DisplayRow(
                            clients[0].UnitIdentifier.ToString(),
                            info.dataModel.Name,
                            registers,
                            decVal,
                            disval
                            );
                    }

                    info.addRow(row);


                    //output("Slave " + clients[0].UnitIdentifier + " Wert in HEX: " + val);
                    output("Slave " + clients[0].UnitIdentifier + " Wert: " + decVal);
                }
            }
            else
            {
                output("Kein Modbus Gerät an Port: "+port);
            }
        }
        private void SetupTimer()
        {
            // DispatcherTimer setup
            dispatcherTimer.Stop();
            scanrate = int.Parse(scan_rate.Text); //Intervall aus Textbox
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0, scanrate); 
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (scanrate == int.Parse(scan_rate.Text))
            {
                if (isConfigLoaded && !String.IsNullOrEmpty(port))
                {
                    readData();
                }
            }
        }
        private dynamic convert(string val, DataModel.DataType datatype)
        {
            dynamic response = null;
            switch(info.config.Name.Value.ToLower())
            {
                case "bauer kraftmessdose":
                    switch (datatype)
                    {
                        case DataModel.DataType.uint32:

                            switch (info.dataModel.Name.ToLower())
                            {
                                case "unit":
                                    if (val != "0" && val != "0000")
                                    {
                                        val = val.TrimEnd('0').TrimStart('0');
                                        response = Units[int.Parse(val) - 1];
                                    }
                                    else
                                    {
                                        response = "unkown / none";
                                    }
                                    break;
                                case "baudrate":
                                    val = val.TrimEnd('0').TrimStart('0');
                                    response = ""+Baudraten[int.Parse(val)-1];
                                    break;
                                default:
                                    response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber) + "";
                                    break;
                            }
                            break;

                        default:
                            Console.WriteLine("Default in convert Function!");
                            break;
                    }
                    break;
                case "sisgeo tiltmeter":
                    switch (info.dataModel.Name.ToLower())
                    {
                        default:
                            response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber) + "";
                            break;
                    }
                    break;
                case "sisgeo inklinometer":
                    switch (info.dataModel.Name.ToLower())
                    {
                        default:
                            response = UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber) + "";
                            break;
                    }
                    break;
                default:
                    output("Unbekannter Sensor in der Config angegeben!");
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

                    response = ""+UInt32.Parse(val, System.Globalization.NumberStyles.HexNumber);
                    break;

                default:
                    Console.WriteLine("Default in convert Function!");
                    break;
            }
            int pos = 0;
            var len = Format.Length;
            if (!String.IsNullOrEmpty(response) && response != "0")
            {
                while (Format.IndexOf('.') > 0)
                {
                    pos += Format.IndexOf('.');
                    response = response.ToString().Insert(pos, ".");
                    pos += 1;
                    Format = Format.Substring(Format.IndexOf('.') + 1);
                }
                while (response.Length > len)
                {
                    response = response.Substring(0, response.Length - 1);
                }
            }
            else
            {
                response = "unkown / none";
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
            //Trim String of COM Port to only COMX
            port = DD_ComPorts.SelectedItem.ToString().Split('-')[0].Trim();
            addLineToDataField("COM Port ausgewählt: "+port);
            WrongPort = true;
            if (isConfigLoaded)
            {
                firstConfigLoad = false;
                resetClients();
                connectToClients(port);
                //readData(); 
                if (!WrongPort)
                    SetupTimer();
            }
        }
        private void scan_rate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            dispatcherTimer.Stop();
            scanrate = int.Parse(scan_rate.Text);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, scanrate);
            dispatcherTimer.Start();
        }
        private void output(dynamic data)
        {
            addLineToDataField(data);
            Console.WriteLine(data);
        }
    }
}

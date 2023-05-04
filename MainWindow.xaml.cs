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
    /// Author: Alexander Hulbe
    /// </summary>
    public partial class MainWindow : Window
    {

        // Declare and initalize needed Variables
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

        // Main Method to setup the Application
        public MainWindow()
        {
            setup();
            ComPorts = GetComPorts();
            InitializeComponent();
            //DD_Baudraten.ItemsSource = Baudraten;
            DD_ComPorts.ItemsSource = ComPorts;
            // Create dispatcherTimer to fetch the data from Modbus Clients
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
        }

        // Create Directory if it doesn't exist
        private void setup()
        {
            // Check if Directory exists
            if (!Directory.Exists(App.pathString))
            {
                // Create Directory
                Directory.CreateDirectory(App.pathString);
                var sourcePath = "./Sample config";
                if (Directory.Exists(App.pathString))
                {
                    string[] files = Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        var fileName = Path.GetFileName(s);
                        var destFile = Path.Combine(App.pathString, fileName);
                        File.Copy(s, destFile, false);
                    }
                }
            }
        }
        
        // Get a List of all active COM Ports of the Device
        private List<String> GetComPorts()
        {
            List<string> portList;
            
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                // Get a list of serial port names.
                var portnames = SerialPort.GetPortNames();
                var port = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                
                // Adds Description of COM Ports to each item in the List
                portList = portnames.Select(n => n + " - " + port.FirstOrDefault(s => s.Contains(n))).ToList();

                // Display each port name to the console
                Console.WriteLine("The following serial ports were found:");
                foreach (string s in portList)
                {
                    Console.WriteLine(s);
                }

            }
            // Return the List of COM Ports
            return new List<string>(portList);
        }
        
        // Disconnect from all Modbus Clients
        private void resetClients()
        {
            // Check if the first Config was loaded
            // if not then there are no possible Connections
            if (!firstConfigLoad)
            {
                // Stop fetching Data from the Modbus Clients
                dispatcherTimer.Stop();

                // Try to Disconnect from each Client
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
        
        // Load Config on Button Click
        private void btn_loadConfig_Click(object sender, RoutedEventArgs e)
        {
            
            // Disconnect from all Clients if there are any
            resetClients();

            // Create FileDialog to select Config File
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set Default Directory to Application Directory in Documents
            openFileDialog.InitialDirectory = App.pathString;
            // Disable Multiselect
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Config Datei auswählen";
            // Only allow .json Files
            openFileDialog.Filter = "Json files (*.json)|*.json";
            // Open FileDialog
            if (openFileDialog.ShowDialog() == true)
            {
                // Check if a File was selected
                if (openFileDialog.FileName != null)
                {
                    // Create Inforamtion object from selected JSON File
                    info = new Informations(openFileDialog.FileName);
                    // Set Information object as Mainwindow Content
                    Frame.Content = info;

                    // Get Config Name
                    var confName = info.config.Name.Value;
                    // Print it to DataField
                    addLineToDataField(confName+" Config Loaded",true);
                    // Set ConfigLoaded to True
                    isConfigLoaded = true;

                    // Check if a COM Port is selected
                    if (!String.IsNullOrEmpty(port))
                    {
                        // Connect to clients on selected COM Port
                        connectToClients(port);
                        // Check if a ModbusDevice is on the selected COM Port
                        if (!WrongPort)
                            // Start DispatchTimer to fetch Data
                            SetupTimer();
                        // Set FirstConfigLoad to True
                        firstConfigLoad = false;
                    }

                }
            }
            else
            {
                // Output that no File was Selected
                Console.WriteLine("No File Selected");
                DataField.Text += "\nUNVALID JSON CONFIG";
            }
        }
        
        // Connect to 
        private void connectToClients(dynamic port)
        {
            // Variable to check if a Modbus Client is already connected
            var alreadyCon = false;

            // Create a ModbusClient
            ModbusClient modbusClient = new ModbusClient(port);
            // Set the Modbus Slave ID to Client
            modbusClient.UnitIdentifier = (byte) Int32.Parse(slave_addr_start.Text);
            // Set Baudrate from Config to ModbusClient
            modbusClient.Baudrate = info.Baudrate;
            // Set Parity from Config to ModbusClient
            modbusClient.Parity = info.parity;
            // Set StopBits from Config to ModbusClient
            modbusClient.StopBits = info.stopBits;

            // Check if any ModbusClient is already Connected at the selected COM Port
            foreach (var client in clients)
            {
                if (client.Connected && client.SerialPort.ToString() == port)
                {
                    // Modbus Client is already Connected at this COM Port
                    alreadyCon = true;
                    break;
                }
            }

            // Check if no ModbusClient is Connected
            if (!alreadyCon)
            {
                try
                {
                    // Connect to a new ModbusClient
                    modbusClient.Connect();
                    // Check if the actual ModbusAdapter at COM Port is reachable
                    if (isAlive(modbusClient))
                    {
                        // Add ModbusClient to modbusClient List
                        clients.Add(modbusClient);
                        // Verify Port
                        WrongPort = false;
                    }
                }
                catch (Exception e)
                {
                    output(e.Message);
                    throw;
                }
            }
            else
            {
                // Add ModbusClient to modbusClient List
                clients.Add(modbusClient);
                output("Slave " + modbusClient.UnitIdentifier + " is already Connected!");
                // Verify Port
                WrongPort = false;
            } 
        }
        
        // Check if ModbusClient is reachable
        private bool isAlive(ModbusClient client)
        {
            // SlaveID of Modbus Device
            int addr = Int32.Parse(info.dataModel.Address);
            // Switch on RegisterType defined in config
            switch (info.dataModel.type)
            {
                case DataModel.Type.HoldingRegister:
                    // Try reaching Modbusdevice on RegisterAddress From Config as HoldingRegister
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
                case DataModel.Type.InputRegister:
                    // Try reaching Modbusdevice on RegisterAddress From Config as InputRegister
                    try
                    {
                        var _addr = UInt32.Parse(addr + "", System.Globalization.NumberStyles.HexNumber);
                        var __addr = int.Parse(_addr + "");
                        var temp = client.ReadInputRegisters(__addr, 1)[0];
                        return true;
                    }
                    catch (Exception)
                    {
                        dispatcherTimer.Stop();
                        output("Kein Modbus Gerät an Port: " + port);
                        return false;
                        //throw;
                    }
            }

            dispatcherTimer.Stop();
            output("Kein Modbus Gerät an Port: " + port);
            return false;
        }
        
        // Fetch Data from ModbusDevice
        private void readData()
        {
            // Verify COM Port
            if (!WrongPort)
            {
                // Registeraddress from Config
                int addr = Int32.Parse(info.dataModel.Address);
                // Count of how many Registers will be read to build one Value
                int count = info.dataModel.Count_of_Addresses;

                string registers = "";

                // Create Registeraddress String to display it properly
                for (int i = 0; i < count; i++)
                {
                    registers += (addr + i).ToString() + " & ";
                }
                registers = registers.TrimEnd(' ').TrimEnd('&').TrimEnd(' ');

                // Read Data for each Modbus Device
                for (int slave = Int32.Parse(slave_addr_start.Text); slave <= info.slaves; slave++)
                {
                    // Create a List of strings that will contain the read data
                    List<String> data = new List<String>();
                    // Set the slaveID
                    clients[0].UnitIdentifier = (byte) slave;

                    String disval = "";
                    string[] RawData = null;
                    dynamic decVal = null;

                    // read all the registers for one Value
                    for (int i = 0; i < count; i++)
                    {
                        dynamic hex = "";
                        
                        // Switch on RegisterType
                        switch (info.dataModel.type)
                        {
                            case DataModel.Type.HoldingRegister:
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
                            case DataModel.Type.InputRegister:
                                try
                                {
                                    if(info.config.Name.Value.ToLower() == "sisgeo tiltmeter")
                                    {
                                        Tiltmeter tilt = new Tiltmeter();
                                        Tiltmeter.Options wanted = Tiltmeter.Options.Null;
                                        switch (info.config.DataModel.Name.Value.ToLower())
                                        {
                                            case "temperatur":
                                                wanted = Tiltmeter.Options.Temperature;
                                                break;
                                            case "siny":
                                                wanted = Tiltmeter.Options.SinY;
                                                break;
                                            case "sinx":
                                                wanted = Tiltmeter.Options.SinX;
                                                break;
                                            case "rawy":
                                                wanted = Tiltmeter.Options.RawY;
                                                break;
                                            case "rawx":
                                                wanted = Tiltmeter.Options.RawX;
                                                break;
                                            case "rawtemperatur":
                                                wanted = Tiltmeter.Options.RawTemperature;
                                                break;
                                            default:
                                                break;
                                        }

                                        var temp = tilt.get(wanted, clients[0]);
                                        RawData = temp[0].Split(' ');
                                        disval = temp[0];
                                        decVal = temp[1];
                                        tilt = null;
                                    }
                                    else
                                    {
                                        var _addr = UInt32.Parse(addr + "", System.Globalization.NumberStyles.HexNumber);

                                        var __addr = int.Parse(_addr + "");

                                        var temp = clients[0].ReadInputRegisters(__addr + i, 1);
                                        var _temp = temp[0];
                                        hex = _temp.ToString("X4");
                                    }

                                    
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
                        if (info.config.Name.Value.ToLower() != "sisgeo tiltmeter")
                        {
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
                        else
                        {
                            output("Slave " + clients[0].UnitIdentifier + " Addr: " + (addr + i) + " => " + RawData[i]);
                        }
                    }
                    
                    
                    if (info.config.Name.Value.ToLower() != "sisgeo tiltmeter")
                    {
                        String val = "";
                        foreach (var item in data)
                        {
                            val += item;
                            disval += item + " ";
                        }

                        if (info.dataModel.format != null)
                        {
                            decVal = convert(val, info.dataModel.dataType, info.dataModel.format);
                        }
                        else
                        {
                            decVal = convert(val, info.dataModel.dataType);
                        }

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

                    // Add new Row to Information Object
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
        // Setup DispatchTimer to fetch Data
        private void SetupTimer()
        {
            // DispatcherTimer setup
            dispatcherTimer.Stop();
            scanrate = int.Parse(scan_rate.Text); //Intervall aus Textbox
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0, scanrate); 
            dispatcherTimer.Start();
        }
        
        // DispatchTimer to fetch Data
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Check if scanrate stayed the same
            if (scanrate == int.Parse(scan_rate.Text))
            {
                if (isConfigLoaded && !String.IsNullOrEmpty(port))
                {
                    readData();
                }
            }
        }
        
        // Convert retrieved HEX Data to needed Type
        private dynamic convert(string val, DataModel.DataType datatype)
        {
            dynamic response = null;
            // Switch on Sensor Name
            switch(info.config.Name.Value.ToLower())
            {
                case "bauer kraftmessdose":
                    // Switch on datatype
                    switch (datatype)
                    {
                        case DataModel.DataType.uint32:
                            
                            // Switch on Name to handle Exeptions
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
                        // There should only be Data of Datatype uint32
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

        // Convert retrieved HEX Data to needed Type and Format
        private dynamic convert(string val, DataModel.DataType datatype, String Format)
        {
            dynamic response = null;
            // Convert to DEC
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
            // Check if data was converted correctly
            if (!String.IsNullOrEmpty(response) && response != "0")
            {
                // Format data to given Format
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
                // if data is 0 or conversion failed
                response = "unkown / none";
            }
            
            return response;
        }
        
        // Add a Line to OutputField including Timestamp
        // with optional new Line at the start
        private void addLineToDataField(dynamic data,bool newLine = false)
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
        
        // Eventhandler of COM Port Dropdown if the Selection changed
        private void DD_ComPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //Trim String of COM Port to only COM[Number]
            port = DD_ComPorts.SelectedItem.ToString().Split('-')[0].Trim();
            // Output the selected COM Port
            output("COM Port ausgewählt: "+port);
            // Reset the Verification of COM Port
            WrongPort = true;
            // Check if a Config is loaded
            if (isConfigLoaded)
            {
                // if a Config is loaded then the first Load is definitely passed
                firstConfigLoad = false;
                // Reset and Try to Connect to all Modbus Devices on new COM Port
                resetClients();
                connectToClients(port);
                // Verify ModbusDevice on COM Port
                if (!WrongPort)
                    // Fetch Data
                    SetupTimer();
            }
        }
        
        // Eventhandler of ScanRate InputFiled if Text changed
        private void scan_rate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Stop Fetching Data
            dispatcherTimer.Stop();
            // Get new TimeInterval
            scanrate = int.Parse(scan_rate.Text);
            // Set new TimeInterval
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, scanrate);
            // Start Fetching Data
            dispatcherTimer.Start();
        }
        
        // Output Data to DataField and Console if executed in IDE
        private void output(dynamic data)
        {
            addLineToDataField(data);
            Console.WriteLine(data);
        }
    }
}

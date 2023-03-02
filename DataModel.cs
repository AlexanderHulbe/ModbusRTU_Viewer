using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace ModbusRTU_Viewer
{
    public class DataModel
    {
        // Declare Variables 
        public enum Type
        {
            HoldingRegister,
            InputRegisters,
            SingleRegister
        }

        public enum DataType
        {
            String,
            float32,
            int32,
            uint32,
            int16,
            uint16,
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public Type type { get; set; }
        public int Count_of_Addresses { get; set; }
        public DataType dataType { get; set; }
        public Unit Unit { get; set; }
        public int Length_of_DataTypeString { get; set; }
        public String format { get; set; }
        public List<String> Value { get; set; }
        public String RegisterType { get; set; }

        // Default Constructor for DataModel
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType) {
            // Transport parameters to created Object
            Name = name;
            //Address = address.TrimStart(trimChars: '4');
            Address = address;
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
        }

        // Constructor with Format for Displaying Data for DataModel
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, string Format) {
            // Transport parameters to created Object
            Name = name;
            //Address = address.TrimStart(trimChars: '4');
            Address = address;
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.format = Format;
        }

        // Constructor with Unit for Displaying Data for DataModel
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, Unit Unit) {
            // Transport parameters to created Object
            Name = name;
            //Address = address.TrimStart(trimChars: '4');
            Address = address;
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.Unit = Unit;
        }
        
         //Added for Future implementation
         //Constructor for StringData for DataModel
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, int Length_of_DataTypeString) {
            // Transport parameters to created Object
            Name = name;
            //Address = address.TrimStart(trimChars: '4');
            Address = address;
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.Length_of_DataTypeString = Length_of_DataTypeString;
        }
    }
}

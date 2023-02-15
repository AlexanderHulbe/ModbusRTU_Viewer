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




        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType) {
            Name = name;
            Address = address.TrimStart(trimChars: '4');
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
        }
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, string Format) {
            Name = name;
            Address = address.TrimStart(trimChars: '4');
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.format = Format;
        }
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, Unit Unit) {
            Name = name;
            Address = address.TrimStart(trimChars: '4');
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.Unit = Unit;
        }
        public DataModel(string name, string address, Type type, int Count_of_Addresses, DataType dataType, int Length_of_DataTypeString) {
            Name = name;
            Address = address.TrimStart(trimChars: '4');
            this.type = type;
            this.Count_of_Addresses = Count_of_Addresses;
            this.dataType = dataType;
            this.Length_of_DataTypeString = Length_of_DataTypeString;
        }
    }
}

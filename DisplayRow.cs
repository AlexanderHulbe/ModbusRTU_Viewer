using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusRTU_Viewer
{
    public class DisplayRow
    {
        // Declare Variables
        public string SlaveAddress { get; set; }
        public string Name { get; set; }
        public string Registers { get; set; }
        public string Value { get; set; }
        public string RawValue { get; set; }
        public String Unit { get; set; }


        // Default Constructor for DisplayRow
        public DisplayRow(string SlaveAddress, string Name, string Registers, string Value, string RawValue)
        {
            // Transport parameters to created Object
            this.SlaveAddress = SlaveAddress;
            this.Name = Name;
            this.Registers = Registers;
            this.Value = Value;
            this.RawValue = RawValue;
        }

        // Default Constructor for DisplayRow with Unit
        public DisplayRow(string SlaveAddress, string Name, string Registers, string Value, string RawValue, Unit Unit)
        {
            // Transport parameters to created Object
            this.SlaveAddress = SlaveAddress;
            this.Name = Name;
            this.Registers = Registers;
            this.Value = Value;
            this.RawValue = RawValue;
            this.Unit = Unit.Name;
        }

        // Get Attributes of DisplayRow
        public static List<String> getAttributes()
        {
            return new List<string> {
                "SlaveAddress",
                "Registers",
                "RawValue",
                "Name",
                "Value",
                "Unit"
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusRTU_Viewer
{
    public class DisplayRow
    {
        public string SlaveAddress { get; set; }
        public string Name { get; set; }
        public string Registers { get; set; }
        public string Value { get; set; }
        public string RawValue { get; set; }
        public String Unit { get; set; }

        public DisplayRow(string SlaveAddress, string Name, string Registers, string Value, string RawValue)
        {
            this.SlaveAddress = SlaveAddress;
            this.Name = Name;
            this.Registers = Registers;
            this.Value = Value;
            this.RawValue = RawValue;
        }
        public DisplayRow(string SlaveAddress, string Name, string Registers, string Value, string RawValue, Unit Unit)
        {
            this.SlaveAddress = SlaveAddress;
            this.Name = Name;
            this.Registers = Registers;
            this.Value = Value;
            this.RawValue = RawValue;
            this.Unit = Unit.Name;
        }

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

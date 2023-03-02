using System;

namespace ModbusRTU_Viewer
{
    public class Unit
    {
        // Declare Variable
        public String Name { get; set; }

        // Default Constructor for Unit
        public Unit(String Unit)
        {
            this.Name = Unit;
        }

    }
}

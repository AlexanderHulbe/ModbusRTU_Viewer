using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModbusRTU_Viewer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\", "ModbusRTU_Viewer");
        //var pathString = "N:\\Local\\hbx\\";
    }
}

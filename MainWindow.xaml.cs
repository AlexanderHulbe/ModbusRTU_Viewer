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

        public MainWindow()
        {
            InitializeComponent();
            DD_Baudraten.ItemsSource = Baudraten;
        }

        private void btn_loadConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName != null)
                {
                    Informations info = new Informations(openFileDialog.FileName);
                    //Informations info = new Informations("D:\\Data\\repos\\ModbusRTU_Viewer\\config.json");
                    Frame.Content = info;
                }
            }
            else
            {
                Console.WriteLine("No File Selected");
                DataField.Text += "\nUNVALID JSON CONFIG";
            }
        }
    }
}

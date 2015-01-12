using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FadeCandySharpSample
{
    /// <summary>
    /// Interaction logic for SimpleSample.xaml
    /// </summary>
    public partial class SimpleSample : Window
    {
        FadeCandyDeviceFactory m_factory;
        FadeCandyGridPanel m_panel;
        MainWindow m_mainWindow;

        public SimpleSample(MainWindow mainWindow)
        {
            InitializeComponent();

            m_mainWindow = mainWindow;
        }

        ~SimpleSample()
        {
            if (m_panel != null)
            {
                m_panel.ClearAll();
            }
        }

        // Step 1: Setup the server
        private void ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_factory = new FadeCandyDeviceFactory("127.0.0.1", 7890);
                ShowStatus("Status: Connected!");
                GetDevicesAndRegisterPanels.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ShowStatus("Error: " + ex.Message);
            }
        }

        // Step 2: Get the connected devices and setup whatever panel types you want.
        private void GetDevicesAndRegisterPanels_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the devices
                List<FadeCandyDevice> devices = m_factory.GetDevices();
                ShowStatus("Status: Got Devices!");

                // Make sure we have at least one panel.
                if(devices.Count == 0)
                {
                    throw new Exception("No devices found!");
                }

                // Get the first device (why not?)
                FadeCandyDevice device = devices[0];

                // Create a Grid panel and register it, if it is not already created
                if (m_panel == null)
                {
                    m_panel = new FadeCandyGridPanel(8,8);

                    device.AddPanel(0, m_panel);
                }

                ShowStatus("Status: Added Panels!");

                // Call start drawing to begin!
                device.StartDrawing();

                // Enable the other buttons.
                AllBlue.IsEnabled = true;
                AllRed.IsEnabled = true;
                AllGreen.IsEnabled = true;
                AllRandom.IsEnabled = true;
                Clear.IsEnabled = true;

                ShowStatus("Status: All Set Up!");
            }
            catch (Exception ex)
            {
                ShowStatus("Error: " + ex.Message);
            }
        }

        private void AllGreen_Click(object sender, RoutedEventArgs e)
        {
            for (int w = 0; w < 8; w++)
            {
                for (int h = 0; h < 8; h++)
                {
                    m_panel.SetPixel(w,h, 0, 80, 0);
                }
            }

            ShowStatus("Status: Changed to green, 20% bright!");
        }

        private void AllRed_Click(object sender, RoutedEventArgs e)
        {
            for (int w = 0; w < 8; w++)
            {
                for (int h = 0; h < 8; h++)
                {
                    m_panel.SetPixel(w, h, 80, 0, 0);
                }
            }

            ShowStatus("Status: Changed to red, 20% bright!");
        }

        private void AllBlue_Click(object sender, RoutedEventArgs e)
        {
            for (int w = 0; w < 8; w++)
            {
                for (int h = 0; h < 8; h++)
                {
                    m_panel.SetPixel(w, h, 0, 0, 80);
                }
            }

            ShowStatus("Status: Changed to blue, 20% bright!");
        }

        private void AllRandom_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            for (int w = 0; w < 8; w++)
            {
                for (int h = 0; h < 8; h++)
                {
                    m_panel.SetPixel(w, h, (byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255));
                }
            }

            ShowStatus("Status: Changed to random!");
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            m_panel.ClearAll();

            ShowStatus("Status: Cleared");
        }

        public void ShowStatus(string text)
        {
            Status.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
               new Action(delegate()
               {
                   Status.Text = text;
               }));
        }     
    }
}

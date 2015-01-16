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
    /// This sample is a little more advance, it shows how to register for the draw call back
    /// and update the frame every time before we draw.
    /// </summary>
    public partial class SimpleAnimatedSample : Window, IFadeCandyPanelDrawCallback
    {
        const int PANEL_SIZE = 8;

        FadeCandyDeviceFactory m_factory;
        FadeCandyGridPanel m_panel;
        Random m_rand;
        bool m_random;
        bool m_hasStarted;
        bool m_isStopped;
        int m_counter;
        int m_round;
        double m_brightness;

        public SimpleAnimatedSample()
        {
            InitializeComponent();
            m_round = 0;
            m_counter = 0;
            m_random = false;
            m_rand = new Random();
            m_brightness = 0.2;
        }

        ~SimpleAnimatedSample()
        {
            if(m_panel != null)
            {
                m_panel.ClearAll();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            m_isStopped = false;
            if (m_hasStarted)
            {
                return;
            }
            m_hasStarted = true;           

            // Setup the factory
            if(m_factory == null)
            {
                try
                {
                    m_factory = new FadeCandyDeviceFactory("127.0.0.1", 7890);
                    ShowStatus("Status: Connected!");
                }
                catch (Exception ex)
                {
                    ShowStatus("Error: " + ex.Message);
                }
            }

            // Setup the panel
            if(m_panel == null)
            {
                try
                {
                    // Get the devices
                    List<FadeCandyDevice> devices = m_factory.GetDevices();
                    ShowStatus("Status: Got Devices!");

                    // Make sure we have at least one panel.
                    if (devices.Count == 0)
                    {
                        throw new Exception("No devices found!");
                    }

                    // Get the first device (why not?)
                    FadeCandyDevice device = devices[0];

                    // Create a Grid panel and register it, if it is not already created
                    if (m_panel == null)
                    {
                        m_panel = new FadeCandyGridPanel(PANEL_SIZE, PANEL_SIZE);

                        device.AddPanel(0, m_panel);
                    }

                    ShowStatus("Status: Added Panels!");

                    // Set us as a callback so we get notification just before we draw
                    m_panel.SetDrawCallback(this);

                    // Call start drawing to begin!
                    device.StartDrawing(33);

                    ShowStatus("Status: Running!");
                }
                catch (Exception ex)
                {
                    ShowStatus("Error: " + ex.Message);
                }
            }
        }

        public void PrepareForDraw()
        {
            if(m_isStopped)
            {
                m_panel.ClearAll();
                return;
            }

            // This function is called by the panel just before each draw.
            int x = m_counter % PANEL_SIZE;
            int y = (int)Math.Floor(m_counter / PANEL_SIZE * 1.0);

            // Set the pixel color
            byte r, b, g;
            if (m_random)
            {
                r = (byte)m_rand.Next(255);
                b = (byte)m_rand.Next(255);
                g = (byte)m_rand.Next(255); 
            }
            else
            {
                r = (byte)(m_round == 0 ? 255 : 0);
                b = (byte)(m_round == 1 ? 255 : 0);
                g = (byte)(m_round == 2 ? 255 : 0);
            }

            // Adjust to the brightness
            r = (byte)(m_brightness * r);
            b = (byte)(m_brightness * b);
            g = (byte)(m_brightness * g);

            // Set the pixel!
            m_panel.SetPixel(x, y, r, g, b);

            // Up and reset the counters.
            m_counter++;
            if (m_counter % PANEL_SIZE == 0)
            {
                m_counter = m_rand.Next(PANEL_SIZE) * PANEL_SIZE;
                m_round++;

                if (m_round == 3)
                {
                    m_round = 0;
                }
            }
        }

        public void ShowStatus(string text)
        {
            Status.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
               new Action(delegate()
               {
                   Status.Text = text;
               }));
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            m_isStopped = true;
        }

        private void UseColor_Click(object sender, RoutedEventArgs e)
        {
            m_random = false;
        }

        private void UseRandom_Click(object sender, RoutedEventArgs e)
        {
            m_random = true;
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_brightness = (e.NewValue / 10);
        }
    }
}

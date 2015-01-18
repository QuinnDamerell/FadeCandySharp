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
    /// Interaction logic for PongGame.xaml
    /// </summary>
    public partial class PongGame : Window, IFadeCandyPanelDrawCallback
    {
        static int PANEL_SIZE = 8;
        bool m_isSetup = false;
        bool m_isRunning = false;
        FadeCandyDeviceFactory m_factory;
        FadeCandyGridPanel m_panel;
        PongPaddle m_leftPaddle;
        PongPaddle m_rightPaddle;
        Key m_lastKeyPressed;

        public PongGame()
        {
            InitializeComponent();
            m_leftPaddle = new PongPaddle(0);
            m_lastKeyPressed = Key.Escape;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (m_isRunning)
            {
                return;
            }
            m_isRunning = true;

            if (!m_isSetup)
            {
                m_isSetup = true;

                // Setup the factory
                if (m_factory == null)
                {
                    try
                    {
                        m_factory = new FadeCandyDeviceFactory("127.0.0.1", 7890);
                    }
                    catch (Exception ex)
                    {
                        ShowMessageBox("Error opening server: "+ex.Message);
                        m_isSetup = false;
                    }
                }

                // Setup the panel
                if (m_panel == null)
                {
                    try
                    {
                        // Get the devices
                        List<FadeCandyDevice> devices = m_factory.GetDevices();

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

                        // Set us as a callback so we get notification just before we draw
                        m_panel.SetDrawCallback(this);

                        // Call start drawing to begin!
                        device.StartDrawing(33);
                    }
                    catch (Exception ex)
                    {
                        ShowMessageBox("Error: " + ex.Message);
                        m_isSetup = false;
                    }
                }
            } // isSetup           
        }

        public void PrepareForDraw()
        {
            if (!m_isRunning)
            {
                m_panel.ClearAll();
                return;
            }

            // Update the game state
            m_leftPaddle.UpdateState(m_lastKeyPressed);
            m_lastKeyPressed = Key.Escape;

            m_panel.ClearAll();
            m_leftPaddle.Draw(m_panel);

            


        }

        public void ShowMessageBox(string text)
        {
            MessageBox.Show(text);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Get key presses
            m_lastKeyPressed = e.Key;
        }
    }
}

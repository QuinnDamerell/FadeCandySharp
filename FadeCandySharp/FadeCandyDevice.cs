﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace FadeCandySharp
{
    public class FadeCandyDevice
    {
        // Object stuff
        DeviceData m_deviceDetails;
        FadeCandyDeviceFactory m_factory;
        Timer m_drawTimer;        

        // Options
        bool m_isDrawing = false;
        bool m_dithering = true;
        bool m_interpolate = true;
        LightStatus m_lightStatus = LightStatus.Auto;

        // Panels
        List<KeyValuePair<int, IFadeCandyPanel>> m_panelList;
         
        // Device information, this reflects the json object
        // sent from the server. 
        public class DeviceData
        {
            public string type;
            public string serial;
            public string timestamp;
            public string version;
            public int bcd_version;
        };     
   
        // States of the device light.
        public enum LightStatus
        {
            On,
            Off,
            Auto
        }

        // Constructor
        public FadeCandyDevice(FadeCandyDeviceFactory factory, DeviceData jDevice)
        {
            m_deviceDetails = jDevice;
            m_factory = factory;
            m_panelList = new List<KeyValuePair<int, IFadeCandyPanel>>();
        }

        #region Device Props

        // Gets the device details.
        public DeviceData GetDeviceData()
        {
            return m_deviceDetails;
        }

        public void SetLightStatus(LightStatus status)
        {
            m_lightStatus = status;
            SetDeviceProps();
        }
        
        public void SetDithering(bool dither)
        {
            m_dithering = dither;
            SetDeviceProps();
        }

        public void SetInterpolate(bool interpolate)
        {
            m_interpolate = interpolate;
            SetDeviceProps();
        }

        private void SetDeviceProps()
        {
            m_factory.SetDeviceOptions(m_deviceDetails, m_lightStatus, m_dithering, m_interpolate);
        }

        #endregion

        #region Panel Stuff

        // Used to set a panel into the device.
        public void AddPanel(int position, IFadeCandyPanel panel)
        {
            if(position < 0)
            {
                throw new Exception("The position can't be negative!");
            }

            // Set ourselves into the panel
            panel.SetDevice(this);

            // Special case the first
            if(m_panelList.Count == 0)
            {
                m_panelList.Add(new KeyValuePair<int, IFadeCandyPanel>(position, panel));
                return;
            }
            
            // Set the panel into the list, we need to keep these in order
            for(int i = 0; i < m_panelList.Count; i++)
            {
                if (m_panelList[i].Key == position)
                {
                    throw new Exception("A panel of that number already exists!");
                }

                if(m_panelList[i].Key < position)
                {
                    continue;
                }
                else
                {
                    m_panelList.Insert(i, new KeyValuePair<int, IFadeCandyPanel>(position, panel));
                    return;
                }
            }

            // If we didn't add it, this is the last panel
            m_panelList.Insert(m_panelList.Count, new KeyValuePair<int, IFadeCandyPanel>(position, panel));            
        }

        public IFadeCandyPanel RemovePanel(int position)
        {
            // Find the panel...
            for (int i = 0; i < m_panelList.Count; i++)
            {
                if (m_panelList[i].Key == position)
                {
                    // ... remove it
                    IFadeCandyPanel panel = m_panelList[i].Value;
                    m_panelList.Remove(m_panelList[i]);
                    return panel;
                }
            }

            // Not found
            return null;
        }

        #endregion

        #region Drawing

        // Start the device drawing, this is one of the two modes
        // of operation.
        public void StartDrawing(int frequencyMs)
        {
            if(frequencyMs < 0)
            {
                throw new Exception("The frequency can't be less than 0!");
            }

            if (m_drawTimer == null)
            {
                m_drawTimer = new Timer();
                m_drawTimer.AutoReset = true;
                m_drawTimer.Elapsed += Draw;
                m_drawTimer.Interval = frequencyMs;
            }
            m_drawTimer.Enabled = true;
        }      

        // Stops the drawing tick.
        public void StopDrawing()
        {
            if (m_drawTimer != null)
            {
                m_drawTimer.Enabled = false;
            }
        }

        // Draws if the draw timer is not running. This is the second options of drawing
        // where the consumer request draws to happen.
        public void DrawIfTimerNotRunning()
        {
            if(m_drawTimer == null || !m_drawTimer.Enabled)
            {
                InvokeDraw();
            }
        }

        // This function invokes a draw instantly.
        public void InvokeDraw()
        {
            Draw(null, null);
        }

        // Main draw loop, this will call the draw function on each 
        // attached panel.
        void Draw(object sender, ElapsedEventArgs e)
        {
            // Don't draw if we are already.
            if(m_isDrawing)
            {
                Debug.WriteLine("Draw was issued while we are drawing!");
                return;
            }
            m_isDrawing = true;

            if(m_panelList.Count == 0)
            {
                // Nothing to do
                return;
            }

            // Ask all of the devices for a pixels
            List<PanelPixel> pixelList = new List<PanelPixel>();

            // This will itterate through in order and get all of the pixels from the panels
            foreach(KeyValuePair<int, IFadeCandyPanel> panel in m_panelList)
            {
                try
                {
                    pixelList.AddRange(panel.Value.Draw());
                }
                catch(Exception ex)
                {
                    Trace.WriteLine("Exception while calling draw; Exception "+ex.Message);
                }                
            }

            // Make our raw array
            int arrayLength = (pixelList.Count * 3) + 4;
            byte[] rawPixelArray = new byte[arrayLength];

            // Set our first 4
            rawPixelArray[0] = 0;
            rawPixelArray[1] = 0;
            rawPixelArray[2] = 0;
            rawPixelArray[3] = 0;

            // Set the pixels into the array
            for (int count = 0; count < pixelList.Count; count++)
            {
                rawPixelArray[count * 3 + 4] = pixelList[count].m_red;
                rawPixelArray[count * 3 + 5] = pixelList[count].m_green;
                rawPixelArray[count * 3 + 6] = pixelList[count].m_blue;
            }

            // Send the raw data
            try
            {
                m_factory.SendRawData(rawPixelArray);
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Failed to send pixels to the device; Exception "+ex.Message);
            }

            // indicate we are done.
            m_isDrawing = false;
        }

        #endregion
    }
}

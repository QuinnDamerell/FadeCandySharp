using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FadeCandySharp
{
    public class FadeCandyDevice
    {
        DeviceData m_deviceDetails;
        FadeCandyDeviceFactory m_factory;
        Timer m_drawTimer;

        public class DeviceData
        {
            public string type;
            public string serial;
            public string timestamp;
            public string version;
            public int bcd_version;
        };        

        public FadeCandyDevice(FadeCandyDeviceFactory factory, DeviceData jDevice)
        {
            m_deviceDetails = jDevice;
            m_factory = factory;
        }

        public void StartDrawing()
        {
            m_factory.SetDeviceOptions(m_deviceDetails, null, true, true);
            if (m_drawTimer == null)
            {
                m_drawTimer = new Timer();
                m_drawTimer.AutoReset = true;
                m_drawTimer.Elapsed += Draw;
                m_drawTimer.Interval = 500;
            }
            m_drawTimer.Enabled = true;
        }      

        public void StopDrawing()
        {
            if (m_drawTimer != null)
            {
                m_drawTimer.Enabled = false;
            }
        }

        public void SetAll()
        {
           
            
        }

        int lastccound = 0;

        void Draw(object sender, ElapsedEventArgs e)
        {
            byte[] array = new byte[200];
            Random rand = new Random();
            rand.NextBytes(array);
            array[0] = 0;
            array[1] = 0;
            array[2] = 0;
            array[3] = 0;
                

            for(int i = 0; i < lastccound; i++)
            {
                array[i] = 150;
            }

            lastccound++;
            if (lastccound >= 180)
            {
                lastccound = 0;
            }

            m_factory.SendRawData(array);

        }
    }
}

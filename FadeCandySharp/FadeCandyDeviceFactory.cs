using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FadeCandySharp
{
    public class FadeCandyDeviceFactory
    {
        private string m_server;
        private int m_port;
        private ClientWebSocket m_webSocket = null;

        private class JsonDevicesResponse
        {
            public string type;
            public List<FadeCandyDevice.DeviceData> devices; 
        }

        public FadeCandyDeviceFactory(string server, int port)
        {
            if(String.IsNullOrWhiteSpace(server) || port < 1 || port > 999999999)
            {
                throw new Exception("You must set a valid adress and port!");
            }

            // Set them
            m_server = server;
            m_port = port;
        }

        public List<FadeCandyDevice> GetDevices()
        {
            // Ask the server for a list of devices
            string response = SendMessage("{ \"type\": \"list_connected_devices\" }");

            // Parse the response
            JsonDevicesResponse responseObj = JsonConvert.DeserializeObject<JsonDevicesResponse>(response);

            List<FadeCandyDevice> devices = new List<FadeCandyDevice>();
            foreach (FadeCandyDevice.DeviceData jDevice in responseObj.devices)
            {
                FadeCandyDevice device = new FadeCandyDevice(this, jDevice);
                devices.Add(device);
            }

            return devices;
        }

        public void SendRawData(byte[] data)
        {
            EnsureConnection();

            // Get the data
            ArraySegment<byte> sendBytes = new ArraySegment<byte>(data);
            Exception ex = null;

            // Send it
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(async (object obj) =>
                {
                    try
                    {
                        CancellationToken token = new CancellationToken();
                        await m_webSocket.SendAsync(sendBytes, WebSocketMessageType.Binary, true, token);
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                    are.Set();
                });
                are.WaitOne();
            }

            if (ex != null)
            {
                ResetConnection();
                throw ex;
            }
        }

        public void SetDeviceOptions(FadeCandyDevice.DeviceData deviceData, bool? light, bool dither, bool interpolate)
        {
            string lightStr = light.HasValue ? (light.Value ? "true" : "false") : "null";
            string ditherStr = dither ? "true" : "false";
            string interStr = interpolate ? "true" : "false";

            string message = "{\"type\": \"device_options\", \"device\": { \"type\": \"fadecandy\", \"serial\": \"" + deviceData.serial + "\" }, \"options\": {\"led\": " + lightStr + ", \"dither\":" + ditherStr + ", \"interpolate\":"+interStr+"}}";
            SendMessage(message);
        }

        public void SendPixels(FadeCandyDevice.DeviceData deviceData)
        {
            string message = "{\"type\": \"device_pixels\", \"device\": {\"type\": \"fadecandy\",\"serial\": \""+ deviceData.serial +"\"},\"pixels\": [255, 0, 0,0, 255, 0, 0, 0, 255]}";
            SendMessage(message);
        }


        private void EnsureConnection()
        {
            if (m_webSocket != null)
            {
                return;
            }
              
            Exception ex = null;
            m_webSocket = new ClientWebSocket();
                
            // Try to connect
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(async (object obj)=>
                    {
                        try
                        {
                            CancellationToken token = new CancellationToken();
                            Uri ur = new Uri("ws://" + m_server + ":" + m_port, UriKind.Absolute);
                            await m_webSocket.ConnectAsync(ur, token);
                        }
                        catch(Exception e)
                        {
                            ex = e;
                        }
                        are.Set();
                    });
                are.WaitOne();
            }

            if(ex != null)
            {
                ResetConnection();
                throw ex;
            }
        }

        private void ResetConnection()
        {
            m_webSocket = null;
        }

        private string SendMessage(string message)
        {
            EnsureConnection();

            // Get the bytes to send
            ArraySegment<byte> sendBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message));
            Exception ex = null;

            // Send them
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(async (object obj) =>
                {
                    try
                    {
                        CancellationToken token = new CancellationToken();               
                        await m_webSocket.SendAsync(sendBytes, WebSocketMessageType.Text, true, token);
                    }
                    catch(Exception e)
                    {
                        ex = e;
                    }
                    are.Set();
                });
                are.WaitOne();
            }

            if(ex != null)
            {
                ResetConnection();
                throw ex;
            }

            // Make a buffer to recieve bytes
            string response = "";

            // Get a response
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(async (object obj) =>
                {
                    try
                    {
                        bool keepReading = true;
                        while (keepReading)
                        {
                            // Set up buffers
                            byte[] recieveArray = new byte[30000];
                            ArraySegment<byte> recieveBytes = new ArraySegment<byte>(recieveArray);

                            // Get the response
                            CancellationToken token = new CancellationToken();
                            WebSocketReceiveResult result = await m_webSocket.ReceiveAsync(recieveBytes, token);

                            // Add the response
                            response += Encoding.ASCII.GetString(recieveBytes.Array, 0, result.Count);

                            keepReading = (result.Count == 30000 || !result.EndOfMessage);      
                        }                        
                    }
                    catch(Exception e)
                    {
                        ex = e;
                    }
             
                    are.Set();
                });
                are.WaitOne();
            }

            if(ex != null)
            {
                ResetConnection();
                throw ex;
            }

            return response;
        }
    }
}

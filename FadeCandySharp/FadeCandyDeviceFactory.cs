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
    // This class is the base class for everything. 
    // It owns the connection to the server and creating devices.
    public class FadeCandyDeviceFactory
    {
        private string m_server;
        private int m_port;
        private ClientWebSocket m_webSocket = null;
        List<FadeCandyDevice> m_attachedDevices;

        // Used to reflect the get devices API call.
        private class JsonDevicesResponse
        {
            public string type;
            public List<FadeCandyDevice.DeviceData> devices; 
        }

        // Main constructor of the factory
        public FadeCandyDeviceFactory(string server, int port)
        {
            if(String.IsNullOrWhiteSpace(server) || port < 1 || port > 999999999)
            {
                throw new Exception("You must set a valid adress and port!");
            }

            m_attachedDevices = new List<FadeCandyDevice>();

            // Set them
            m_server = server;
            m_port = port;
        }

        // Returns a list of devices, devices own the LEDs on them.
        public List<FadeCandyDevice> GetDevices()
        {
            // Ask the server for a list of devices
            string response = SendMessage("{ \"type\": \"list_connected_devices\" }");

            // Parse the response
            JsonDevicesResponse responseObj = JsonConvert.DeserializeObject<JsonDevicesResponse>(response);

            // Loop through the response, add or remove devices if they are new
            foreach (FadeCandyDevice.DeviceData jDevice in responseObj.devices)
            {
                // Check if the device is already created
                bool alreadyExists = false;
                foreach (FadeCandyDevice searchDevice in m_attachedDevices)
                {
                    if(searchDevice.GetDeviceData().serial == jDevice.serial)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if(!alreadyExists)
                {
                    // Create the new device
                    FadeCandyDevice device = new FadeCandyDevice(this, jDevice);
                    m_attachedDevices.Add(device);
                }                
            }
            return m_attachedDevices;
        }

        // This will send raw data to (one?) device. This should only be called by
        // the devices. This should only be called by a device class.
        public void SendRawData(byte[] data)
        {
            EnsureConnection();

            // Get the data
            ArraySegment<byte> sendBytes = new ArraySegment<byte>(data);
            Exception ex = null;

            // Lock the socket and send it.
            lock (m_webSocket)
            {
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
            }

            if (ex != null)
            {
                ResetConnection();
                throw ex;
            }            
        }

        // Sets the options for a device. This should only be called by the device class.
        public void SetDeviceOptions(FadeCandyDevice.DeviceData deviceData, FadeCandyDevice.LightStatus status, bool dither, bool interpolate)
        {
            string lightStr = "null";
            switch(status)
            {
                case FadeCandyDevice.LightStatus.Auto:
                    lightStr = "null";
                    break;
                case FadeCandyDevice.LightStatus.On:
                    lightStr = "true";
                    break;
                case FadeCandyDevice.LightStatus.Off:
                    lightStr = "false";
                    break;
                default:
                    throw new Exception("Unknown light status");
            }
            string ditherStr = dither ? "true" : "false";
            string interStr = interpolate ? "true" : "false";

            string message = "{\"type\": \"device_options\", \"device\": { \"type\": \"fadecandy\", \"serial\": \"" + deviceData.serial + "\" }, \"options\": {\"led\": " + lightStr + ", \"dither\":" + ditherStr + ", \"interpolate\":"+interStr+"}}";
            SendMessage(message);
        }

        // Makes sure there is a valid connection.
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

        // Resets the connection if there is an error
        private void ResetConnection()
        {
            m_webSocket = null;
        }

        // Sends a json message to the server, returns the response as a string.
        private string SendMessage(string message)
        {
            EnsureConnection();

            // Get the bytes to send
            ArraySegment<byte> sendBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message));
            Exception ex = null;

            // Lock the websocket and send
            lock (m_webSocket)
            {
                using (AutoResetEvent are = new AutoResetEvent(false))
                {
                    ThreadPool.QueueUserWorkItem(async (object obj) =>
                    {
                        try
                        {
                            CancellationToken token = new CancellationToken();
                            await m_webSocket.SendAsync(sendBytes, WebSocketMessageType.Text, true, token);
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }
                        are.Set();
                    });
                    are.WaitOne();
                }
            }

            if(ex != null)
            {
                ResetConnection();
                throw ex;
            }

            // Make a buffer to recieve bytes
            string response = "";

            // lock and get a response
            lock (m_webSocket)
            {
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
                        catch (Exception e)
                        {
                            ex = e;
                        }

                        are.Set();
                    });
                    are.WaitOne();
                }
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

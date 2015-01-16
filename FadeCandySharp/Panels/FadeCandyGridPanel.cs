using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharp
{
    // This is an example implementation of a panel. This panel is made to represent a grid of x by y size.
    // The panel class can be extended for any type of LED array, be it a circle, strip, or grid.
    public class FadeCandyGridPanel : IFadeCandyPanel
    {
        // Internal things.
        int m_width;
        int m_height;
        byte[,,] m_pixels;

        // Context info
        FadeCandyDevice m_device;
        List<IFadeCandyPanelDrawCallback> m_drawCallbacks;

        // Main constructor
        public FadeCandyGridPanel(int width, int height)
        {
            if(m_width < 0 || m_height < 0)
            {
                throw new Exception("The size has to be larger than 0!");
            }
            m_width = width;
            m_height = height;
            m_pixels = new byte[width,height,3];
            m_drawCallbacks = new List<IFadeCandyPanelDrawCallback>();
        }

        // This function sets the rgb values for one pixel.
        public void SetPixel(int x, int y, byte red, byte green, byte blue)
        {
            m_pixels[x, y, 0] = red;
            m_pixels[x, y, 1] = green;
            m_pixels[x, y, 2] = blue;    
        
            if(m_device != null)
            {
                // Draw if the timer isn't running.
                m_device.DrawIfTimerNotRunning();
            }
        }

        // This is how a consume of the SDK sets to get draw call backs.
        // The callback will be called just before each draw
        public void SetDrawCallback(IFadeCandyPanelDrawCallback callback)
        {
            m_drawCallbacks.Add(callback);
        }

        //This is how the device sets itself into the panel
        public void SetDevice(FadeCandyDevice device)
        {
            m_device = device;
        }

        // This is the main draw call from the device. The panel should turn it's output into a list
        // of pixels that will be sent to the device. 
        public List<PanelPixel> Draw()
        {
            foreach(IFadeCandyPanelDrawCallback callback in m_drawCallbacks)
            {
                try
                { 
                    callback.PrepareForDraw();
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Prepare for draw call bac");
                }
            }

            List<PanelPixel> pixels = new List<PanelPixel>();
            for (int w = 0; w < m_width; w++)
            {
                for(int h = 0; h < m_height; h++)
                {
                    PanelPixel pixel = new PanelPixel();
                    pixel.m_red = m_pixels[w,h,0];
                    pixel.m_green = m_pixels[w,h,1];
                    pixel.m_blue = m_pixels[w,h,2];
                    pixels.Add(pixel);
                }
            }

            return pixels;
        }

        // Can be used to clear the entire gird with one call.
        public void ClearAll()
        {
            for (int w = 0; w < m_width; w++)
            {
                for (int h = 0; h < m_height; h++)
                {
                    m_pixels[w, h, 0] = 0;
                    m_pixels[w, h, 1] = 0;
                    m_pixels[w, h, 2] = 0;
                }
            }

            if (m_device != null)
            {
                // Draw if the timer isn't running.
                m_device.DrawIfTimerNotRunning();
            }
        }
    }
}

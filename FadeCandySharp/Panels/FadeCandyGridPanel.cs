using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharp
{
    public class FadeCandyGridPanel : FadeCandyPanel
    {
        int m_width;
        int m_height;

        byte[,,] m_pixels;

        public FadeCandyGridPanel(int width, int height)
        {
            m_width = width;
            m_height = height;
            m_pixels = new byte[width,height,3];
        }

        public void SetPixel(int x, int y, byte red, byte green, byte blue)
        {
            m_pixels[x, y, 0] = red;
            m_pixels[x, y, 1] = green;
            m_pixels[x, y, 2] = blue;            
        }

        public List<PanelPixel> Draw()
        {
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
        }
    }
}

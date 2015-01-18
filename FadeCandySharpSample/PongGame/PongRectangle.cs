using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharpSample
{
    public class PongRectangle : IDrawable
    {
        int m_x = 0;
        int m_y = 0;
        int m_heigth = 1;
        int m_width = 1;

        public PongRectangle(int height, int width, int x, int y)
        {
            m_heigth = height;
            m_width = width;
            m_y = y;
            m_x = x;
        }

        public void Draw(FadeCandyGridPanel panel)
        {
            for (int drawX = 0; drawX < m_width; drawX++)
            {
                for (int drawY = 0; drawY < m_heigth; drawY++)
                {
                    panel.SetPixel(m_x + drawX, m_y + drawY, 100, 100, 100);
                }
            }
        }

        public int GetX()
        {
            return m_x;
        }

        public int GetY()
        {
            return m_y;
        }

        public int GetHeight()
        {
            return m_heigth;
        }

        public int GetWidth()
        {
            return m_width;
        }

        public void SetX(int x)
        {
            m_x = x;
        }

        public void SetY(int y)
        {
            m_y = y;
        }

        public void SetWidth(int width)
        {
            m_width = width;
        }

        public void SetHeight(int height)
        {
            m_heigth = height;
        }
    }
}

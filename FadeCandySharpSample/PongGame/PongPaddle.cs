using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FadeCandySharpSample
{
    public class PongPaddle : IDrawable
    {
        int m_xOffset;
        int m_gridWidth;
        int m_gridHeight;

        PongRectangle m_rect;

        public PongPaddle(int xOffset, int gridWidth, int gridHeight)
        {
            m_xOffset = xOffset;
            m_gridHeight = gridHeight;
            m_gridWidth = gridWidth;
            m_rect = new PongRectangle(3, 1, m_xOffset, 0);
        }

        public void Draw(FadeCandyGridPanel panel)
        {
            m_rect.Draw(panel);            
        }

        public void UpdateState(Key lastKeyPressed)
        {
           if(lastKeyPressed == Key.Up)
           {
               int oldY = m_rect.GetY() - 1;
               if(oldY < 0)
               {
                   oldY = 0;
               }
               m_rect.SetY(oldY);
           }
           else if(lastKeyPressed == Key.Down)
           {
               int oldY = m_rect.GetY() +1;
               if (oldY + m_rect.GetHeight() > m_gridHeight)
               {
                   oldY = m_gridHeight - m_rect.GetHeight();
               }
               m_rect.SetY(oldY);
           }
        }
    }
}

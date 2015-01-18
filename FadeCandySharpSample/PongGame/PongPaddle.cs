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
        int m_yOffset;
        PongRectangle m_rect;

        public PongPaddle(int yOffset)
        {
            m_yOffset = yOffset;
            m_rect = new PongRectangle(3, 1, 0, m_yOffset);
        }

        public void Draw(FadeCandyGridPanel panel)
        {
            m_rect.Draw(panel);            
        }

        public void UpdateState(Key lastKeyPressed)
        {
           if(lastKeyPressed == Key.Up)
           {
               m_rect.SetX(m_rect.GetX() + 1);
           }
           else if(lastKeyPressed == Key.Down)
           {
               m_rect.SetX(m_rect.GetX() - 1);
           }
        }
    }
}

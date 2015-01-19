using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharpSample
{
    public class PongBall : IDrawable
    {
        int m_gridWidth;
        int m_gridHeight;
        PongRectangle m_rect;
        double velocityAngle;

        public PongBall(int gridWidth, int gridHeight)
        {
            m_gridHeight = gridHeight;
            m_gridWidth = gridWidth;
            m_rect = new PongRectangle(2, 2, 4, 4);
            velocityAngle = 90;
        }

        public void UpdateState()
        {
           // int x = Math.Sin(velocityAngle);
           // int y = Math.Cos(velocityAngle);

            // Bounds check.
        }

        public void Draw(FadeCandyGridPanel panel)
        {
            m_rect.Draw(panel);
        }
    }
}

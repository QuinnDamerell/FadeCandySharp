using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharp
{
    public interface IFadeCandyPanel
    {
        // Called by the device when the panel should draw.
        // This returns a list of pixels in order of drawing.
        List<PanelPixel> Draw();

        // Used to set the device to the panel.
        void SetDevice(FadeCandyDevice device);
    }
}

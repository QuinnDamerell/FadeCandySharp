using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandySharp
{
    public interface IFadeCandyPanelDrawCallback
    {
        // This is called just before draw will be called on the panel. 
        // Consumers should use this as the main tick in their application
        // to draw things like animations and game graphics.
        void PrepareForDraw();
    }
}

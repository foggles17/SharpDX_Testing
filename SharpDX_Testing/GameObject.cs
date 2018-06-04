using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX_Testing
{
    public class GameObject
    {
        TCM_GraphicsObject display;
        public GameObject(TCM_Graphics g)
        {
            display = new TCM_DrawingObject(g, new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 1), new SharpDX.Mathematics.Interop.RawColor4(.5f, .5f, .5f, 1), 2);
            ((TCM_DrawingObject)display).setGeometry(TCM_DrawingObject.Shape.rectangle, -60, -60, 120, 120);
        }
        public void setPlacement(float x, float y, float w, float h)
        {
            ((TCM_DrawingObject)display).setGeometry(TCM_DrawingObject.Shape.rectangle, x, y, w, h);
        }
        public TCM_GraphicsObject getGraphicsObject()
        {
            return display;
        }
    }
}

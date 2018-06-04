using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1;

using Color = SharpDX.Mathematics.Interop.RawColor4;

namespace SharpDX_Testing
{
    public abstract class TCM_GraphicsObject
    {
        public TCM_GraphicsObject parent;
        //public List<TCM_GraphicsObject> frontChildren;
        //public List<TCM_GraphicsObject> backChildren;
        //public RawMatrix3x2 transform;
        public TCM_Graphics graphics;

        public TCM_GraphicsObject(TCM_Graphics g)
        {
            transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
            frontChildren = new List<TCM_GraphicsObject>();
            backChildren = new List<TCM_GraphicsObject>();
            parent = null;
            graphics = g;
        }

        public abstract void innerDraw();
        public abstract void setPlacement(float x, float y, float w, float h);

        public void draw()
        {
            //apply transform
            graphics.applyTransform(transform);

            //draw stuff behind
            for (int i = 0; i < backChildren.Count; i++)
            {
                backChildren[i].draw();
            }

            //draw this
            innerDraw();

            //draw stuff in front
            for (int i = 0; i < frontChildren.Count; i++)
            {
                frontChildren[i].draw();
            }

            //undo transform
            graphics.revertTransform();
        }

        public void setTransform(float a1, float b1, float a2, float b2, float a3, float b3)
        {
            transform = new RawMatrix3x2(a1, b1, a2, b2, a3, b3);
        }
        public void resetTransform()
        {
            transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
        }
        public void applyTransform(RawMatrix3x2 a)
        {
            transform = TCM_Matrix3x2.multiply(a, transform);
        }
        public void addChild(TCM_GraphicsObject o, bool back)
        {
            o.parent = this;
            if (back)
                backChildren.Add(o);
            else
                frontChildren.Add(o);
        }
    }
    public class TCM_LayerObject : TCM_GraphicsObject
    {
        public float x, y;
        public TCM_LayerObject(TCM_Graphics g) : base(g)
        {
            x = 0;
            y = 0;
        }
        public void setPlacement(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override void innerDraw() { }

        public override void setPlacement(float x, float y, float w, float h)
        {
            throw new NotImplementedException();
        }
    }
    public class TCM_DrawingObject : TCM_GraphicsObject
    {
        public enum Shape { rectangle, ellipse }
        public Color fillColor;
        public Color outlineColor;
        public float lineWidth;
        public Geometry shape;
        
        public TCM_DrawingObject(TCM_Graphics g, Color fillColor, Color outlineColor, float lineWidth) : base(g)
        {
            this.fillColor = fillColor;
            this.outlineColor = outlineColor;
            this.lineWidth = lineWidth;
            shape = graphics.createGeometry(Shape.rectangle, 0, 0, 1, 1);
        }
        public override void innerDraw()
        {
            //draw this
            graphics.drawDrawingObject(this);
        }
        public void setGeometry(Shape s, float x, float y, float w, float h)
        {
            shape = graphics.createGeometry(s, x, y, w, h);
        }
        public override void setPlacement(float x, float y, float w, float h)
        {
            
        }
    }
    public class TCM_BitmapObject : TCM_GraphicsObject
    {
        public Bitmap bitmap;
        public float x, y, w, h;
        public float transparency;

        public TCM_BitmapObject(TCM_Graphics g, string filename) : base(g)
        {
            setBitmap(filename);
            transparency = 1;
            x = 0;
            y = 0;
            w = 1;
            h = 1;
        }

        public override void innerDraw()
        {
            //draw this
            graphics.drawBitmapObject(this);
        }

        public void setBitmap(string filename)
        {
            bitmap = graphics.loadBitmapFromFile(filename);
        }

        public override void setPlacement(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
    }
    public class TCM_TextObject : TCM_GraphicsObject
    {
        public string text;
        public SharpDX.DirectWrite.TextFormat format;
        public RawRectangleF shape;
        public Color color;
        public TCM_TextObject(TCM_Graphics g, string text, Color color) : base(g)
        {
            this.text = text;
            setFormat("Arial", 18);
            this.color = color;
        }
        public override void setPlacement(float x, float y, float w, float h)
        {
            shape = new RawRectangleF(x, y, x + w, y + h);
        }
        public void setFormat(string fontFamilyName, float fontSize)
        {
            format = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated), fontFamilyName, fontSize);
        }
        public void setFormat(SharpDX.DirectWrite.TextFormat format)
        {
            this.format = format;
        }
        public override void innerDraw()
        {
            graphics.drawTextObject(this);
        }
    }
}

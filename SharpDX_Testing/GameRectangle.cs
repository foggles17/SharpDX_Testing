using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1;

using Color = SharpDX.Mathematics.Interop.RawColor4;
using SDXDW = SharpDX.DirectWrite;

namespace SharpDX_Testing
{
    public abstract class GameRectangle
    {
        public Frame parent;
        public RawRectangleF boundingBox;

        public GameRectangle(float x, float y, float w, float h)
        {
            boundingBox = new RawRectangleF(x, y, x + w, y + h);
            parent = null;
        }

        public void setBoundingBox(float x, float y, float w, float h)
        {
            boundingBox.Left = x;
            boundingBox.Top = y;
            boundingBox.Right = x + w;
            boundingBox.Bottom = y + h;
        }
        public RawMatrix3x2 getFullTransform()
        {
            return parent.getFullTransform();
        }
        /// <summary>
        /// assumes both rectangles have the same transform
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool simpleIsTouching(GameRectangle that)
        {

            if (this.boundingBox.Left > that.boundingBox.Right || this.boundingBox.Top > that.boundingBox.Bottom
                || this.boundingBox.Right < that.boundingBox.Left || this.boundingBox.Bottom < that.boundingBox.Top)
            {
                return false;
            }
            return true;
        }
        public bool isTouching(GameRectangle that)
        {
            RawVector2[] theseCorners = this.getGlobalCorners();
            RawVector2[] thoseCorners = that.getGlobalCorners();

            RawVector2 thisMin = theseCorners[0];
            RawVector2 thatMin = thoseCorners[0];
            RawVector2 thisMax = theseCorners[0];
            RawVector2 thatMax = thoseCorners[0];
            for (int i = 1; i < 4; i++)
            {
                thisMin.X = Math.Min(thisMin.X, theseCorners[i].X);
                thisMin.Y = Math.Min(thisMin.Y, theseCorners[i].Y);
                thisMax.X = Math.Max(thisMax.X, theseCorners[i].X);
                thisMax.Y = Math.Max(thisMax.Y, theseCorners[i].Y);
                thatMin.X = Math.Min(thatMin.X, thoseCorners[i].X);
                thatMin.Y = Math.Min(thatMin.Y, thoseCorners[i].Y);
                thatMax.X = Math.Max(thatMax.X, thoseCorners[i].X);
                thatMax.Y = Math.Max(thatMax.Y, thoseCorners[i].Y);
            }
            if (thisMin.X > thatMax.X || thisMin.Y > thatMax.Y || thisMax.X < thatMin.X || thisMax.Y < thatMin.Y)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (testSeparator(theseCorners[i], theseCorners[(i + 1) % 4], theseCorners[(i + 2) % 4], thoseCorners))
                {
                    return false;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (testSeparator(thoseCorners[i], thoseCorners[(i + 1) % 4], thoseCorners[(i + 2) % 4], theseCorners))
                {
                    return false;
                }
            }
            return true;
        }
        private static bool testSeparator(RawVector2 a1, RawVector2 a2, RawVector2 a3, RawVector2[] b)
        {
            RawVector2 line = new RawVector2(a1.X - a2.X, a1.Y - a2.Y);
            RawVector2 perp = new RawVector2(-line.Y, line.X);
            bool opposite = (a1.X - a3.X) * perp.X + (a1.Y - a3.Y) * perp.Y > 0;
            foreach (RawVector2 point in b)
            {
                if (((a1.X - point.X) * perp.X + (a1.Y - point.Y) * perp.Y > 0) == opposite)
                    return false;
            }
            return true;
        }
        public RawVector2[] getGlobalCorners()
        {
            RawVector2[] output = new RawVector2[4];
            RawMatrix3x2 transform = getFullTransform();
            output[0] = TCM_Matrix3x2.transformPoint(boundingBox.Left, boundingBox.Top, transform);
            output[1] = TCM_Matrix3x2.transformPoint(boundingBox.Right, boundingBox.Top, transform);
            output[2] = TCM_Matrix3x2.transformPoint(boundingBox.Right, boundingBox.Bottom, transform);
            output[3] = TCM_Matrix3x2.transformPoint(boundingBox.Left, boundingBox.Bottom, transform);
            return output;
        }
    }

    public class Hitbox : GameRectangle
    {
        public Hitbox(float x = 0, float y = 0, float w = 1, float h = 1) : base(x, y, w, h) { }
    }

    public abstract class Visual : GameRectangle
    {
        public Visual(float x = 0, float y = 0, float w = 120, float h = 120) : base(x, y, w, h) { }
        public void draw()
        {
            if (isTouching(TCM_Graphics.screenFrame.hitboxes[0]))
                innerDraw();
        }
        public abstract void innerDraw();
    }
    public class BitmapVisual : Visual
    {
        public Bitmap bitmap;
        public float opacity;

        public BitmapVisual(string filename, float x = 0, float y = 0, float w = 1, float h = 1) : base(x,y,w,h)
        {
            setBitmap(filename);
            opacity = 1;
        }
        public BitmapVisual(Bitmap bitmap, float x = 0, float y = 0, float w = 1, float h = 1) : base(x,y,w,h)
        {
            setBitmap(bitmap);
            opacity = 1;
        }
        public void setBitmap(string filename)
        {
            bitmap = TCM_Graphics.loadBitmapFromFile(filename);
        }
        public void setBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }
        public void setTransparency(float transparency)
        {
            this.opacity = transparency;
        }
        public override void innerDraw()
        {
            TCM_Graphics.drawBitmapVisual(this);
        }
    }
    public class TextVisual : Visual
    {
        public string text;
        public SDXDW.TextFormat format;
        public Color color;
        public TextVisual(string text, Color color) : base()
        {
            this.text = text;
            setFormat("Arial", 18);
            this.color = color;
        }
        public void setFormat(string fontFamilyName, float fontSize)
        {
            setFormat(new SDXDW.TextFormat(new SDXDW.Factory(SDXDW.FactoryType.Isolated), fontFamilyName, fontSize));
        }
        public void setFormat(SDXDW.TextFormat format)
        {
            this.format = format;
        }
        public void setColor(float r, float g, float b, float a)
        {
            setColor(new Color(r, g, b, a));
        }
        public void setColor(Color color)
        {
            this.color = color;
        }
        public override void innerDraw()
        {
            TCM_Graphics.drawTextVisual(this);
        }
    }

    /// <summary>
    /// This one goes character by character
    /// need to figure out how to reset it
    /// </summary>
    //public class ProgressingTextVisual : Visual
    //{
    //    TextVisual tv;
    //    string text;
    //    bool drawing;
    //    int currentLength;
    //    public ProgressingTextVisual(string text, Color color) : base()
    //    {
    //        this.text = text;
    //        tv = new TextVisual("", color);
    //        setFormat("Arial", 18);
    //        drawing = false;
    //        currentLength = 0;
    //    }
    //    public void setFormat(string fontFamilyName, float fontSize)
    //    {
    //        setFormat(new SDXDW.TextFormat(new SDXDW.Factory(SDXDW.FactoryType.Isolated), fontFamilyName, fontSize));
    //    }
    //    public void setFormat(SDXDW.TextFormat format)
    //    {
    //        tv.format = format;
    //    }
    //    public void setColor(float r, float g, float b, float a)
    //    {
    //        setColor(new Color(r, g, b, a));
    //    }
    //    public void setColor(Color color)
    //    {
    //        tv.color = color;
    //    }
    //    public override void innerDraw()
    //    {
    //        tv.boundingBox = boundingBox;
    //        if(!drawing)
    //        {
    //            drawing = true;
    //            currentLength = 0;
    //        }
    //        if(currentLength < text.Length)
    //        {
    //            currentLength++;
    //            tv.text = text.Substring(0, currentLength);
    //        }
    //        TCM_Graphics.drawTextVisual(tv);
    //    }
    //    public void reset()
    //    {
    //        drawing = false;
    //    }
    //}
}

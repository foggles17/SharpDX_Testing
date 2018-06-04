using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.XInput;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.Mathematics.Interop;
using SharpDX.Mathematics;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Color = SharpDX.Mathematics.Interop.RawColor4;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace SharpDX_Testing
{
    public static class TCM_Graphics
    {
        //dispose
        static Device dev;
        static SwapChain sc;
        static RenderTargetView rtv;
        static Texture2D backBuffer;
        static Factory fact;

        //useful
        public static RenderForm form;
        public static RenderTarget rt;
        static SharpDX.Direct2D1.Factory d2dFactory;
        static SolidColorBrush brush;
        static Color bgColor;

        public static Frame screenFrame;

        public static Stack<RawMatrix3x2> transformStack;

        public static void Initialize(string name, int width, int height, Color bgColor)
        {
            form = new RenderForm(name);
            form.ClientSize = new System.Drawing.Size(width, height);
            screenFrame = new Frame();
            screenFrame.setTransform(1, 0, 0, 1, 0, 0);
            screenFrame.addHitbox(new Hitbox(0, 0, width, height));
            TCM_Graphics.bgColor = bgColor;

            //overall begin drawing
            transformStack = new Stack<RawMatrix3x2>();
            var swapDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 },
                swapDescription, out dev, out sc);

            d2dFactory = new SharpDX.Direct2D1.Factory();

            fact = sc.GetParent<Factory>();
            fact.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(sc, 0);
            rtv = new RenderTargetView(dev, backBuffer);
            Surface surf = backBuffer.QueryInterface<Surface>();


            rt = new RenderTarget(d2dFactory, surf,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));

            brush = new SolidColorBrush(rt, bgColor);
        }
        //overall end drawing
        public static void dispose()
        {
            rtv.Dispose();
            backBuffer.Dispose();
            dev.ImmediateContext.ClearState();
            dev.ImmediateContext.Flush();
            dev.Dispose();
            sc.Dispose();
            fact.Dispose();
        }
        //frame by frame begin drawing
        public static void beginDraw()
        {
            rt.BeginDraw();
            rt.Clear(bgColor);
        }
        //frame by frame end drawing
        public static void endDraw()
        {
            rt.EndDraw();
            sc.Present(0, PresentFlags.None);
        }

        //transform manipulation
        public static void resetTransform()
        {
            setTransform(1, 0, 0, 1, 0, 0);
            transformStack.Clear();
        }
        public static void setTransform(float a1, float b1, float a2, float b2, float a3, float b3)
        {
            rt.Transform = new RawMatrix3x2(a1, b1, a2, b2, a3, b3);
        }
        public static void setTransform(RawMatrix3x2 m)
        {
            rt.Transform = m;
        }
        public static void applyTransform(RawMatrix3x2 m)
        {
            transformStack.Push(rt.Transform);
            setTransform(TCM_Matrix3x2.multiply(m, rt.Transform));
        }
        public static void revertTransform()
        {
            setTransform(transformStack.Pop());
        }



        //drawing
        public static void drawTextVisual(TextVisual tv)
        {
            brush.Color = tv.color;
            rt.DrawText(tv.text, tv.format, tv.boundingBox, brush);
        }
        public static void drawBitmapVisual(BitmapVisual bv)
        {
            rt.DrawBitmap(bv.bitmap, bv.boundingBox, bv.opacity, BitmapInterpolationMode.NearestNeighbor);
        }

        //public void drawDrawingObject(TCM_DrawingObject o)
        //{
        //    brush.Color = o.fillColor;
        //    rt.FillGeometry(o.shape, brush);
        //    brush.Color = o.outlineColor;
        //    rt.DrawGeometry(o.shape, brush, o.lineWidth);
        //}
        //public Geometry createGeometry(TCM_DrawingObject.Shape s, float x, float y, float w, float h)
        //{
        //    switch(s)
        //    {
        //        case TCM_DrawingObject.Shape.ellipse:
        //            {
        //                return new EllipseGeometry(d2dFactory, new Ellipse(new RawVector2(x + w / 2, y + h / 2), w, h));
        //            }
        //        case TCM_DrawingObject.Shape.rectangle:
        //            {
        //                return new RectangleGeometry(d2dFactory, new RawRectangleF(x, y, x + w, y + h));
        //            }
        //        default:
        //            {
        //                return null;
        //            }
        //    }
        //}

        /// <summary>
        /// Gets a png from the images folder.
        /// Image List:
        /// (1) "baseBird" - 16x16 bird, perfect.
        /// (2) "bird" - 120x120 bird, white square, imperfect.
        /// (3) "transparencyBird" - 120x120 bird, imperfect.
        /// (4) "burg" - 4330x3247 burger.
        /// (5) "blank" - one color; size is irrelevant.
        /// (6) "eeeeeee" - 277x279 scary face
        /// </summary>
        /// <param name="file">The name of the file, not including ".png" or its location.</param>
        /// <returns></returns>
        public static Bitmap loadPNG(string file)
        {
            /*
            Image List:
            "baseBird" - 16x16 bird, perfect
            "bird" - 120x120 bird, white square, imperfect
            "transparencyBird" - 120x120 bird, imperfect
            "burg" - 4330x3247 burger
            */

            return loadBitmapFromFile("../../images/" + file + ".png");
        }
        public static Bitmap loadBitmapFromFile(string filename)
        {
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filename))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bp = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                var size = new Size2(bitmap.Width, bitmap.Height);

                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                {
                    var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;
                    return new Bitmap(rt, size, tempStream, stride, bp);
                }
            }
        }
    }
}

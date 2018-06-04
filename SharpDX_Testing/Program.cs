using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Threading;

using SharpDX;
//controller
using SharpDX.XInput;
//graphics
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
//audio
using SharpDX.Multimedia;
using SharpDX.XAudio2;

using Color = SharpDX.Mathematics.Interop.RawColor4;


namespace SharpDX_Testing
{
    public enum StickState { neutral, right, upRight, up, upLeft, left, downLeft, down, downRight }
    public static class Program
    {
        static Frame pause;
        static TCM_Controller control;
        static long frameCount;
        public static Room currentRoom;
        static bool paused;
        static List<Room> rooms;
        //Frame camera;
        static Color transparentBlack = new Color(0, 0, 0, 0);
        //public Program()
        //{
        //    frameCount = 0;
        //    TCM_Graphics.Initialize("Game", 1920, 1080, new Color(0, 0, 0, 1));
        //    TCM_Audio.Initialize();
        //    rooms = new List<Room>();
        //    rooms.Add(new Room(3, 2, true));
        //    currentRoom = rooms[0];
        //    currentRoom.initialize();
        //    //camera.addChild(currentRoom.objects, false);
        //    if (new Controller() != null)
        //    {
        //        control = new TCM_Controller(new Controller(UserIndex.One));
        //        control.LSDeadZone = .18;
        //        control.RSDeadZone = .18;
        //        control.axialLeft = false;
        //        control.axialRight = false;
        //    }
        //}

        public static float getAnalog(AnalogEnum a)
        {
            return (float)control.analogs[(int)a];
        }
        public static StickState leftStickState()
        {
            return getStickState(true);
        }
        public static StickState rightStickState()
        {
            return getStickState(false);
        }
        public static StickState getStickState(bool left)
        {
            float x;
            float y;
            if (left)
            {
                x = getAnalog(AnalogEnum.lx);
                y = getAnalog(AnalogEnum.ly);
            }
            else
            {
                x = getAnalog(AnalogEnum.rx);
                y = getAnalog(AnalogEnum.ry);
            }
            float tan = (float)Math.Tan(3 * Math.PI / 8);
            if (Math.Abs(x * x) + Math.Abs(y * y) < .015)
            {
                return StickState.neutral;
            }
            else if (x == 0)
            {
                if (y > 0)
                    return StickState.down;
                else
                    return StickState.up;
            }
            else if (y == 0)
            {
                if (x > 0)
                    return StickState.right;
                else
                    return StickState.left;
            }
            else if (Math.Abs(y / x) > tan)
            {
                if (y > 0)
                    return StickState.down;
                else
                    return StickState.up;
            }
            else if (Math.Abs(x / y) > tan)
            {
                if (x > 0)
                    return StickState.right;
                else
                    return StickState.left;
            }
            else if (x > 0)
            {
                if (y > 0)
                    return StickState.downRight;
                else
                    return StickState.upRight;
            }
            else
            {
                if (y > 0)
                    return StickState.downLeft;
                else
                    return StickState.upLeft;
            }
        }
        public static StickState dpadStickState()
        {
            ButtonState u = getButton(ButtonEnum.du);
            ButtonState d = getButton(ButtonEnum.dd);
            ButtonState l = getButton(ButtonEnum.dl);
            ButtonState r = getButton(ButtonEnum.dr);
            bool up;
            bool down;
            bool left;
            bool right;
            if (u == ButtonState.released || u == ButtonState.up)
                up = false;
            else
                up = true;
            if (d == ButtonState.released || d == ButtonState.up)
                down = false;
            else
                down = true;
            if (l == ButtonState.released || l == ButtonState.up)
                left = false;
            else
                left = true;
            if (r == ButtonState.released || r == ButtonState.up)
                right = false;
            else
                right = true;
            if (up)
            {
                if (left)
                    return StickState.upLeft;
                else if (right)
                    return StickState.upRight;
                else
                    return StickState.up;
            }
            else if (down)
            {
                if (left)
                    return StickState.downLeft;
                else if (right)
                    return StickState.downRight;
                else
                    return StickState.down;
            }
            else if (left)
                return StickState.left;
            else if (right)
                return StickState.right;
            else
                return StickState.neutral;
        }
        public static ButtonState getButton(ButtonEnum b)
        {
            return control.buttons[(int)b];
        }

        public static void step()
        {
            TCM_Audio.cleanOutSources();
            TCM_Graphics.beginDraw();
            control.storeInputs();

            bool playing = true;

            if (playing)
            {
                if (getButton(ButtonEnum.rb) == ButtonState.down || getButton(ButtonEnum.rb) == ButtonState.pressed)
                {
                    control.vib.RightMotorSpeed = (ushort)(getAnalog(AnalogEnum.rt)*65535);
                    control.setVibration(control.vib);
                }
                if (getButton(ButtonEnum.lb) == ButtonState.down || getButton(ButtonEnum.lb) == ButtonState.pressed)
                {
                    control.vib.LeftMotorSpeed = (ushort)(getAnalog(AnalogEnum.lt) * 65535);
                    control.setVibration(control.vib);
                }
                if (getButton(ButtonEnum.start) == ButtonState.pressed)
                {
                    paused = !paused;
                    /*if (paused)
                        TCM_Audio.pauseAllAudio();
                    else
                        TCM_Audio.resumeAllAudio();*/
                }
                if (!paused)
                {
                    //put game code here
                    currentRoom.step();
                    frameCount++;
                }
                else
                {
                    //put menu code here
                    currentRoom.draw();
                    pause.draw();
                    if (getButton(ButtonEnum.back) == ButtonState.pressed)
                    {
                        TCM_Graphics.form.Close();
                    }
                }
            }

            control.storeGamepad();
            TCM_Graphics.endDraw();
            Thread.Sleep(20);
        }
        private static void startUp()
        {
            frameCount = 0;
            TCM_Graphics.Initialize("Game", 1920, 1080, new Color(0, 0, 0, 1));
            TCM_Audio.Initialize();
            rooms = new List<Room>();
            rooms.Add(new Room(2, 1, true));
            currentRoom = rooms[0];
            currentRoom.initialize();

            pause = new Frame();



            //working on it, does scrolly text, needs to be able to reset
            //ProgressingTextVisual pauseText = new ProgressingTextVisual("PAUSED - back to quit\ntesting how much text i can fit in this box"
            //    + "\napparently that wasn't enough, so i'll just keep adding until something weird starts happening", new Color(1, 1, 1, 1));
            //pauseText.setBoundingBox(490, 280, 940, 520);
            //pauseText.setFormat("Consolas", 32);

            //List<ProgressingTextVisual> scrolly = new List<ProgressingTextVisual>();
            //scrolly.Add(pauseText);

            TextVisual pauseText = new TextVisual("PAUSED - back to quit\ntesting how much text i can fit in this box"
                + "\napparently that wasn't enough, so i'll just keep adding until something weird starts happening", new Color(1, 1, 1, 1));
            pauseText.setBoundingBox(490, 280, 940, 520);
            pauseText.setFormat(pauseText.format.FontFamilyName, 32);
            pauseText.setFormat("Consolas", 32);

            BitmapVisual pauseBitmap = new BitmapVisual(TCM_Graphics.loadPNG("blank"), 480, 270, 960, 540);
            pauseBitmap.setTransparency(.5f);
            pause.addVisual(pauseBitmap);
            pause.addVisual(pauseText);
            if (new Controller() != null)
            {
                control = new TCM_Controller(new Controller(UserIndex.One));
                control.LSDeadZone = .18;
                control.RSDeadZone = .18;
                control.axialLeft = false;
                control.axialRight = false;
            }
            else
            {
                
            }
        }
        public static void runLoop()
        {
            startUp();
            RenderLoop.Run(TCM_Graphics.form, step);
            TCM_Audio.clear();
            TCM_Graphics.dispose();
            TCM_Audio.disposeAudio();
        }
    }
}

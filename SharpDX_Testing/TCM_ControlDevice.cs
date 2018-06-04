using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using SharpDX;

namespace SharpDX_Testing
{
    public enum ButtonEnum { start, back, a, b, x, y, lb, rb, ls, rs, dl, dr, du, dd }
    public enum AnalogEnum { lt, rt, lx, ly, rx, ry }
    public enum ButtonState { pressed, down, released, up }

    public abstract class TCM_ControlDevice
    {

    }
    public class TCM_Controller : TCM_ControlDevice
    {
        public ButtonState[] buttons;
        public double[] analogs;
        Controller myController;
        Enum[] buttonValues;
        public Gamepad prev;
        public double LSDeadZone;
        public double RSDeadZone;
        public bool axialLeft;
        public bool axialRight;

        public Vibration vib;

        //todo: work right when no controller is connected
        public TCM_Controller()
        {

        }
        public TCM_Controller(Controller innards)
        {
            buttons = new ButtonState[(int)ButtonEnum.dd + 1];
            analogs = new double[(int)AnalogEnum.ry + 1];
            myController = innards;
            buttonValues = new Enum[]
            {
                GamepadButtonFlags.Start,
                GamepadButtonFlags.Back,
                GamepadButtonFlags.A,
                GamepadButtonFlags.B,
                GamepadButtonFlags.X,
                GamepadButtonFlags.Y,
                GamepadButtonFlags.LeftShoulder,
                GamepadButtonFlags.RightShoulder,
                GamepadButtonFlags.LeftThumb,
                GamepadButtonFlags.RightThumb,
                GamepadButtonFlags.DPadLeft,
                GamepadButtonFlags.DPadRight,
                GamepadButtonFlags.DPadUp,
                GamepadButtonFlags.DPadDown
            };
            LSDeadZone = 0;
            RSDeadZone = 0;
            axialLeft = true;
            axialRight = true;
            vib = new Vibration();
        }
        public void assignController(Controller innards)
        {
            myController = innards;
        }
        /// <summary>
        /// Makes circular deadzones
        /// </summary>
        /// <param name="x">The x input from the stick</param>
        /// <param name="y">The y input from the stick</param>
        /// <param name="left">Is it the left or right stick</param>
        /// <returns>The processed vector containing a useable x-input and y-input.</returns>
        private Vector2 applyRadialDeadzone(short x, short y, bool left)
        {
            var dz = (left ? LSDeadZone : RSDeadZone);
            Vector2 vect = new Vector2((float)x, (float)-y);
            float length = vect.Length()/30000;
            if (length > 1)
                length = 1;
            if (length < dz)
                vect = Vector2.Zero;
            else
            {
                vect.Normalize();
                vect.Y = vect.Y * (float)((length - dz) / (1 - dz));
                vect.X = vect.X * (float)((length - dz) / (1 - dz));
            }
            return vect;
        }
        /// <summary>
        /// Makes "+" shaped deadzones
        /// </summary>
        /// <param name="x">The x input from the stick</param>
        /// <param name="y">The y input from the stick</param>
        /// <param name="left">Is it the left or right stick</param>
        /// <returns>The processed vector containing a useable x-input and y-input.</returns>
        private Vector2 applyAxialDeadzone(short x, short y, bool left)
        {
            var dz = (left ? LSDeadZone : RSDeadZone);
            Vector2 vect = Vector2.Zero;
            double rawX = (x + .5) / (short.MaxValue + .5);
            if (rawX > dz)
                vect.X = (float)((rawX - dz) / (1 - dz));
            else if (rawX < -dz)
                vect.X = (float)((rawX + dz) / (1 - dz));
            double rawY = (-y + .5) / (short.MaxValue + .5);
            if (rawY > dz)
                vect.Y = (float)((rawY - dz) / (1 - dz));
            else if (rawY < -dz)
                vect.Y = (float)((rawY + dz) / (1 - dz));
            return vect;
        }
        public void setVibration(Vibration v)
        {
            vib = v;
            myController.SetVibration(v);
        }
        public void storeInputs()
        {
            analogs[(int)AnalogEnum.lx] = getAnalogValue(AnalogEnum.lx);
            analogs[(int)AnalogEnum.ly] = getAnalogValue(AnalogEnum.ly);

            analogs[(int)AnalogEnum.rx] = getAnalogValue(AnalogEnum.rx);
            analogs[(int)AnalogEnum.ry] = getAnalogValue(AnalogEnum.ry);

            analogs[(int)AnalogEnum.lt] = getAnalogValue(AnalogEnum.lt);
            analogs[(int)AnalogEnum.rt] = getAnalogValue(AnalogEnum.rt);

            for(int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = getState((ButtonEnum)i);
            }
        }
        public ButtonState getButton(ButtonEnum butt)
        {
            return buttons[(int)butt];
        }
        private ButtonState getState(ButtonEnum butt)
        {
            switch (buttonChange(butt))
            {
                case (-1):
                    {
                        return ButtonState.released;
                    }
                case (1):
                    {
                        return ButtonState.pressed;
                    }
                case (0):
                    {
                        if (buttonDown(butt))
                            return ButtonState.down;
                        else
                            return ButtonState.up;
                    }
                default:
                    {
                        throw new IndexOutOfRangeException();
                    }
            }
        }
        public double getAnalogValue(AnalogEnum analog, Gamepad gamepad)
        {
            switch (analog)
            {
                case AnalogEnum.lt:
                    {
                        return (double)gamepad.LeftTrigger / byte.MaxValue;
                    }
                case AnalogEnum.rt:
                    {
                        return (double)gamepad.RightTrigger / byte.MaxValue;
                    }
                case AnalogEnum.lx:
                    {
                        return axialLeft ? applyAxialDeadzone(gamepad.LeftThumbX, gamepad.LeftThumbY, true).X 
                            : applyRadialDeadzone(gamepad.LeftThumbX, gamepad.LeftThumbY, true).X;
                    }
                case AnalogEnum.ly:
                    {
                        return axialLeft ? applyAxialDeadzone(gamepad.LeftThumbX, gamepad.LeftThumbY, true).Y
                            : applyRadialDeadzone(gamepad.LeftThumbX, gamepad.LeftThumbY, true).Y;
                    }
                case AnalogEnum.rx:
                    {
                        return axialRight ? applyAxialDeadzone(gamepad.RightThumbX, gamepad.RightThumbY, false).X
                            : applyRadialDeadzone(gamepad.RightThumbX, gamepad.RightThumbY, false).X;
                    }
                case AnalogEnum.ry:
                    {
                        return axialRight ? applyAxialDeadzone(gamepad.RightThumbX, gamepad.RightThumbY, false).Y
                            : applyRadialDeadzone(gamepad.RightThumbX, gamepad.RightThumbY, false).Y;
                    }
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        public double getAnalogValue(AnalogEnum analog)
        {
            return getAnalogValue(analog, getGamepad());
        }
        public Gamepad getGamepad()
        {
            return myController.GetState().Gamepad;
        }
        public void storeGamepad()
        {
            prev = getGamepad();
        }
        public bool buttonDown(ButtonEnum button, Gamepad gamepad)
        {
            return gamepad.Buttons.HasFlag(buttonValues[(int)button]);
        }
        public bool buttonDown(ButtonEnum button)
        {
            return buttonDown(button, getGamepad());
        }
        private bool buttonHasChanged(ButtonEnum button)
        {
            return buttonDown(button) != buttonDown(button, prev);
        }
        public int buttonChange(ButtonEnum button)
        {
            if (!buttonHasChanged(button))
                return 0;
            else
                return buttonDown(button) ? 1 : -1;
        }
        public bool buttonPress(ButtonEnum button)
        {
            if (buttonChange(button) == 1)
                return true;

            return false;
        }
        public bool buttonRelease(ButtonEnum button)
        {
            if (buttonChange(button) == -1)
                return true;

            return false;
        }
    }

    public class TCM_Keyboard : TCM_ControlDevice
    {

    }
}

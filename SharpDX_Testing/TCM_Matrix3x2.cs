using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Mathematics.Interop;

namespace SharpDX_Testing
{
    public static class TCM_Matrix3x2
    {
        /// <summary>
        /// Multiplies two RawMatrix3x2 objects as if they were 3x3 matrices, with the final column being (0,0,1).
        /// Can apply a transform 'a' onto a transform 'b'.
        /// </summary>
        /// <param name="a">The first matrix</param>
        /// <param name="b">The second matrix</param>
        /// <returns></returns>
        public static RawMatrix3x2 multiply(RawMatrix3x2 a, RawMatrix3x2 b)
        {
            RawMatrix3x2 output = new RawMatrix3x2();
            output.M11 = a.M11 * b.M11 + a.M12 * b.M21;
            output.M12 = a.M11 * b.M12 + a.M12 * b.M22;
            output.M21 = a.M21 * b.M11 + a.M22 * b.M21;
            output.M22 = a.M21 * b.M12 + a.M22 * b.M22;
            output.M31 = a.M31 * b.M11 + a.M32 * b.M21 + b.M31;
            output.M32 = a.M31 * b.M12 + a.M32 * b.M22 + b.M32;
            return output;
        }
        public static RawMatrix3x2 translate(float dx, float dy)
        {
            return new RawMatrix3x2(1, 0, 0, 1, dx, dy);
        }
        public static RawMatrix3x2 scale(float sx, float sy)
        {
            return new RawMatrix3x2(sx, 0, 0, sy, 0, 0);
        }
        public static RawMatrix3x2 rotate(float theta)
        {
            return new RawMatrix3x2((float)Math.Cos(theta), -(float)Math.Sin(theta), (float)Math.Sin(theta), (float)Math.Cos(theta), 0,0);
        }
        public static RawVector2 transformPoint(float x, float y, RawMatrix3x2 transform)
        {
            RawVector2 output = new RawVector2();

            output.X = x * transform.M11 + y * transform.M21 + transform.M31;
            output.Y = x * transform.M12 + y * transform.M22 + transform.M32;

            return output;
        }
    }
}

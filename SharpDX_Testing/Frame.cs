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
    public class Frame
    {
        public Frame parent;
        public List<Frame> frontChildren;
        public List<Frame> backChildren;
        public List<Visual> visuals;
        public List<Hitbox> hitboxes;
        public RawMatrix3x2 transform;

        public Frame()
        {
            transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
            frontChildren = new List<Frame>();
            backChildren = new List<Frame>();
            visuals = new List<Visual>();
            hitboxes = new List<Hitbox>();
            parent = null;
        }
        public void step()
        {
            stepStart();
            stepFinish();
        }
        public virtual void stepStart()
        {
            foreach (Frame f in backChildren)
                f.stepStart();
            foreach (Frame f in frontChildren)
                f.stepStart();
        }
        public virtual void stepFinish()
        {
            foreach (Frame f in backChildren)
                f.stepFinish();
            foreach (Frame f in frontChildren)
                f.stepFinish();
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
        public RawMatrix3x2 getFullTransform()
        {
            if (parent == null)
            {
                return transform;
            }
            else
            {
                return TCM_Matrix3x2.multiply(parent.getFullTransform(), transform);
            }
        }

        /// <summary>
        /// Attaches a frame to this, with render order based on 'back'.
        /// NOTE: Actors and Rooms are Frames!
        /// </summary>
        /// <param name="o">Frame to add</param>
        /// <param name="back">True if behind parent</param>
        public void addChild(Frame o, bool back)
        {
            o.parent = this;
            if (back)
                backChildren.Add(o);
            else
                frontChildren.Add(o);
        }
        public void innerDraw()
        {
            for(int i = 0; i < visuals.Count; i++)
            {
                visuals[i].draw();
            }
        }
        public void clear()
        {
            frontChildren.Clear();
            backChildren.Clear();
            visuals.Clear();
            hitboxes.Clear();
        }
        public void draw()
        {
            //apply transform
            TCM_Graphics.applyTransform(transform);

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
            TCM_Graphics.revertTransform();
        }
        public void addVisual(Visual v)
        {
            v.parent = this;
            visuals.Add(v);
        }
        public void addHitbox(Hitbox h)
        {
            h.parent = this;
            hitboxes.Add(h);
        }
    }
}

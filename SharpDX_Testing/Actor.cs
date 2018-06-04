using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace SharpDX_Testing
{
    public enum Anchor { center, centerRight, topRight, topCenter, topLeft, centerLeft, bottomLeft, bottomCenter, bottomRight }
    public abstract class Actor : Frame
    {
        public RawVector2 movement;
        RawVector2 oldLocation;
        public Hitbox mainBox;
        public int id;
        public static int nextId = 0;
        public Actor()
        {
            mainBox = new Hitbox();
            addHitbox(mainBox);
            id = nextId;
            nextId++;
            movement = new RawVector2(0, 0);
        }
        public void move(float dx, float dy)
        {
            transform.M31 += dx;
            transform.M32 += dy;
            //applyTransform(TCM_Matrix3x2.translate(dx, dy));
        }

        public float left()
        {
            return transform.M31 + mainBox.boundingBox.Left;
        }
        public float right()
        {
            return transform.M31 + mainBox.boundingBox.Right;
        }
        public float top()
        {
            return transform.M32 + mainBox.boundingBox.Top;
        }
        public float bottom()
        {
            return transform.M32 + mainBox.boundingBox.Bottom;
        }
        /// <summary>
        /// Sets the width, height, x, and y of the mainBox.
        /// </summary>
        /// <param name="w">Width of the mainBox.</param>
        /// <param name="h">Height of the mainBox.</param>
        /// <param name="a">The point on the mainBox that the origin will be at.</param>
        public void setMainBox(float w, float h, Anchor a)
        {
            float x = 0;
            float y = 0;
            switch(a)
            {
                case (Anchor.topLeft):
                    break;
                case (Anchor.topRight):
                    x = -w;
                    break;
                case (Anchor.topCenter):
                    x = -w / 2;
                    break;
                case (Anchor.centerLeft):
                    y = -h / 2;
                    break;
                case (Anchor.bottomLeft):
                    y = -h;
                    break;
                case (Anchor.centerRight):
                    x = -w;
                    y = -h / 2;
                    break;
                case (Anchor.bottomCenter):
                    x = -w / 2;
                    y = -h;
                    break;
                case (Anchor.bottomRight):
                    x = -w;
                    y = -h;
                    break;
                case (Anchor.center):
                    x = -w / 2;
                    y = -h / 2;
                    break;
            }
            setMainBox(x, y, w, h);
        }
        public void setMainBox(float x, float y, float w, float h)
        {
            mainBox.setBoundingBox(x, y, w, h);
        }
        public override void stepStart()
        {
            oldLocation.X = transform.M31;
            oldLocation.Y = transform.M32;
            base.stepStart();
        }
        public override void stepFinish()
        {
            movement = new RawVector2(transform.M31 - oldLocation.X, transform.M32 - oldLocation.Y);
            base.stepFinish();
        }
    }
    public abstract class MovingActor : Actor
    {
        //public RawVector2 frameMovement;
        public float ySpeed;
        public bool grounded;
        public float gravity;
        public bool facingLeft;
        public float maxFallSpeed;
        public float maxSpeed;

        public void safeMove(float dx, float dy)
        {
            float l = distanceToLeftWall();
            float r = distanceToRightWall();
            if(dx > r)
            {
                dx = r;
            }
            else if(dx < -l)
            {
                dx = -l;
            }
            move(dx, 0);

            float u = distanceToCeiling();
            float d = distanceToFloor();
            if (dy > d)
            {
                dy = d;
                grounded = true;
                ySpeed = 0;
            }
            else if (dy < -u)
            {
                dy = -u;
                ySpeed = 0;
            }
            move(0, dy);
        }
        /// <summary>
        /// ---you---
        /// </summary>
        /// <returns>A list of platforms that have some y values in common with this actor.</returns>
        public List<Platform> verticallyRelevantPlatforms()
        {
            List<Platform> output = new List<Platform>();
            foreach (Platform p in Program.currentRoom.platforms)
            {
                if (top() < p.bottom() && bottom() > p.top())
                {
                    output.Add(p);
                }
            }
            return output;
        }
        public float distanceToLeftWall()
        {
            List<Platform> input = verticallyRelevantPlatforms();
            float output = float.MaxValue;
            foreach(Platform p in input)
            {
                if (p.isALeftWall())
                {
                    float value = left() - p.right();
                    if (value >= 0 && value < output)
                    {
                        output = value;
                    }
                }
            }
            return output;
        }
        public float distanceToRightWall()
        {
            List<Platform> input = verticallyRelevantPlatforms();
            float output = float.MaxValue;
            foreach (Platform p in input)
            {
                if (p.isARightWall())
                {
                    float value = p.left() - right();
                    if (value >= 0 && value < output)
                    {
                        output = value;
                    }
                }
            }
            return output;
        }
        /// <summary>
        ///  |
        /// you
        ///  |
        /// </summary>
        /// <returns>A list of platforms that have some x values in common with this actor.</returns>
        public List<Platform> horizontallyRelevantPlatforms()
        {
            List<Platform> output = new List<Platform>();
            foreach (Platform p in Program.currentRoom.platforms)
            {
                if (left() < p.right() && right() > p.left())
                {
                    output.Add(p);
                }
            }
            return output;
        }
        public float distanceToCeiling()
        {
            List<Platform> input = horizontallyRelevantPlatforms();
            float output = float.MaxValue;
            foreach (Platform p in input)
            {
                if (p.isACeiling())
                {
                    float value = top() - p.bottom();
                    if (value >= 0 && value < output)
                    {
                        output = value;
                    }
                }
            }
            return output;
        }
        public float distanceToFloor()
        {
            List<Platform> input = horizontallyRelevantPlatforms();
            float output = float.MaxValue;
            foreach (Platform p in input)
            {
                if (p.isAFloor())
                {
                    float value = p.top() - bottom();
                    if (value >= 0 && value < output)
                    {
                        output = value;
                    }
                }
            }
            return output;
        }
    }
    public class Enemy : MovingActor
    {
        public Enemy()
        {
            facingLeft = (id%2 == 0);
            gravity = 2;
            maxFallSpeed = 60;
            maxSpeed = 5;
            int height = 120;
            int width = 80;
            setMainBox(width, height, Anchor.center);
            addVisual(new BitmapVisual(TCM_Graphics.loadPNG("robot"), -width/2, -height/2, width, height));
            setTransform(1, 0, 0, 1, 600, 400);
        }
        public override void stepStart()
        {
            float xSpeed = maxSpeed;
            if (facingLeft)
                xSpeed = -xSpeed;
            ySpeed += gravity;

            ySpeed = Math.Min(ySpeed, maxFallSpeed);
            float oldX = transform.M31;
            safeMove(xSpeed, ySpeed);
            if(Math.Abs(transform.M31 - oldX) < maxSpeed)
            {
                facingLeft = !facingLeft;
            }

            if (distanceToFloor() > 0)
            {
                grounded = false;
            }
            base.stepStart();
        }
    }
    public enum PlayerAction { idle, attacking, shooting }
    public class Player : MovingActor
    {
        PlayerAction currentAction;
        float jumpStrength;
        int actionFrame;
        int jumpFrame;
        RawVector2 idealLocation;
        public Player() : base()
        {
            currentAction = PlayerAction.idle;
            gravity = 2;
            maxSpeed = 20;
            jumpStrength = 30;
            maxFallSpeed = 60;
            actionFrame = 0;
            jumpFrame = 0;
            grounded = false;
            int height = 200;
            int width = 100;
            idealLocation = new RawVector2(TCM_Graphics.form.ClientSize.Width / 2, TCM_Graphics.form.ClientSize.Height / 2);
            setMainBox(100, 200, Anchor.center);
            addVisual(new BitmapVisual(TCM_Graphics.loadPNG("bird"), -width/2, -height/2, width, height));
            setTransform(1, 0, 0, 1, 1920 / 2, 1080 / 2);
        }
        public override void stepStart()
        {
            float xSpeed = 0;
            if (currentAction == PlayerAction.idle || !grounded)
            {
                float lx = Program.getAnalog(AnalogEnum.lx);
                float ly = Program.getAnalog(AnalogEnum.ly);
                float speed = maxSpeed;
                //if(Math.Sqrt(lx * lx + ly * ly) < .5)
                //{
                //    speed /= 2;
                //}
                switch (Program.leftStickState())
                {
                    case (StickState.right):
                        xSpeed = speed;
                        break;
                    case (StickState.upRight):
                        xSpeed = speed;
                        break;
                    case (StickState.downRight):
                        xSpeed = speed / 2;
                        break;
                    case (StickState.left):
                        xSpeed = -speed;
                        break;
                    case (StickState.upLeft):
                        xSpeed = -speed;
                        break;
                    case (StickState.downLeft):
                        xSpeed = -speed / 2;
                        break;
                    case (StickState.up):
                        break;
                    case (StickState.down):
                        break;
                    default:
                        switch (Program.dpadStickState())
                        {
                            case (StickState.right):
                                xSpeed = speed;
                                break;
                            case (StickState.upRight):
                                xSpeed = speed;
                                break;
                            case (StickState.downRight):
                                xSpeed = speed / 2;
                                break;
                            case (StickState.left):
                                xSpeed = -speed;
                                break;
                            case (StickState.upLeft):
                                xSpeed = -speed;
                                break;
                            case (StickState.downLeft):
                                xSpeed = -speed / 2;
                                break;
                            case (StickState.up):
                                break;
                            case (StickState.down):
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
            if(currentAction == PlayerAction.attacking)
            {
                if(actionFrame >= 0)
                {
                    currentAction = PlayerAction.idle;
                }
            }
            else if (currentAction == PlayerAction.idle)
            {
                if (Program.getButton(ButtonEnum.a) == ButtonState.pressed)
                {
                    jump();
                }
                if (Program.getButton(ButtonEnum.x) == ButtonState.pressed)
                {
                    attack();
                }
            }
            if (jumpFrame > 0 )
            {
                if (Program.getButton(ButtonEnum.a) != ButtonState.down && Program.getButton(ButtonEnum.a) != ButtonState.pressed)
                {
                    jumpFrame = 0;
                }
            }
            safeMove(xSpeed, ySpeed);

            if(jumpFrame <= 0)
                ySpeed += gravity;

            ySpeed = Math.Min(ySpeed, maxFallSpeed);
            if(distanceToFloor() > 0)
            {
                grounded = false;
            }
            else
            {
                jumpFrame = 0;
            }
            RawMatrix3x2 fullTransform = getFullTransform();
            RawVector2 location = new RawVector2(fullTransform.M31, fullTransform.M32);
            if(location.X != idealLocation.X || location.Y != idealLocation.Y)
            {
                Program.currentRoom.move(location.X - idealLocation.X, location.Y - idealLocation.Y);
            }
            base.stepStart();
        }
        public override void stepFinish()
        {
            actionFrame++;
            if (jumpFrame > 0)
                jumpFrame--;
            base.stepFinish();
        }
        public void jump()
        {
            if (grounded)
            {
                ySpeed = -jumpStrength;
                jumpFrame = 10;
                TCM_Audio.playWAV("grunt");
            }
        }
        public void attack()
        {
            actionFrame = 0;
            //currentAction = PlayerAction.attacking;
            Random r = new Random();
            int h = r.Next(1, 5);

            switch(h)
            {
                case (1):
                    TCM_Audio.playWAV("pewDown2");
                    break;
                case (2):
                    TCM_Audio.playWAV("pewDown1");
                    break;
                case (3):
                    TCM_Audio.playWAV("pew");
                    break;
                case (4):
                    TCM_Audio.playWAV("pewUp1");
                    break;
                case (5):
                    TCM_Audio.playWAV("pewUp2");
                    break;
            }
        }
    }
    public class Platform : Actor
    {
        bool leftWall;
        bool rightWall;
        bool floor;
        bool ceiling;
        public Platform(bool leftWall = true, bool rightWall = true, bool floor = true, bool ceiling = true) : base()
        {
            this.leftWall = leftWall;
            this.rightWall = rightWall;
            this.floor = floor;
            this.ceiling = ceiling;
        }
        public bool isAFloor()
        {
            return floor;
        }
        public bool isACeiling()
        {
            return ceiling;
        }
        public bool isALeftWall()
        {
            return leftWall;
        }
        public bool isARightWall()
        {
            return rightWall;
        }
    }
}
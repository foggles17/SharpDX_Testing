using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX_Testing
{
    public class Room : Frame
    {
        //public Frame objects;

        public Frame farBackground;

        public Frame midground;
        Frame closeBackground;
        Frame platformLayer;
        Frame actorLayer;
        Frame playerLayer;
        Frame closeForeground;

        public Frame farForeground;

        public List<Platform> platforms;
        public List<Actor> actors;
        public Player player;

        bool enclosed;
        int height;
        int width;
        public Room(int width, int height, bool enclosed)
        {
            //platforms = new List<Frame>();
            this.width = width;
            this.height = height;
            this.enclosed = enclosed;
            platforms = new List<Platform>();
            actors = new List<Actor>();
            player = new Player();
        }
        public float minX()
        {
            return (width - 1) * -TCM_Graphics.form.ClientSize.Width;
        }
        public float minY()
        {
            return (height - 1) * -TCM_Graphics.form.ClientSize.Height;
        }
        public void move(float dx, float dy)
        {
            float roundedX = (float)Math.Round(-dx);
            float roundedY = (float)Math.Round(-dy);
            float currentX = transform.M31;
            float currentY = transform.M32;

            float changeX = roundedX;
            float changeY = roundedY;

            if (roundedX + currentX > 0)
                changeX = -currentX;
            else if (roundedX + currentX < minX())
                changeX = minX() - currentX;

            if (roundedY + currentY > 0)
                changeY = -currentY;
            else if (roundedY + currentY < minY())
                changeY = minY() - currentY;

            applyTransform(TCM_Matrix3x2.translate(changeX, changeY));
            farBackground.applyTransform(TCM_Matrix3x2.translate(-changeX / 2, -changeY / 2));
            farForeground.applyTransform(TCM_Matrix3x2.translate(changeX, changeY));
        }
        public override void stepFinish()
        {
            draw();
            base.stepFinish();
        }
        public void initialize()
        {
            farBackground = new Frame();

            midground = new Frame();
            closeBackground = new Frame();
            platformLayer = new Frame();
            actorLayer = new Frame();
            playerLayer = new Frame();
            closeForeground = new Frame();

            farForeground = new Frame();

            addChild(farBackground, true);

            addChild(midground, false);
            midground.addChild(closeBackground, true);
            midground.addChild(platformLayer, false);
            midground.addChild(actorLayer, false);
            midground.addChild(playerLayer, false);
            midground.addChild(closeForeground, false);
            playerLayer.addChild(player, false);
            actorLayer.addChild(new Enemy(), false);

            addChild(farForeground, false);

            BitmapVisual backgroundBitmap = new BitmapVisual(TCM_Graphics.loadPNG("wall1"));
            backgroundBitmap.opacity = .5f;
            backgroundBitmap.setBoundingBox(0, 0, 1920 + (width-1)*16*60, 1080 + (height-1)*9*60);
            farBackground.addVisual(backgroundBitmap);
            int squaresWide = 16 * width + 1;
            int squaresTall = 9 * height + 1;

            string wallImage = "wall1";

            if (enclosed)
            {

                //makes the 4 walls
                Platform topPlatform = new Platform();
                topPlatform.applyTransform(TCM_Matrix3x2.translate(-60, -60));
                topPlatform.setMainBox(120 + 1920 * width, 120, Anchor.topLeft);
                platforms.Add(topPlatform);

                Platform bottomPlatform = new Platform();
                bottomPlatform.applyTransform(TCM_Matrix3x2.translate(-60, -60 + 120 * (squaresTall - 1)));
                bottomPlatform.setMainBox(120 + 1920 * width, 120, Anchor.topLeft);
                platforms.Add(bottomPlatform);
                for (int i = 0; i < squaresWide; i++)
                {
                    topPlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
                    topPlatform.visuals[i].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(i * 120, 0, 120 * (i + 1), 120);

                    
                    bottomPlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
                    bottomPlatform.visuals[i].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(i * 120, 0, 120 * (i + 1), 120);
                }

                Platform leftPlatform = new Platform();
                leftPlatform.applyTransform(TCM_Matrix3x2.translate(-60, 60));
                leftPlatform.setMainBox(120, 120 + height * 1080, Anchor.topLeft);
                platforms.Add(leftPlatform);

                Platform rightPlatform = new Platform();
                rightPlatform.applyTransform(TCM_Matrix3x2.translate(-60 + 120 * (squaresWide - 1), 60));
                rightPlatform.setMainBox(120, 120 + height * 1080, Anchor.topLeft);
                platforms.Add(rightPlatform);
                for (int i = 0; i < squaresTall - 2; i++)
                {
                    leftPlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
                    leftPlatform.visuals[i].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(0, 120 * i, 120, 120 * (i + 1));

                    rightPlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
                    rightPlatform.visuals[i].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(0, 120 * i, 120, 120 * (i + 1));
                }
            }
            else
            {
                for (int i = 0; i < squaresWide; i++)
                {
                    Frame bottomPlatform = new Frame();
                    bottomPlatform.applyTransform(TCM_Matrix3x2.translate(-60 + 120 * i, -60 + 120 * (squaresTall - 1)));
                    bottomPlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
                    bottomPlatform.visuals[0].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 120, 120);
                    platformLayer.addChild(bottomPlatform, false);
                }
            }


            //makes the floating platform
            Platform middlePlatform = new Platform(false, false, true, false);
            middlePlatform.applyTransform(TCM_Matrix3x2.translate(360, 540));
            middlePlatform.setMainBox(120, 15, Anchor.topLeft);
            middlePlatform.addVisual(new BitmapVisual(TCM_Graphics.loadPNG(wallImage)));
            middlePlatform.visuals[0].boundingBox = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 120, 15);
            platforms.Add(middlePlatform);

            //puts the platforms into a layer
            foreach (Platform p in platforms)
            {
                platformLayer.addChild(p, false);
            }
        }
    }
}

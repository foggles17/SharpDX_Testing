using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct2D1;

namespace SharpDX_Testing
{
    public class GameRectangleSequence
    {
        public List<GameRectangle> grs;
        public List<int> grSequence;
        public int index;
        public GameRectangleSequence()
        {
            grs = new List<GameRectangle>();
            index = 0;
        }
        public void addGameRectangle(GameRectangle bv)
        {
            grs.Add(bv);
        }
        public void addHitbox(float x = 0, float y = 0, float w = 1, float h = 1)
        {
            addHitbox(new Hitbox(x, y, w, h));
        }
        public void addHitbox(Hitbox hb)
        {
            addGameRectangle(hb);
        }
        public void addBitmap(Bitmap b, float x = 0, float y = 0, float w = 1, float h = 1)
        {
            addGameRectangle(new BitmapVisual(b, x, y, w, h));
        }
        public void addBitmap(BitmapVisual bv)
        {
            addGameRectangle(bv);
        }
        public void step()
        {
            index++;
            if (index >= grSequence.Count)
                index = 0;
        }
        public GameRectangle getCurrentImage()
        {
            return grs[grSequence[index]];
        }
    }
}

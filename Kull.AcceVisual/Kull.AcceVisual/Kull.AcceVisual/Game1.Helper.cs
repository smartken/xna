using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Kull.AcceVisual
{
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {

        private void setPixel(Texture2D backgroundTexture, Color[] pixels, int x, int y, Color color)
        {
            pixels[y * backgroundTexture.Width + x] = color;
        }

        private void drawCenterCircle(Texture2D texture, Color[] pixels, int radius, Color color)
        {
            Point center = new Point(texture.Width/2,texture.Height/2);
            int halfPoint = (int)(0.707 * radius + 0.5);
            for (int y = -halfPoint; y <= halfPoint; y++) {
                int x1 = (int)Math.Round(Math.Sqrt(radius* radius-Math.Pow(y,2)));
                int x2 = -x1;

                setPixel(texture, pixels, x1 + center.X, y + center.Y, color);
                setPixel(texture, pixels, x2 + center.X, y + center.Y, color);

                setPixel(texture, pixels, y + center.X, x1 + center.Y, color);
                setPixel(texture, pixels, y + center.X, x2 + center.Y, color);
            }
        }


        private void drawFilledCenteredCircle(Texture2D texture, Color[] pixels, int radius)
        {
            Point center = new Point(texture.Width/2,texture.Height/2);
            for (int y = -radius; y < radius; y++) {
                int x1 = (int)Math.Round(Math.Sqrt(radius * radius - Math.Pow(y, 2)));
                for (int x = -x1; x < x1; x++) {
                    setPixel(texture, pixels, x + center.X, y + center.Y, Color.White);
                }
            }
        }
    }
}

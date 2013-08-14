using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Kull.Petzold
{
    public class Texture2DExt
    {

        public static Texture2D createBall(GraphicsDevice graphicsDevice, int radius)
        {
            Texture2D ball = new Texture2D(graphicsDevice,2*radius,2*radius);
            Color[] pixels = new Color[ball.Width * ball.Height];
            int raduisSqu = radius * radius;

            for (int y = -radius; y < radius; y++) {
                int x2 = (int)Math.Round(Math.Sqrt(Math.Pow(radius,2)-y*y));
                int x1 = -x2;
                for (int x = x1; x < x2; x++) {
                    pixels[(int)(ball.Width * (y + radius) + x + radius)] = Color.White;
                }
            }
            ball.SetData<Color>(pixels);
                return ball;
        }
    }
}

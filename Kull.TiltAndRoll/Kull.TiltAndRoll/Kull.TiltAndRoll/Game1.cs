using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Kull.Petzold;

namespace Kull.TiltAndRoll
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : BaseGame
    {

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
               this.Exit();
            
            Vector2 newAcce = Vector2.Zero;
            lock (acceLock)
            {
                newAcce = new Vector2(acce.X, -acce.Y);
            }

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ballVelocity += GRAVITY * newAcce * elapsedSeconds;
            ballPosition += ballVelocity * elapsedSeconds;

            if (ballPosition.X - BALL_RADIUS < 0) {
                ballPosition.X = BALL_RADIUS;
                ballVelocity.X=0;
            }
            else if (ballPosition.X + BALL_RADIUS > viewPort.Width) {
                ballPosition.X = viewPort.Width - BALL_RADIUS;
                ballVelocity.X = 0;
            }

            if (ballPosition.Y - BALL_RADIUS < 0) {
                ballPosition.Y = BALL_RADIUS;
                ballVelocity.Y = 0;
            }
            else if (ballPosition.Y + BALL_RADIUS > viewPort.Height) {
                ballPosition.Y = viewPort.Height - BALL_RADIUS;
                ballVelocity.Y = 0;
            }
            base.Update(gameTime);
     
        }


      
    }
}

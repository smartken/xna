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

namespace Kull.TiltAndBounce
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : BaseGame
    {
        const float BONCE = 2f / 3;


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
                this.Exit();
            }
            Vector2 newAcce = Vector2.Zero;
            lock (acceLock) {
                newAcce = new Vector2(acce.X, -acce.Y);
            }
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ballVelocity += GRAVITY * newAcce * elapsedSeconds;
            ballPosition += ballVelocity * elapsedSeconds;
            bool needAnotherLoop = false;

            do
            {
                needAnotherLoop = false;
                if (ballPosition.X - BALL_RADIUS < 0)
                {
                    ballPosition.X =-ballPosition.X+ BALL_RADIUS*2;
                    ballVelocity.X *= -BONCE;
                    needAnotherLoop = true;
                }
                else if (ballPosition.X + BALL_RADIUS > viewPort.Width)
                {
                    ballPosition.X = -ballPosition.X - 2 * (BALL_RADIUS - viewPort.Width);
                    ballVelocity.X *= -BONCE;
                    needAnotherLoop = true;
                }

                if (ballPosition.Y - BALL_RADIUS < 0)
                {
                    ballPosition.Y = -ballPosition.Y+2*BALL_RADIUS;
                    ballVelocity.Y *= -BONCE;
                    needAnotherLoop = true;
                }
                else if (ballPosition.Y + BALL_RADIUS > viewPort.Height)
                {
                    ballPosition.Y = -ballPosition.Y-2*(BALL_RADIUS-viewPort.Height);
                    ballVelocity.Y *= -BONCE;
                    needAnotherLoop = true;
                }
            } while (needAnotherLoop);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Navy);
            spriteBatch.Begin();
            spriteBatch.Draw(ball, ballPosition, null, Color.Pink, 0, ballCenter, 1f / BALL_SCALE, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

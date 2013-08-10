using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kull.PhreeCell
{
    public  class CongratulationsComponent:DrawableGameComponent
    {
        const float SCALE_SPEED = 0.5f;
        const float ROTATE_SPEED = 0.5f;

        SpriteBatch spiteBatch;
        SpriteFont pericles108;
        string congratulationsText = "You Win!";
        float textScale, textAngle;
        Vector2 textPosition, textOrigin;

        public CongratulationsComponent(Game game):base(game)
        {

        }

        protected override void LoadContent()
        {
            spiteBatch = new SpriteBatch(this.GraphicsDevice);
            pericles108 = this.Game.Content.Load<SpriteFont>("Pericles108");
            textOrigin = pericles108.MeasureString(congratulationsText) / 2;
            Viewport viewport = this.GraphicsDevice.Viewport;
            textPosition=new Vector2(Math.Max(viewport.Width,viewport.Height)/2,
                                      Math.Min(viewport.Width,viewport.Height)/2
                );
            base.LoadContent();
        }


        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            Visible = Enabled;
            if (Enabled) {
                textScale = textAngle = 0;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (textScale < 1) {
                textScale += SCALE_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                textAngle += ROTATE_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (textAngle != 0) {
                textScale = 1;
                textAngle = 0;
            }

            base.Update(gameTime);

        }

        public override void Draw(GameTime gameTime)
        {
            spiteBatch.Begin();
            spiteBatch.DrawString(pericles108, congratulationsText, textPosition, Color.White, textAngle, textOrigin, textScale, SpriteEffects.None, 0);
            spiteBatch.End();
            base.Draw(gameTime);
        }

    }
}

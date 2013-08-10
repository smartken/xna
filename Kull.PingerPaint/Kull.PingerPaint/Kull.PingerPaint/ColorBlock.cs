using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kull.PingerPaint
{
    public class ColorBlock : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        Texture2D block;

        public ColorBlock(Game game):base(game)
        {

        }

        public Color color { get; set; }

        public Rectangle destination { get; set; }

        public bool isSelected { get; set; }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            block = new Texture2D(this.GraphicsDevice, 1, 1);
            block.SetData<uint>(new uint[] { Color.White.PackedValue });
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle rect = destination;
            spriteBatch.Begin();
            spriteBatch.Draw(block, rect, isSelected ? Color.White : Color.DarkGray);
            rect.Inflate(-6, -6);
            spriteBatch.Draw(block, rect, color);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

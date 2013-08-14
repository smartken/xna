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
using Microsoft.Devices.Sensors;

namespace Kull.AcceGraph
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        int displayWidth, displayHeight;
        Texture2D backgroundTexture,graphTexture;
        uint[] pixels;
        int totalTicks,oldInsterRow;
        Vector3 oldAcce,acce;
        object acceLock=new object();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // 延长锁定时的电池寿命。
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 768;
        }

        /// <summary>
        /// 允许游戏在开始运行之前执行其所需的任何初始化。
        /// 游戏能够在此时查询任何所需服务并加载任何非图形
        /// 相关的内容。调用 base.Initialize 将枚举所有组件
        /// 并对其进行初始化。 
        /// </summary>
        protected override void Initialize()
        {
            // TODO: 在此处添加初始化逻辑
            Accelerometer accer = new Accelerometer();
            accer.CurrentValueChanged += (s, e) => {
                lock (acceLock) {

                    acce = e.SensorReading.Acceleration;
                }
            };
            accer.Start();
            base.Initialize();
        }

        /// <summary>
        /// 对于每个游戏会调用一次 LoadContent，
        /// 用于加载所有内容。
        /// </summary>
        protected override void LoadContent()
        {
            // 创建新的 SpriteBatch，可将其用于绘制纹理。
            spriteBatch = new SpriteBatch(GraphicsDevice);
            displayWidth = this.GraphicsDevice.Viewport.Width;
            displayHeight = this.GraphicsDevice.Viewport.Height;
            int ticksPerSecond = 1000 / this.TargetElapsedTime.Milliseconds;
            int tickPerFinfth = ticksPerSecond / 5;
            backgroundTexture = new Texture2D(this.GraphicsDevice,displayWidth,ticksPerSecond);
            pixels = new uint[backgroundTexture.Width * backgroundTexture.Height];

            for (int y = 0; y < backgroundTexture.Height; y++) {
                for (int x = 0; x < backgroundTexture.Width; x++) {
                    Color color = Color.Black;
                    if (y == 0 || x == backgroundTexture.Width / 2
                        ||x==backgroundTexture.Width/4||x==3*backgroundTexture.Width/4
                        ) {
                            color = new Color(128,128,128);
                    }else if( y% tickPerFinfth==0||((x-backgroundTexture.Width/2)%(backgroundTexture.Width/16)==0 )){
                        color = new Color(64,64,64);
                    }
                    pixels[y * backgroundTexture.Width + x] = color.PackedValue;
                }
            }
            // TODO: 在此处使用 this.Content 加载游戏内容
            backgroundTexture.SetData<uint>(pixels);

            graphTexture = new Texture2D(this.GraphicsDevice,displayWidth,displayHeight);
            pixels=new uint[graphTexture.Width*graphTexture.Height];
            oldInsterRow = graphTexture.Height - 2;
        }

        /// <summary>
        /// 对于每个游戏会调用一次 UnloadContent，
        /// 用于取消加载所有内容。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: 在此处取消加载任何非 ContentManager 内容
        }

        /// <summary>
        /// 允许游戏运行逻辑，例如更新全部内容、
        /// 检查冲突、收集输入信息以及播放音频。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Update(GameTime gameTime)
        {
            // 允许游戏退出
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: 在此处添加更新逻辑
            Vector3 newAcce;

            lock (acceLock) {
                newAcce = acce;
            }

            totalTicks = (int)Math.Round(gameTime.TotalGameTime.TotalSeconds/this.TargetElapsedTime.TotalSeconds);
            int insertRow = (totalTicks + graphTexture.Height - 1) % graphTexture.Height;
            int newInsertRow = insertRow < oldInsterRow ? insertRow + graphTexture.Height : insertRow;

            for (int y = oldInsterRow + 1; y <= newInsertRow; y++) {

                for (int x = 0; x < graphTexture.Width; x++) {
                    pixels[(y % graphTexture.Height) * graphTexture.Width + x] = 0;
                }
            }
                drawLines(graphTexture,pixels,oldInsterRow,newInsertRow,oldAcce,newAcce);
                this.GraphicsDevice.Textures[0] = null;
                if (newInsertRow >= graphTexture.Height)
                {
                    graphTexture.SetData<uint>(pixels);
                }
                else {
                    Rectangle rect = new Rectangle(0,oldInsterRow,graphTexture.Width,newInsertRow-oldInsterRow+1);
                    graphTexture.SetData<uint>(0,rect,pixels,rect.Y*rect.Width,rect.Height*rect.Width);
                }
                oldInsterRow = insertRow;
                oldAcce = newAcce;

                base.Update(gameTime);
        }

        private void drawLines(Texture2D texture, uint[] pixels, int oldInsterRow, int newInsertRow, Vector3 oldAcce, Vector3 newAcce)
        {
            drawLine(texture, pixels, oldInsterRow, newInsertRow, oldAcce.X, newAcce.X, Color.Red);
            drawLine(texture, pixels, oldInsterRow, newInsertRow, oldAcce.Y, newAcce.Y, Color.Green);
            drawLine(texture, pixels, oldInsterRow, newInsertRow, oldAcce.Z, newAcce.Z, Color.Blue);
        }

        private void drawLine(Texture2D texture, uint[] pixels, int oldInsterRow, int newInsertRow, float oldAcc, float newAcc, Color color)
        {
            drawLine(texture, pixels, texture.Width / 2 + (int)(oldAcc * texture.Width / 4), oldInsterRow,
                              texture.Width / 2 + (int)(newAcc * texture.Width / 4), newInsertRow, color
                );
        }

        private void drawLine(Texture2D texture,uint[] pixels,int x1,int y1,int x2,int y2,Color color){
            if (x1 == x2 && y1 == y2) return;
            else if (Math.Abs(y2 - y1) > Math.Abs(x2 - x1)) {
                int sign = Math.Sign(y2-y1);
                for (int y = y1; y != y2; y += sign) {
                    float t = (float)(y - y1) / (y2 - y1);
                    int x = (int)(x1 + t * (x2 - x1) + 0.5f);
                    setPixel(texture, pixels, x, y, color);
                }
            }

        }

        private void setPixel(Texture2D texture, uint[] pixels, int x, int y, Color color)
        {
            pixels[(y % texture.Height) * texture.Width + x] |= color.PackedValue;
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: 在此处添加绘图代码
            spriteBatch.Begin();
            int displayRow = -totalTicks % backgroundTexture.Height;
            while (displayRow < displayHeight) {
                spriteBatch.Draw(backgroundTexture, new Vector2(0, displayRow), Color.White);
                displayRow += backgroundTexture.Height;
            }
            displayRow = -totalTicks % graphTexture.Height;
            while (displayRow < displayHeight) {
                spriteBatch.Draw(graphTexture,new Vector2(0,displayRow),Color.White);
                displayRow += graphTexture.Height;

            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices.Sensors;

namespace Kull.Petzold
{
    public abstract class BaseGame:Game
    {
        protected const float GRAVITY = 1000;
        protected const int BALL_RADIUS = 16;
        protected const int BALL_SCALE = 16;
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;

        protected Viewport viewPort;
        protected Texture2D ball;
        protected Vector2 ballCenter, ballPosition, ballVelocity = Vector2.Zero;
        protected Vector3 oldAcce, acce;
        protected object acceLock = new object();


        public BaseGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            
            // 延长锁定时的电池寿命。
            InactiveSleepTime = TimeSpan.FromSeconds(1);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 480;
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
                    acce = 0.5f * oldAcce + 0.5f * e.SensorReading.Acceleration;
                    oldAcce = acce;
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

            // TODO: 在此处使用 this.Content 加载游戏内容

            viewPort = this.GraphicsDevice.Viewport;
            ball= Texture2DExt.createBall(this.GraphicsDevice, BALL_RADIUS * BALL_SCALE);
            ballCenter = new Vector2(ball.Width/2,ball.Height/2);
            ballPosition = new Vector2(viewPort.Width/2,viewPort.Height/2);
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
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
             //   this.Exit();

            // TODO: 在此处添加更新逻辑
            

            

            base.Update(gameTime);
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
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

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
using System.Text;
using Microsoft.Devices.Sensors;

namespace Kull.AcceVisual
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        const int BALL_RADIUS = 8;
        Accelerometer acce ;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Viewport viewport;
        SpriteFont segoe14;
        StringBuilder stringBuilder = new StringBuilder();
        int unitRadius;
        Vector2 screenCenter, backGroundTextureCenter,ballTextureCenter, ballPosition, ballTexturePosition;
        Texture2D backgroundTexture, ballTexture;

        float ballScale;
        bool isZNegative;

        Vector3 acceVector, oldAcce, minAcce=2*Vector3.One, maxAcce=-2*Vector3.One;
        object acceVectorLock = new object();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // 延长锁定时的电池寿命。
           // InactiveSleepTime = TimeSpan.FromSeconds(1);

            graphics.PreferredBackBufferWidth = 728;
            graphics.PreferredBackBufferHeight = 480;
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
            acce = new Accelerometer();
            
            acce.CurrentValueChanged+= new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(onAcceReadingChanged);
            acce.Start();
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
            viewport = this.GraphicsDevice.Viewport;
            screenCenter = new Vector2(viewport.Width/2,viewport.Height/2);
            segoe14 = this.Content.Load<SpriteFont>("Segoe14");
            unitRadius = (viewport.Height-BALL_RADIUS)/2;
            backgroundTexture = new Texture2D(this.GraphicsDevice,viewport.Height,viewport.Height);
            backGroundTextureCenter = new Vector2(viewport.Height / 2, viewport.Height / 2);

            Color[] pixels = new Color[backgroundTexture.Width * backgroundTexture.Height];

            for (int x = 0; x < backgroundTexture.Width; x++) {
                setPixel(backgroundTexture, pixels,x,backgroundTexture.Height/2,Color.White);
            }

            for (int y = 0; y < backgroundTexture.Height; y++) {
                setPixel(backgroundTexture, pixels, backgroundTexture.Width / 2, y, Color.White);
            }

            drawCenterCircle(backgroundTexture,pixels,unitRadius,Color.White);
            drawCenterCircle(backgroundTexture, pixels, 3*unitRadius/4, Color.Gray);
            drawCenterCircle(backgroundTexture, pixels, unitRadius/2, Color.White);
            drawCenterCircle(backgroundTexture, pixels, unitRadius/4, Color.Gray);
            drawCenterCircle(backgroundTexture, pixels, BALL_RADIUS, Color.White);
            backgroundTexture.SetData<Color>(pixels);

            ballTexture = new Texture2D(this.GraphicsDevice,2*BALL_RADIUS,2*BALL_RADIUS);
            ballTextureCenter = new Vector2(BALL_RADIUS,BALL_RADIUS);
            pixels=new Color[ballTexture.Width*ballTexture.Height];
            drawFilledCenteredCircle(ballTexture, pixels, BALL_RADIUS);
            ballTexture.SetData<Color>(pixels);
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
            Vector3 newAcce = Vector3.Zero;
            lock (acceVectorLock) {
                newAcce = acceVector;
            }
            maxAcce = Vector3.Max(maxAcce,newAcce);
            minAcce = Vector3.Min(minAcce, newAcce);
            Vector3 avgAcce = 0.5f * oldAcce + 0.5f * newAcce;
            avgAcce = 0.75f * oldAcce + 0.25f * newAcce;
            stringBuilder.Clear();
            stringBuilder.AppendFormat("Raw:({0:F2},{1:F2},{2:F2})={2:F2} \n"
                ,newAcce.X,newAcce.Y,newAcce.Z,newAcce.Length()
                );
            stringBuilder.AppendFormat("Avg:({0:F2},{1:F2},{2:F2})={2:F2} \n"
                , avgAcce.X, avgAcce.Y, avgAcce.Z, avgAcce.Length()
                );
            stringBuilder.AppendFormat("Min:({0:F2},{1:F2},{2:F2})={2:F2} \n"
                , minAcce.X, minAcce.Y, minAcce.Z, minAcce.Length()
                );
            stringBuilder.AppendFormat("Max:({0:F2},{1:F2},{2:F2})={2:F2} \n"
                , maxAcce.X, maxAcce.Y, maxAcce.Z, maxAcce.Length()
                );

            ballScale = avgAcce.Length();

            int sign = this.Window.CurrentOrientation == DisplayOrientation.LandscapeLeft ? 1 : -1;

            ballPosition = new Vector2(
                    screenCenter.X+sign*unitRadius*avgAcce.Y/ballScale
                  , screenCenter.Y+ sign * unitRadius * avgAcce.X / ballScale
                );
            isZNegative = avgAcce.Z < 0;
            oldAcce = avgAcce;
            base.Update(gameTime);
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: 在此处添加绘图代码
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture,screenCenter,null,Color.White,0,backGroundTextureCenter,1,SpriteEffects.None,0);
            spriteBatch.Draw(ballTexture,ballPosition,null,isZNegative?Color.Red:Color.Lime,0,ballTextureCenter,ballScale,SpriteEffects.None,0);
            spriteBatch.DrawString(segoe14, stringBuilder, Vector2.Zero, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }


        protected void onAcceReadingChanged(object sender, SensorReadingEventArgs<AccelerometerReading> args)
        {
            lock (acceVectorLock) {
                acceVector = new Vector3((float)args.SensorReading.Acceleration.X,(float)args.SensorReading.Acceleration.Y,(float)args.SensorReading.Acceleration.Z);
            }
        }
    }
}

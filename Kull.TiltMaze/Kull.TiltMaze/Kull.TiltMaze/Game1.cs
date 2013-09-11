using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Petzold.Phone.Xna;

namespace Kull.TiltMaze
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
         const float GRAVITY = 1000, BOUNCE = 2f/3;
        const int BALL_RADIUS = 16, BALL_SCALE = 16, WALL_WIDTH = 32;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Viewport viewport;
        private Texture2D tinyTexture2D,ball;

        MazeGrid mazedGrid=new MazeGrid(5,8);
        List<Line2D> borders=new List<Line2D>();

        private Vector2 ballCenter, ballPosition, ballVelocity = Vector2.Zero;
        private Vector3 oldAcce, acce;
        object acceLock=new object();

        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 768;

            // 延长锁定时的电池寿命。
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        private Accelerometer accer;
        /// <summary>
        /// 允许游戏在开始运行之前执行其所需的任何初始化。
        /// 游戏能够在此时查询任何所需服务并加载任何非图形
        /// 相关的内容。调用 base.Initialize 将枚举所有组件
        /// 并对其进行初始化。 
        /// </summary>
        protected override void Initialize()
        {
            // TODO: 在此处添加初始化逻辑
            accer = new Accelerometer();
            accer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(onAcceReadingChanged);
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
            viewport = this.GraphicsDevice.Viewport;

            tinyTexture2D=new Texture2D(this.GraphicsDevice,1,1);
            tinyTexture2D.SetData(new Color[]{Color.White});

            ball = Texture2DExtensions.CreateBall(this.GraphicsDevice, BALL_RADIUS*BALL_SCALE);
            ballCenter=new Vector2(ball.Width/2,ball.Height/2);
            ballPosition=new Vector2((viewport.Width/mazedGrid.Width)/2,(viewport.Height/mazedGrid.Height)/2);
            borders.Clear();

            int cellWidth = viewport.Width/mazedGrid.Width, cellHeight = viewport.Height/mazedGrid.Height
                ,halfWallWidth=WALL_WIDTH/2
                ;
            for (int x = 0; x < mazedGrid.Width; x++)
            {
                for (int y = 0; y < mazedGrid.Height; y++)
                {
                    MazeCell mazeCell = mazedGrid.Cells[x, y];
                    Vector2 ll = new Vector2(x*cellWidth, (y + 1)*cellHeight)
                        ,
                        ul = new Vector2(x*cellWidth, y*cellHeight)
                        ,
                        ur = new Vector2((x + 1)*cellWidth, y*cellHeight)
                        ,
                        lr = new Vector2((x + 1)*cellWidth, (y + 1)*cellHeight)
                        ,
                        right = halfWallWidth*Vector2.UnitX
                        ,
                        left = -right
                        ,down=halfWallWidth*Vector2.UnitY
                        ,up=-down
                       ;
                    if (mazeCell.HasLeft)
                    {
                        borders.Add(new Line2D(ll+down,ll+down+right));
                        borders.Add(new Line2D(ll + down+right, ul + up + right));
                        borders.Add(new Line2D(ul + up+right, ul+up));
                    }
                    if (mazeCell.HasTop)
                    {
                        borders.Add(new Line2D(ul + left, ul + left + down));
                        borders.Add(new Line2D(ul + left + down, ur + right + down));
                        borders.Add(new Line2D(ur + right+down, ur + right));
                    }
                    if (mazeCell.HasRight)
                    {
                        borders.Add(new Line2D(ur + up, ur + up + left));
                        borders.Add(new Line2D(ur + up + left, lr + down + left));
                        borders.Add(new Line2D(lr + down + left, lr + down));
                    }
                    if (mazeCell.HasBottom)
                    {
                        borders.Add(new Line2D(lr + right, lr + right + up));
                        borders.Add(new Line2D(lr + up + right, ll + left + up));
                        borders.Add(new Line2D(ll + left + up, ll + left));
                    }
                }
            }
            // TODO: 在此处使用 this.Content 加载游戏内容
        }


        private void onAcceReadingChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            lock (acceLock)
            {
                this.acce = 0.5f*oldAcce +0.5f* new Vector3(
                            (float)e.SensorReading.Acceleration.X,
                            (float)e.SensorReading.Acceleration.Y
                         , (float)e.SensorReading.Acceleration.Z
                    );
                this.oldAcce = acce;
            }
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
            Vector2 newAcce = Vector2.Zero;

            lock (acceLock)
            {
                newAcce=new Vector2(acce.X,-acce.Y);
            }
            float elapsedSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
            ballVelocity += GRAVITY*newAcce*elapsedSeconds;
            Vector2 oldPosition = ballPosition;
            ballPosition += ballVelocity*elapsedSeconds;

            bool needAnotherLoop = false;

            do
            {
                needAnotherLoop = false;
                foreach (Line2D line in borders)
                {
                    Line2D shiftedLine = line.ShiftOut(BALL_RADIUS*line.Normal)
                        ,ballTrajectory=new Line2D(oldPosition,ballPosition)
                        ;
                    Vector2 intersetion = shiftedLine.SegmentIntersection(ballTrajectory);
                    float angleDiff = MathHelper.WrapAngle(line.Angle - ballTrajectory.Angle);

                    if (Line2D.IsValid(intersetion) && angleDiff > 0 &&
                        Line2D.IsValid(Vector2.Normalize(ballVelocity))
                        )
                    {
                        float beyond = (ballPosition - intersetion).Length();
                        ballVelocity = BOUNCE*Vector2.Reflect(ballVelocity, line.Normal);
                        ballPosition = intersetion + beyond*Vector2.Normalize(ballVelocity);
                        needAnotherLoop = true;
                        break;
                    }
                }
            } while (needAnotherLoop);
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

             int cellWidth = viewport.Width/mazedGrid.Width, cellHeight = viewport.Height/mazedGrid.Height
                ,halfWallWidth=WALL_WIDTH/2
                ;
            for (int x = 0; x < mazedGrid.Width; x++)
            {
                for (int y = 0; y < mazedGrid.Height; y++)
                {
                    MazeCell mazeCell = mazedGrid.Cells[x, y];

                    if (mazeCell.HasLeft)
                    {
                        Rectangle rect = new Rectangle(x*cellWidth, y*cellHeight - halfWallWidth, halfWallWidth,
                            cellHeight + WALL_WIDTH);
                        spriteBatch.Draw(tinyTexture2D, rect, Color.Green);
                    }
                    if (mazeCell.HasTop)
                    {
                        Rectangle rect = new Rectangle(x*cellWidth - halfWallWidth, y*cellHeight, cellWidth + WALL_WIDTH,
                            halfWallWidth);
                        spriteBatch.Draw(tinyTexture2D, rect, Color.Green);

                    }
                    if (mazeCell.HasRight)
                    {
                        Rectangle rect = new Rectangle((x + 1)*cellWidth - halfWallWidth, y*cellHeight - halfWallWidth,
                            halfWallWidth, cellHeight + WALL_WIDTH);
                        spriteBatch.Draw(tinyTexture2D, rect, Color.Green);

                    }
                    if (mazeCell.HasBottom)
                    {

                        Rectangle rect = new Rectangle(x*cellWidth - halfWallWidth, (y + 1)*cellHeight - halfWallWidth,
                            cellWidth + WALL_WIDTH, halfWallWidth);
                        spriteBatch.Draw(tinyTexture2D, rect, Color.Green);
                    }
                }
            }
            spriteBatch.Draw(ball,ballPosition,null,Color.Pink,0,ballCenter,1f/BALL_SCALE,SpriteEffects.None,0);
            spriteBatch.End();
            base.Draw(gameTime);
        }



    }
}

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

            // Windows Phone ��Ĭ��֡����Ϊ 30 fps��
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            
            // �ӳ�����ʱ�ĵ��������
            InactiveSleepTime = TimeSpan.FromSeconds(1);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 480;
        }

        /// <summary>
        /// ������Ϸ�ڿ�ʼ����֮ǰִ����������κγ�ʼ����
        /// ��Ϸ�ܹ��ڴ�ʱ��ѯ�κ�������񲢼����κη�ͼ��
        /// ��ص����ݡ����� base.Initialize ��ö���������
        /// ��������г�ʼ���� 
        /// </summary>
        protected override void Initialize()
        {
            // TODO: �ڴ˴���ӳ�ʼ���߼�
           
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
        /// ����ÿ����Ϸ�����һ�� LoadContent��
        /// ���ڼ����������ݡ�
        /// </summary>
        protected override void LoadContent()
        {
            // �����µ� SpriteBatch���ɽ������ڻ�������
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: �ڴ˴�ʹ�� this.Content ������Ϸ����

            viewPort = this.GraphicsDevice.Viewport;
            ball= Texture2DExt.createBall(this.GraphicsDevice, BALL_RADIUS * BALL_SCALE);
            ballCenter = new Vector2(ball.Width/2,ball.Height/2);
            ballPosition = new Vector2(viewPort.Width/2,viewPort.Height/2);
        }

        /// <summary>
        /// ����ÿ����Ϸ�����һ�� UnloadContent��
        /// ����ȡ�������������ݡ�
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: �ڴ˴�ȡ�������κη� ContentManager ����
        }

        /// <summary>
        /// ������Ϸ�����߼����������ȫ�����ݡ�
        /// ����ͻ���ռ�������Ϣ�Լ�������Ƶ��
        /// </summary>
        /// <param name="gameTime">�ṩ��ʱֵ�Ŀ��ա�</param>
        protected override void Update(GameTime gameTime)
        {
            // ������Ϸ�˳�
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
             //   this.Exit();

            // TODO: �ڴ˴���Ӹ����߼�
            

            

            base.Update(gameTime);
        }

        /// <summary>
        /// ����Ϸ�ý������һ���ʱ���ô��
        /// </summary>
        /// <param name="gameTime">�ṩ��ʱֵ�Ŀ��ա�</param>
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

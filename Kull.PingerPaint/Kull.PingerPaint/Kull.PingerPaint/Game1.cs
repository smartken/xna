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
using Petzold.Phone.Xna;


namespace Kull.PingerPaint
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D canvas;
        Vector2 canvasSize, canvasPosition;
        uint[] pixels;
        List<float> xCollection = new List<float>();

        Button clearButton, saveButton;
        string filename;

        List<ColorBlock> colorBlocks = new List<ColorBlock>();
        Color drawingColor = Color.Blue;
        int? touchIdToIgnore;
        bool isNeedUpdate = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // 延长锁定时的电池寿命。
            //InactiveSleepTime = TimeSpan.FromSeconds(1);
            this.graphics.PreferredBackBufferWidth = 480;
            this.graphics.PreferredBackBufferHeight = 768;

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
            clearButton = new Button(this, "clear");
            clearButton.Click += btnClear_onclick;
            this.Components.Add(clearButton);

            saveButton = new Button(this, "save");
            saveButton.Click += btnSave_onclick;
            this.Components.Add(saveButton);

            Color[] colors ={
                            Color.Red,Color.Green,Color.Blue
                            ,Color.Cyan,Color.Magenta,Color.Yellow
                            ,Color.Black,new Color(0.2f,0.2f,0.2f),new Color(0.4f,0.4f,0.4f)
                            ,new Color(0.6f,0.6f,0.6f),new Color(0.8f,0.8f,0.8f),Color.White
                           };
            foreach (Color color in colors) {
                ColorBlock cb = new ColorBlock(this);
                cb.color = color;
                colorBlocks.Add(cb);
                this.Components.Add(cb);
            }
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
            Rectangle clientBounds = this.GraphicsDevice.Viewport.Bounds;
            SpriteFont segoe14 = this.Content.Load<SpriteFont>("Segoe14");
            clearButton.SpriteFont = segoe14;
            saveButton.SpriteFont = segoe14;

            Vector2 textSize = segoe14.MeasureString(clearButton.Text);
            int buttonWidth = (int)(2 * textSize.X);
            int buttonHeight = (int)(1.5 * textSize.Y);

            clearButton.Destination = new Rectangle(
                     clientBounds.Left+20,clientBounds.Bottom-2-buttonHeight
                     ,buttonWidth,buttonHeight
                );

            saveButton.Destination = new Rectangle(
                    clientBounds.Right - 20-buttonWidth, clientBounds.Bottom - 2 - buttonHeight
                    , buttonWidth, buttonHeight
               );

            int colorBlockSize = clientBounds.Width / (colorBlocks.Count / 2) - 2;
            int xColorBlock = 2;
            int yColorBlock = 2;

            foreach (ColorBlock colorBlock in colorBlocks) {
                colorBlock.destination = new Rectangle(xColorBlock, yColorBlock, colorBlockSize, colorBlockSize);
                xColorBlock += colorBlockSize + 2;
                if (xColorBlock + colorBlockSize > clientBounds.Width) {
                    xColorBlock = 2;
                    yColorBlock += colorBlockSize + 2;
                }
            }

            canvasPosition = new Vector2(0, 2 * colorBlockSize + 6);
            canvasSize = new Vector2(clientBounds.Width, clientBounds.Height - canvasPosition.Y - buttonHeight - 4);
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

            if (!string.IsNullOrWhiteSpace(filename)) {
                canvas.SaveToPhotoLibrary(filename);
                filename = string.Empty;
            }
            bool canvasNeedsUpdate = false;
            TouchCollection touches = TouchPanel.GetState();

            foreach( TouchLocation touch in touches){
                if (touchIdToIgnore.HasValue && touch.Id == touchIdToIgnore.Value) continue;

                bool touchHandled = false;

                foreach (GameComponent compnent in this.Components) {
                    if (compnent is IProcessTouch && (compnent as IProcessTouch).ProcessTouch(touch)) {
                        touchHandled = true;
                        break;
                    }
                }

                if (touchHandled) return;
                switch (touch.State) {
                    case TouchLocationState.Pressed: {
                        Vector2 postion = touch.Position;
                        ColorBlock newSelectColorBolck = null;
                        foreach (ColorBlock colorBolock in colorBlocks) {
                            Rectangle rect = colorBolock.destination;
                            if (postion.X >= rect.Left && postion.X < rect.Right && postion.Y >= rect.Top && postion.Y < rect.Bottom) {
                                drawingColor = colorBolock.color;
                                newSelectColorBolck = colorBolock;
                            }
                        }
                        if (newSelectColorBolck == null)
                        {
                            touchIdToIgnore = null;
                        }
                        else {
                            foreach (ColorBlock colorBlock in colorBlocks) {
                                colorBlock.isSelected = colorBlock == newSelectColorBolck;
                            }
                            touchIdToIgnore = touch.Id;
                        }
                        break; }
                    case TouchLocationState.Moved: {
                        TouchLocation prevTouchLocation;
                        touch.TryGetPreviousLocation(out prevTouchLocation);
                        Vector2 point1 = prevTouchLocation.Position - canvasPosition;
                        Vector2 point2 = touch.Position - canvasPosition;
                        float radius = 12;
                        RoundCappedLine line = new RoundCappedLine(point1, point2, radius);
                        int yMin = (int)(Math.Min(point1.Y, point2.Y) - radius);
                        int yMax = (int)(Math.Max(point1.Y, point2.Y) - radius);

                        yMin = Math.Max(0,Math.Min(canvas.Height,yMin));
                        yMax = Math.Max(0, Math.Min(canvas.Height, yMax));

                        for (int y = yMin; y < yMax; y++) {
                            xCollection.Clear();
                            line.GetAllX(y, xCollection);

                            if (xCollection.Count == 2) {
                                int xMin = (int)(Math.Min(xCollection[0], xCollection[1]) + 0.5f);
                                int xMax = (int)(Math.Max(xCollection[0], xCollection[1]) + 0.5f);
                                xMin = Math.Max(0, Math.Min(canvas.Width, xMin));
                                xMax = Math.Max(0,Math.Min(canvas.Width,xMax));

                                for (int x = xMin; x < xMax; x++) {
                                    pixels[y * canvas.Width + x] = drawingColor.PackedValue;
                                }
                                
                                canvasNeedsUpdate = true;
                            }
                            if (canvasNeedsUpdate) {
                                canvas.SetData<uint>(pixels);
                                base.Update(gameTime);
                            }
                            
                        }
                            break;
                    }
                }

            }



            base.Update(gameTime);
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: 在此处添加绘图代码
            spriteBatch.Begin();
            spriteBatch.Draw(canvas,canvasPosition, Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            bool newlyCreated = false;
            canvas = Texture2DExtensions.LoadFromPhoneServiceState(this.GraphicsDevice, "canvas");
            if (canvas == null) {
                canvas = new Texture2D(this.GraphicsDevice,(int)canvasSize.X,(int)canvasSize.Y);
                newlyCreated = true;
            }
            pixels=new uint[canvas.Width*canvas.Height];
            canvas.GetData<uint>(pixels);
            if (newlyCreated) {
                clearPixelArray();
            }

          // if (PhoneApplicationService.Current.State.ContainsKey("color")) {
         //       drawingColor = (Color)PhoneApplicationService.Current.State["color"];
         //   }

            foreach (ColorBlock colorBlock in colorBlocks) {
                colorBlock.isSelected = colorBlock.color == drawingColor;
            }
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
           // PhoneApplicationService.Current.State["color"] = drawingColor;
            canvas.SaveToPhoneServiceState("canvas");
            base.OnDeactivated(sender, args);
        }

        protected void btnClear_onclick(object sender, EventArgs args) {
            clearPixelArray();
        }

        protected void btnSave_onclick(object sender, EventArgs args)
        {
            DateTime dt = DateTime.Now;
            filename = string.Format("PhingerPaint: {0} {1}",
                 DateTime.Now.ToLongDateString()
                ,DateTime.Now.ToLongTimeString());
            Guide.BeginShowKeyboardInput(PlayerIndex.One,"save file","enter filename:",filename,keyboardCallBack,null);
        }

        void clearPixelArray() {
            for (int y = 0; y < canvas.Height; y++) {
                for (int x = 0; x < canvas.Width; x++) {
                    pixels[x + canvas.Width * y] = Color.GhostWhite.PackedValue;
                }
            }
            canvas.SetData<uint>(pixels);
        }

        void keyboardCallBack(IAsyncResult result) {
            filename = Guide.EndShowKeyboardInput(result);
        }
    }
}

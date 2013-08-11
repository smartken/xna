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
using Microsoft.Phone.Shell;

namespace Kull.PhreeCell
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        static readonly TimeSpan AutoMoveDuration = TimeSpan.FromSeconds(0.25);
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        CongratulationsComponent congratsComponent;
        Texture2D cards, surface;
        Rectangle[] cardSpots = new Rectangle[16];

        Matrix displayMatrix, inverseMatrix;
        CardInfo[] deck = new CardInfo[52], holds = new CardInfo[4];
        List<CardInfo>[] piles = new List<CardInfo>[8], finals = new List<CardInfo>[4];

        bool firstDragInGesture = true;
        CardInfo touchedCard;
        Vector2 touchedCardPosition;
        object touchedCardOrigin;
        int touchedCardOriginIndex;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // 延长锁定时的电池寿命。
            InactiveSleepTime = TimeSpan.FromSeconds(1);
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 800;
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag | GestureType.DragComplete;
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

            for (int suit = 0; suit < CardInfo.suits.Length; suit++)
            {
                for (int rank = 0; rank < CardInfo.ranks.Length; rank++)
                {
                    CardInfo card = new CardInfo(suit, rank);
                    deck[suit * CardInfo.ranks.Length + rank] = card;
                }

            }

            for (int pile = 0; pile < 8; pile++)
            {

                piles[pile] = new List<CardInfo>();
            }

            for (int final = 0; final < 4; final++)
            {
                finals[final] = new List<CardInfo>();
            }

            congratsComponent = new CongratulationsComponent(this);
            congratsComponent.Enabled = false;
            this.Components.Add(congratsComponent);
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

            cards = this.Content.Load<Texture2D>("cards");
            createCardSpots(cardSpots);
            surface = createSurface(this.GraphicsDevice, cardSpots);
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
            bool checkForNextAutoMove = false;

            foreach (List<CardInfo> final in finals) {
                foreach (CardInfo card in final) {
                    if (card.AutoMoveTime > TimeSpan.Zero) {
                        card.AutoMoveTime -= gameTime.ElapsedGameTime;
                        if (card.AutoMoveTime <= TimeSpan.Zero) {
                            card.AutoMoveTime = TimeSpan.Zero;
                            checkForNextAutoMove = true;
                        }
                        card.AutoMoveInterpolation = (float)card.AutoMoveTime.Ticks / AutoMoveDuration.Ticks;

                    }
                }
                if (checkForNextAutoMove && !AnalyzeForAntoMove() && hasWon()) {
                    congratsComponent.Enabled = true;
                }
            }

            while (TouchPanel.IsGestureAvailable) {
                GestureSample gesture = TouchPanel.ReadGesture();
                Vector2 position = Vector2.Transform(gesture.Position,inverseMatrix);
                Vector2 delta = position - Vector2.Transform(gesture.Position-gesture.Delta,inverseMatrix);

                switch (gesture.GestureType) { 
                
                    case GestureType.Tap:
                        if ((position - centerReplay).Length() < radiusReplay) {
                            congratsComponent.Enabled = false;
                            replay();
                        }
                        break;
                    case GestureType.FreeDrag:
                        if (touchedCard != null) {
                            touchedCardPosition += delta;
                        }
                        else if (firstDragInGesture) {
                            tryPickUpCard(position);
                        }
                        firstDragInGesture = false;
                        break;
                    case GestureType.DragComplete:
                        if (touchedCard != null && tryPutDownCard(touchedCard)) {
                            calculateDisplayMatrix();
                            if (!AnalyzeForAntoMove() && hasWon()) {
                                congratsComponent.Enabled = true;
                            }
                        }
                        firstDragInGesture = true;
                        touchedCard = null;
                        break;
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: 在此处添加绘图代码
            spriteBatch.Begin(SpriteSortMode.Immediate,null,null,null,null,null,displayMatrix);
            spriteBatch.Draw(surface,Vector2.Zero,Color.White);


            for (int hold = 0; hold < CardInfo.suits.Length; hold++) {
                CardInfo cardInfo = holds[hold];
                if (cardInfo == null) continue;
                Rectangle source = GetCardTextureSource(cardInfo);
                Vector2 destination = new Vector2(cardSpots[hold].X,cardSpots[hold].Y);
                spriteBatch.Draw(cards, destination, source, Color.White);
            }

            for (int pile = 0; pile < 8; pile++) { 
              Rectangle cardSpot=cardSpots[pile+8];
              for (int card = 0; card < piles[pile].Count; card++) {
                  CardInfo cardInfo = piles[pile][card];
                  Rectangle source = GetCardTextureSource(cardInfo);
                  Vector2 destination = new Vector2(cardSpot.X,cardSpot.Y+card*yOverlay);
                  spriteBatch.Draw(cards,destination,source,Color.White);
              }
            }

            for (int pass = 0; pass < 2; pass++) {
                for (int final = 0; final < 4; final++) {
                    for (int card = 0; card < finals[final].Count; card++) {
                        CardInfo cardInfo = finals[final][card];
                        if (pass == 0 && cardInfo.AutoMoveInterpolation == 0 ||
                            pass == 1 && cardInfo.AutoMoveInterpolation != 0
                            ) {
                                Rectangle source = GetCardTextureSource(cardInfo);
                                Vector2 destination = new Vector2(cardSpots[final+4].X,cardSpots[final+4].Y)+
                                    cardInfo.AutoMoveInterpolation*cardInfo.AutoMoveOffset
                                    ;
                                spriteBatch.Draw(cards, destination, source, Color.White);
                        }
                    }
                }
            }

            if (touchedCard != null) {
                Rectangle source = GetCardTextureSource(touchedCard);
                //spriteBatch.Draw(cards, touchedCardPosition, source, Color.White);
            }

                spriteBatch.End();
            base.Draw(gameTime);
        }

      


        protected override void OnDeactivated(object sender, EventArgs args)
        {
            PhoneApplicationService appService = PhoneApplicationService.Current;
            List<int>[] piles = new List<int>[8];
            for (int i = 0; i < piles.Length; i++) {
                piles[i] = new List<int>();
                foreach (CardInfo cardinof in this.piles[i]) {
                    piles[i].Add(13*cardinof.Suit+cardinof.Rank);
                }
            }
            appService.State["piles"] = piles;

            List<int>[] finals = new List<int>[4];

            for (int i = 0; i < finals.Length; i++) {
                finals[i] = new List<int>();
                foreach (CardInfo cardinfo in this.finals[i]) {
                    finals[i].Add(13*cardinfo.Suit+cardinfo.Rank);
                }
            }
            appService.State["finals"] = finals;

            int[] holds = new int[4];

            for (int i = 0; i < holds.Length; i++) {
                if (this.holds[i] == null)
                {
                    holds[i] = -1;
                }
                else {
                    holds[i] = CardInfo.ranks.Length * this.holds[i].Suit + this.holds[i].Rank;
                }
            }
            appService.State["holds"] = holds;
            base.OnDeactivated(sender,args);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            PhoneApplicationService appService = PhoneApplicationService.Current;
            if (appService.State.ContainsKey("piles"))
            {
                List<int>[] piles = appService.State["piles"] as List<int>[];

                for (int i = 0; i < piles.Length; i++)
                {
                    foreach (int cardindex in piles[i])
                    {
                        this.piles[i].Add(deck[cardindex]);
                    }
                }

                List<int>[] finals = appService.State["finals"] as List<int>[];

                for (int i = 0; i < finals.Length; i++)
                {
                    foreach (int cardindex in finals[i])
                    {
                        this.finals[i].Add(deck[cardindex]);
                    }
                }
                int[] holds = appService.State["holds"] as int[];

                for (int i = 0; i < holds.Length; i++)
                {
                    if (holds[i] != -1)
                    {
                        this.holds[i] = deck[holds[i]];
                    }

                }
                calculateDisplayMatrix();
            }
            else {
                replay();
            }
            base.OnActivated(sender, args);
        }

        protected void createCardSpots(Rectangle[] rects) {

            int x = xMargin, y = yMargin;
            for (int i = 0; i < 8; i++) {
                cardSpots[i] = new Rectangle(x,y,wCard,hCard);
                x += wCard + (i == 3 ? xMidGap : xGap);
            }
            x = xMargin + xIndent;
            y += hCard + yGap;
            for (int i = 8; i < 16; i++) {
                cardSpots[i] = new Rectangle(x,y,wCard,hCard);
                x += wCard + xGap;
            }
        }


        protected Texture2D createSurface(GraphicsDevice device, Rectangle[] rects) {
            uint backgroudColor = new Color(0, 0, 0x60).PackedValue
                ,outlineColor=Color.White.PackedValue
                ,replayColor=Color.Red.PackedValue
                ;
            Texture2D surface = new Texture2D(device, wSurface, hSurface);
            uint[] pixels = new uint[wSurface*hSurface];
            for (int i = 0; i < pixels.Length; i++) {
                Vector2 v = new Vector2(i % wSurface, i / wSurface)-centerReplay;
                if (v.LengthSquared() < radiusReplay * radiusReplay)
                {
                    pixels[i] = replayColor;
                }
                else {
                    pixels[i] = backgroudColor;
                }
            }

            foreach(Rectangle rect in rects){
                for (int x = 0; x < wCard; x++) {
                    pixels[(rect.Top - 1) * wSurface + rect.Left + x] = outlineColor;
                    pixels[rect.Bottom * wSurface + rect.Left + x] = outlineColor;
                }

                for (int y = 0; y < hCard; y++) {
                    pixels[(rect.Top + y) * wSurface + rect.Left - 1] = outlineColor;
                    pixels[(rect.Top + y) * wSurface + rect.Right] = outlineColor;
                }
            }
            surface.SetData<uint>(pixels);
                return surface;
        }


    }
}

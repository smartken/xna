using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Kull.PhreeCell
{
    public partial class Game1: Game 
    {
        const int wCard = 80, hCard = 112, wSurface = 800, xGap = 16, xMargin = 8
           , xMidGap=wSurface-(2*xMargin+8* wCard+6* xGap) 
           , xIndent=(wSurface-(2*xMargin+8* wCard+7* xGap))/2
           ,yMargin=8,yGap=16,yOverlay=28
           ,hSurface=2*yMargin+yGap+2* hCard+19* yOverlay
           ,radiusReplay=xMidGap/2-8
            ;
        static readonly Vector2 centerReplay = new Vector2(wSurface/2, xMargin+hCard/2);

        static void suffleDeck(CardInfo[] deck) {
            Random rand = new Random();
            int cardCount=CardInfo.ranks.Length*CardInfo.suits.Length;
            for (int card = 0; card < cardCount; card++) {
                int random = rand.Next(cardCount);
                CardInfo swap = deck[card];
                deck[card] = deck[random];
                deck[random] = swap;

            }
        }

        static bool isWithinRectangle(Vector2 point, Rectangle rect) {
            return point.X >= rect.Left &&
                   point.X <= rect.Right &&
                   point.Y >= rect.Top &&
                   point.Y <= rect.Bottom;
        }

        static CardInfo topCard(List<CardInfo> cardInfos) {
            return cardInfos.Count > 0 ? cardInfos.Last() : null;
        }

        void replay() {
            for (int i = 0; i < 4; i++) {
                holds[i] = null;
            }

            foreach (List<CardInfo> final in finals) {
                final.Clear();
            }

            foreach (List<CardInfo> pile in piles) {
                pile.Clear();
            }

            suffleDeck(deck);

            for (int card = 0; card < CardInfo.suits.Length * CardInfo.ranks.Length; card++) {
                piles[card % 8].Add(deck[card]);
            }
            calculateDisplayMatrix();    
        }

        void calculateDisplayMatrix() {
            int viewportHeight = this.GraphicsDevice.Viewport.Height;
            int maxCardsInPiles = 0;

            foreach (List<CardInfo> pile in piles) {
                maxCardsInPiles = Math.Max(maxCardsInPiles,pile.Count());
            }

            int requiedHeight = 2 * yMargin + yGap + 2 * hCard + yOverlay * (maxCardsInPiles - 1);
            if (requiedHeight > viewportHeight)
            {
                displayMatrix = Matrix.CreateScale(1, (float)viewportHeight / requiedHeight, 1);
            }
            else {
                displayMatrix = Matrix.Identity;
            }
            inverseMatrix = Matrix.Invert(displayMatrix);

        }

        private bool hasWon()
        {
            bool hw = true;
            foreach (List<CardInfo> cardInfos in finals) {
                hw &= cardInfos.Count > 0 && topCard(cardInfos).Rank == 12;
            }
            return hw;
        }

        

        private bool checkForAutoMove(CardInfo cardinfo)
        {
            if (cardinfo.Rank == 0) {
                for (int final = 0; final < 4; final++) {
                    finals[final].Add(cardinfo);
                    cardinfo.AutoMoveOffset = -new Vector2(cardSpots[final + 4].X, cardSpots[final + 4].Y);
                    return true;
                }
            }
            else if (cardinfo.Rank == 1)
            {
                for (int final = 0; final < 4; final++)
                {
                    CardInfo topCardInfo = topCard(finals[final]);
                    if (topCardInfo != null && topCardInfo.Suit == cardinfo.Suit && topCardInfo.Rank == 0) {
                        finals[final].Add(cardinfo);
                        cardinfo.AutoMoveOffset = -new Vector2(cardSpots[final+4].X,cardSpots[final+4].Y);
                    }
                   return true;
                }
            }
            else {
                int slot = -1;
                int count = 0;
                for (int final = 0; final < 4; final++) {
                    CardInfo topCardinfo = topCard(finals[final]);
                    if (topCardinfo != null) {
                        if (topCardinfo.Suit == cardinfo.Suit
                            && topCardinfo.Rank == cardinfo.Rank - 1
                            ) {
                                slot = final;
                            }
                        else if (topCardinfo.Suit < 2 != cardinfo.Suit < 2
                           && topCardinfo.Rank >= cardinfo.Rank - 1
                           ) {
                               count++;
                        }
                    }
                }
                if (slot >= 0 && count == 2) {
                    cardinfo.AutoMoveOffset = -new Vector2(cardSpots[slot + 4].X, cardSpots[slot + 4].Y);
                    finals[slot].Add(cardinfo);
                    return true;
                }
            }
            return false;
        }

        private bool AnalyzeForAntoMove()
        {
            System.Diagnostics.Debug.WriteLine("analyze for automove");
            for (int hold = 0; hold < 4; hold++)
            {
                CardInfo cardinfo = holds[hold];
                if (cardinfo != null && checkForAutoMove(cardinfo))
                {
                    holds[hold] = null;
                    cardinfo.AutoMoveOffset += new Vector2(cardSpots[hold].X, cardSpots[hold].Y);
                    cardinfo.AutoMoveInterpolation = 1;
                    cardinfo.AutoMoveTime = AutoMoveDuration;
                    System.Diagnostics.Debug.WriteLine("return true 1");
                    return true;
                }
            }

            for (int pile = 0; pile < 8; pile++)
            {
                CardInfo cardinfo = topCard(piles[pile]);
                if (cardinfo != null && checkForAutoMove(cardinfo))
                {
                    piles[pile].Remove(cardinfo);
                    cardinfo.AutoMoveOffset = new Vector2(cardSpots[pile + 8].X
                        , cardSpots[pile + 8].Y + piles[pile].Count * yOverlay
                        );
                    cardinfo.AutoMoveTime = AutoMoveDuration;
                    cardinfo.AutoMoveInterpolation = 1;
                    System.Diagnostics.Debug.WriteLine("return true 2");
                    return true;
                }
            }
            return false;
        }

        private bool tryPickUpCard(Vector2 position)
        {
            for (int hold = 0; hold < 4; hold++) {
                if (holds[hold] != null && isWithinRectangle(position, cardSpots[hold]))
                {
                    Point pt = cardSpots[hold].Location;
                    touchedCard = holds[hold];
                    touchedCardOrigin = holds;
                    touchedCardOriginIndex = hold;
                    touchedCardPosition = new Vector2(pt.X, pt.Y);
                    holds[hold] = null;
                    return true;
                }
            }

            for (int pile = 0; pile < 8; pile++) {
                if (piles[pile].Count() == 0)continue;

                Rectangle pileSpot = cardSpots[pile + 8];
                pileSpot.Offset(0,yOverlay*(piles[pile].Count-1));

                if (isWithinRectangle(position, pileSpot)) {
                    Point pt = pileSpot.Location;
                    int pileIndex = piles[pile].Count - 1;

                    touchedCard = piles[pile][pileIndex];
                    touchedCardOrigin = piles;
                    touchedCardOriginIndex = pile;
                    touchedCardPosition = new Vector2(pt.X,pt.Y);
                    piles[pile].RemoveAt(pileIndex);
                    return true;
                }
                
            }
                return false;
        }

        private bool tryPutDownCard(CardInfo touchedCard)
        {
            Vector2 cardCenter = new Vector2(touchedCardPosition.X+wCard/2
                  , touchedCardPosition.Y + hCard / 2
                );
            for (int cardspot = 0; cardspot < 16; cardspot++)
            {
                Rectangle rect = cardSpots[cardspot];
                if (cardspot >= 8)
                {
                    rect.Inflate(0, hSurface - rect.Bottom);
                }

                if (isWithinRectangle(cardCenter, rect))
                {

                    if (cardspot < 4)
                    {
                        int hold = cardspot;
                        if (holds[hold] == null)
                        {
                            holds[hold] = touchedCard;
                            return true;
                        }

                    }
                    else if (cardspot < 8)
                    {
                        int final = cardspot - 4;
                        if (topCard(finals[final]) == null)
                        {
                            if (touchedCard.Rank == 0)
                            {
                                finals[final].Add(touchedCard);
                                return true;
                            }

                        }
                        else if (touchedCard.Suit == topCard(finals[final]).Suit &&
                            touchedCard.Rank == topCard(finals[final]).Rank + 1
                           )
                        {
                            finals[final].Add(touchedCard);
                            return true;
                        }
                    }
                    else
                    {
                        int pile = cardspot - 8;

                        if (piles[pile].Count == 0)
                        {
                            piles[pile].Add(touchedCard);
                            return true;
                        }
                        else
                        {
                            CardInfo topcardp = topCard(piles[pile]);
                            if (touchedCard.Suit < 2 != topcardp.Suit < 2
                                && touchedCard.Rank == topcardp.Rank - 1
                                )
                            {
                                piles[pile].Add(touchedCard);
                                return true;
                            }
                        }

                    }
                    break;
                }
            }

            if (touchedCardOrigin is CardInfo[])
            {
                (touchedCardOrigin as CardInfo[])[touchedCardOriginIndex] = touchedCard;
            }
            else if(touchedCardOrigin is List<CardInfo>[])
            {
                (touchedCardOrigin as List<CardInfo>[])[touchedCardOriginIndex].Add(touchedCard);

            }
            return false;

        }

        private Rectangle GetCardTextureSource(CardInfo cardInfo)
        {
            return new Rectangle(wCard * cardInfo.Rank, hCard * cardInfo.Suit, wCard, hCard);
        }
    }
}

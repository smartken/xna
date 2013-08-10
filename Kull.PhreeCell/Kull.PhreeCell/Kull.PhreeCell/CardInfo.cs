using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Kull.PhreeCell
{
    class CardInfo
    {
        public readonly static string[] ranks ={"Ace","Deuce","Three","Four","Five","Six","Seven","Eight","Nine"
                              ,"Ten","Jack","Queen","King"
                              };

        public readonly static string[] suits = { "Spades", "Clubs", "Hearts", "Diamonds" };

        public int Suit { protected set; get; }
        public int Rank { protected set; get; }

        public Vector2 AutoMoveOffset { get; set; }
        public TimeSpan AutoMoveTime { get; set; }

        public float AutoMoveInterpolation { set; get; }

        public CardInfo(int suit, int rank)
        {
            Suit = suit;
            Rank = rank;

        }

        public override string ToString()
        {
            return ranks[Rank] + " of " + suits[Suit];
        }
    }
}

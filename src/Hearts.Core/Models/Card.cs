using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hearts.Models.Extensions;
using Hearts.Utility;

namespace Hearts.Models
{
    public struct Card : IEquatable<Card>, IComparable<Card>, IComparable
    {
        private readonly Suit _suit;
        private readonly Rank _rank;
        public Suit Suit { get { return _suit; } }
        public Rank Rank { get { return _rank; } }
        public Card(Suit suit, Rank rank)
        {
            _suit = suit;
            _rank = rank;
        }

        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Card left, Card right)
        {
            return !(left == right);
        }

        public int CompareTo(Card value)
        {
            int comp = Suit.CompareTo(value.Suit);
            if (comp != 0)
            {
                return comp;
            }
            return Rank.CompareTo(value.Rank);
        }

        public int CompareTo(object value)
        {
            if (value is Card)
            {
                return CompareTo((Card)value);
            }
            throw new ArgumentException
                ("value is not a Hearts.Card object");
        }

        public bool Equals(Card obj)
        {
            return Suit == obj.Suit
                && Rank == obj.Rank;
        }

        public override bool Equals(object obj)
        {
            if (obj is Card)
            {
                return Equals((Card)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (13 * (int)Suit) + (int)Rank;
        }

        internal static IList<Card> ParseHand(string s)
        {
            var suitMap = Utility.Enum.GetValues<Suit>()
                .ToDictionary(e => e.GetDescription());
            var rankMap = Utility.Enum.GetValues<Rank>()
                .ToDictionary(e => e.GetDescription());
            return s
                .Split('|')
                .Select(sym => new
                    {
                        Rank = sym.Substring(0, sym.Length - 1),
                        Suit = sym.Substring(sym.Length - 1)
                    })
                .Select(desc =>
                    new Card(suitMap[desc.Suit], rankMap[desc.Rank]))
                .ToList();
        }

        public override string ToString()
        {
            return string.Format("{0}{1}",
                Rank.GetDescription(),
                Suit.GetDescription());
        }

        public static readonly Card QueenOfSpades =
            new Card(Suit.Spades, Rank.Queen);
        public static readonly Card TwoOfClubs =
            new Card(Suit.Clubs, Rank.Two);
        private static readonly List<Card> _deck;
        public static IList<Card> Deck
        {
            get { return _deck.AsReadOnly(); }
        }

        static Card()
        {
            var suits = Utility.Enum.GetValues<Suit>();
            var ranks = Utility.Enum.GetValues<Rank>();
            _deck = suits
                .SelectMany(s => ranks
                    .Select(r => new Card(s, r)))
                .ToList();
        }
    }
}

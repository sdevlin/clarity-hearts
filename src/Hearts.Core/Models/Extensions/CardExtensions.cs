using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Utility;
using Hearts.Models;

namespace Hearts.Models.Extensions
{
    public static class CardExtensions
    {
        public static string AsFormattedString
            (this IEnumerable<Card> cards)
        {
            var strings = cards
                .OrderBy(c => c.Suit)
                .ThenBy(c => c.Rank)
                .Select(c => c.ToString());
            return string.Join(" ", strings);
        }

        internal static void DealTo
            (this IEnumerable<Card> cards, IList<PlayerBase> players)
        {
            if (players.SelectMany(p => p.CardList).Any() ||
                players.SelectMany(p => p.TrickList).Any())
            {
                throw new ArgumentException("players already have cards");
            }
            int i = 0;
            foreach (var card in cards.Shuffle())
            {
                players[i].CardList.Add(card);
                i += 1;
                i %= players.Count;
            }
        }

        public static IEnumerable<Card> OfSuit
            (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.Where(c => c.Suit == suit);
        }

        public static IEnumerable<Card> NotOfSuit
            (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.Where(c => c.Suit != suit);
        }

        public static bool Outranks(this Card card, Card value)
        {
            return card.Suit == value.Suit
                && card.Rank > value.Rank;
        }

        public static bool IsOutrankedBy(this Card card, Card value)
        {
            return value.Outranks(card);
        }

        public static IEnumerable<Card> Outranking
            (this IEnumerable<Card> cards, Card value)
        {
            return cards.Where(c => c.Outranks(value));
        }

        public static IEnumerable<Card> OutrankedBy
            (this IEnumerable<Card> cards, Card value)
        {
            return cards
                .Except(cards
                    .Outranking(value));
        }

        public static int Value(this Card card)
        {
            if (card.Suit == Suit.Hearts)
            {
                return 1;
            }
            if (card == Card.QueenOfSpades)
            {
                return 13;
            }
            return 0;
        }

        public static int Value(this IEnumerable<Card> cards)
        {
            return cards.Sum(c => c.Value());
        }

        public static bool IsPointCard(this Card card)
        {
            return card.Value() > 0;
        }

        public static bool HasPointCards(this IEnumerable<Card> cards)
        {
            return cards.Value() > 0;
        }
    }
}

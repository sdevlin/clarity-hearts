using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hearts.Views;

namespace Hearts.Models
{
    public abstract class PlayerBase
    {
        /// <summary>
        /// The player's name. Used for display.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Not ready yet.
        /// </summary>
        public virtual string Avatar
        {
            get { return null; }
        }
        
        internal Table Table { get; set; }
        internal bool IsSeated
        {
            get { return Table != null; }
        }

        internal AbsoluteLocation Location
        {
            get { return Table.GetAbsoluteLocation(this); }
        }

        /// <summary>
        /// The player's score. The game ends when any player's score
        /// meets or exceeds 100.
        /// </summary>
        public int Score { get; internal set; }

        internal List<Card> CardList { get; private set; }

        /// <summary>
        /// The player's current hand. Repopulated at the beginning of
        /// each deal.
        /// </summary>
        public IList<Card> Hand
        {
            get { return CardList.AsReadOnly(); }
        }

        internal List<Trick> TrickList { get; private set; }

        /// <summary>
        /// All tricks taken during the current deal. Cleared at the end
        /// of each deal.
        /// </summary>
        public IEnumerable<TrickView> Tricks
        {
            get
            {
                return TrickList.Select(t => new TrickView(t, this));
            }
        }

        protected PlayerBase()
        {
            CardList = new List<Card>();
            TrickList = new List<Trick>();
        }

        internal void AddTrick(Trick trick)
        {
            TrickList.Add(trick);
        }

        /// <summary>
        /// Get three cards to pass to an opposing player at the
        /// beginning of a deal. Called at the beginning of each deal
        /// after cards have been dealt. (Exception: not called when
        /// the passing mode is "Hold" (i.e. every fourth deal).)
        /// </summary>
        /// <param name="toPlayer">
        /// The player who will receive the cards.
        /// </param>
        public abstract IEnumerable<Card> GetCardsToPass
            (PlayerView toPlayer);

        /// <summary>
        /// Strictly informational. The results of the card passing phase
        /// the player is aware of. Called after all cards have been passed.
        /// </summary>
        /// <param name="cards">The cards received by the player.</param>
        /// <param name="fromPlayer">The player they came from.</param>
        public virtual void NotifyPassedCards
            (IEnumerable<Card> cards, PlayerView fromPlayer)
        { }

        /// <summary>
        /// Get a card to lead the next trick. Only called when the
        /// player has the lead.
        /// </summary>
        /// <param name="dealInProgress">The deal as it stands now.</param>
        public abstract Card GetLead(DealView dealInProgress);

        /// <summary>
        /// Get a card to play in the current trick. Only called when the
        /// player does not have the lead.
        /// </summary>
        /// <param name="dealInProgress">The deal as it stands now.</param>
        /// <param name="trickInProgress">The trick as it stands now.</param>
        public abstract Card GetPlay
            (DealView dealInProgress, TrickView trickInProgress);

        /// <summary>
        /// Strictly informational. The results of the previous trick.
        /// Called immediately after the trick ends.
        /// </summary>
        /// <param name="completedTrick">The trick in question.</param>
        public virtual void NotifyTrickResults(TrickView completedTrick)
        { }

        /// <summary>
        /// Strictly information. The results of the previous deal.
        /// Called immediately after the deal ends.
        /// </summary>
        /// <param name="completedDeal">The deal in question.</param>
        public virtual void NotifyDealResults(DealView completedDeal)
        { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Name: {0} \n", Name);
            sb.AppendFormat("Score: {0} \n", Score);
            sb.AppendLine("Cards: ");
            var cards = Hand
                .OrderBy(c => c.Suit)
                .ThenBy(c => c.Rank);
            foreach (var card in cards)
            {
                sb.AppendFormat("\t{0} \n", card);
            }
            sb.AppendLine("Tricks: ");
            foreach (var trick in Tricks)
            {
                sb.AppendFormat("\t{0} \n",
                    string.Join(", ", trick
                        .Cards
                        .Select(c => c.ToString())
                        .ToArray()));
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a deck of cards, managing the collection, shuffling, drawing, and adding of cards.
/// </summary>
public class Deck : IDeck
{
    #region Fields

    /// <summary>
    /// The list of cards currently in the deck.
    /// </summary>
    private List<ICard> cards;

    /// <summary>
    /// Random number generator used for shuffling the deck.
    /// </summary>
    private Random random = new Random();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Deck"/> class and populates it with cards.
    /// </summary>
    public Deck()
    {
        InitializeDeck();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes the deck with 52 standard cards and Jokers.
    /// </summary>
    private void InitializeDeck()
    {
        cards = new List<ICard>();

        // Add standard cards
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>())
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)).Cast<CardRank>().Where(r => r != CardRank.Joker))
            {
                // Generate a name for the card based on its rank and suit
                string cardName = $"{rank} of {suit}";
                cards.Add(new Card(cardName, suit, rank));
            }
        }

        // Add jokers with specific effects based on their suit
        cards.Add(new JokerCard(CardSuit.Hearts, JokerEffectType.HeartsEffect));
        cards.Add(new JokerCard(CardSuit.Diamonds, JokerEffectType.DiamondsEffect));

        //TODO: (See JokerCard.CS)
        //cards.Add(new JokerCard(CardSuit.Clubs, JokerEffectType.ClubsEffect));
        //cards.Add(new JokerCard(CardSuit.Spades, JokerEffectType.SpadesEffect));

        Shuffle();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Shuffles the deck by randomly reordering the cards.
    /// </summary>
    public void Shuffle()
    {
        cards = cards.OrderBy(card => random.Next()).ToList();
    }

    /// <summary>
    /// Draws the top card from the deck, removing it from the deck.
    /// </summary>
    /// <returns>The card drawn from the top of the deck, or null if the deck is empty.</returns>
    public ICard Draw()
    {
        if (cards.Count == 0) return null;
        ICard card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    /// <summary>
    /// Adds a card to the bottom of the deck.
    /// </summary>
    /// <param name="card">The card to add to the deck.</param>
    public void AddCard(ICard card)
    {
        cards.Add(card);
    }

    /// <summary>
    /// Removes a specific card from the deck.
    /// </summary>
    /// <param name="card">The card to remove from the deck.</param>
    public void RemoveCard(ICard card)
    {
        cards.Remove(card);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of cards currently in the deck.
    /// </summary>
    public int Count => cards.Count;

    #endregion
}
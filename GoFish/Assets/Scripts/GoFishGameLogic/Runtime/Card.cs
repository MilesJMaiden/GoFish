/// <summary>
/// Represents a card in a standard deck, including its name, suit, and rank.
/// </summary>
public class Card : ICard
{
    #region Properties

    /// <summary>
    /// Gets the name of the card.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the suit of the card.
    /// </summary>
    public CardSuit Suit { get; private set; }

    /// <summary>
    /// Gets the rank of the card.
    /// </summary>
    public CardRank Rank { get; private set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class with the specified name, suit, and rank.
    /// </summary>
    /// <param name="name">The name of the card.</param>
    /// <param name="suit">The suit of the card.</param>
    /// <param name="rank">The rank of the card.</param>
    public Card(string name, CardSuit suit, CardRank rank)
    {
        Name = name;
        Suit = suit;
        Rank = rank;
    }

    #endregion
}

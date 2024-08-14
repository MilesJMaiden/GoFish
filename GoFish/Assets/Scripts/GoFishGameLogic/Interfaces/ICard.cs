/// <summary>
/// Represents a card with a name, rank, and suit.
/// </summary>
public interface ICard
{
    #region Properties

    /// <summary>
    /// Gets the name of the card.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the rank of the card.
    /// </summary>
    CardRank Rank { get; }

    /// <summary>
    /// Gets the suit of the card.
    /// </summary>
    CardSuit Suit { get; }

    #endregion
}
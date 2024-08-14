/// <summary>
/// Represents a deck of cards with functionalities to shuffle, draw, add, and remove cards.
/// </summary>
public interface IDeck
{
    #region Methods

    /// <summary>
    /// Shuffles the deck.
    /// </summary>
    void Shuffle();

    /// <summary>
    /// Draws the top card from the deck.
    /// </summary>
    /// <returns>The drawn card.</returns>
    ICard Draw();

    /// <summary>
    /// Adds a card to the deck.
    /// </summary>
    /// <param name="card">The card to add.</param>
    void AddCard(ICard card);

    /// <summary>
    /// Removes a card from the deck.
    /// </summary>
    /// <param name="card">The card to remove.</param>
    void RemoveCard(ICard card);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of cards remaining in the deck.
    /// </summary>
    int Count { get; }

    #endregion
}
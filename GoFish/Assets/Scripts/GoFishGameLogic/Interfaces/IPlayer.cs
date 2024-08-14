using System.Collections.Generic;

/// <summary>
/// Represents a player in the game with functionalities to manage their hand and take turns.
/// </summary>
public interface IPlayer
{
    #region Properties

    /// <summary>
    /// Gets the name of the player.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the player's current hand of cards.
    /// </summary>
    List<ICard> Hand { get; }

    /// <summary>
    /// Gets or sets the player's current score.
    /// </summary>
    int Score { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Receives a card and adds it to the player's hand.
    /// </summary>
    /// <param name="card">The card to receive.</param>
    void ReceiveCard(ICard card);

    /// <summary>
    /// Checks if the player has any cards of the specified rank.
    /// </summary>
    /// <param name="cardRank">The rank of the card to check for.</param>
    /// <returns>True if the player has at least one card of the specified rank, otherwise false.</returns>
    bool HasCard(CardRank cardRank);

    /// <summary>
    /// Gives all cards of the specified rank from the player's hand.
    /// </summary>
    /// <param name="cardRank">The rank of the cards to give.</param>
    /// <returns>A list of cards of the specified rank.</returns>
    List<ICard> GiveAllCards(CardRank cardRank);

    /// <summary>
    /// Collects and removes books (sets of four cards of the same rank) from the player's hand.
    /// </summary>
    /// <returns>A list of collected books as strings.</returns>
    List<string> CollectBooks();

    /// <summary>
    /// Executes the player's turn, asking for cards from other players and drawing if necessary.
    /// </summary>
    /// <param name="players">The list of players in the game.</param>
    /// <param="deck">The deck of cards.</param>
    void TakeTurn(List<IPlayer> players, IDeck deck);

    /// <summary>
    /// Gets the rank of the card the player wants to ask for.
    /// </summary>
    /// <returns>The rank of the requested card.</returns>
    CardRank GetRequestedCardRank();

    #endregion
}
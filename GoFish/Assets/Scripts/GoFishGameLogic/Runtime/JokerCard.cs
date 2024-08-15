using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Joker card with specific effects in the game of Go Fish.
/// </summary>
public class JokerCard : ICard
{
    public CardRank Rank { get; private set; }
    public CardSuit Suit { get; private set; }
    public string Name => $"{Suit} Joker";

    public JokerCard(CardSuit suit, JokerEffectType effectType)
    {
        Rank = CardRank.Joker;
        Suit = suit;
        EffectType = effectType;
    }

    /// <summary>
    /// Gets or sets the effect type of the Joker card.
    /// </summary>
    public JokerEffectType EffectType { get; private set; }

    /// <summary>
    /// Executes the effect of the Joker card based on its suit.
    /// </summary>
    /// <param name="currentPlayer">The player who drew the Joker card.</param>
    /// <param name="targetPlayer">The player who made the current player Go Fish.</param>
    /// <param name="deck">The deck of cards.</param>
    public void ExecuteEffect(Player currentPlayer, Player targetPlayer, IDeck deck)
    {
        switch (EffectType)
        {
            case JokerEffectType.HeartsEffect:
                ExecuteHeartsEffect(currentPlayer, deck);
                break;

            case JokerEffectType.DiamondsEffect:
                ExecuteDiamondsEffect(currentPlayer, targetPlayer);
                break;

            default:
                Debug.LogError("Unknown Joker effect type.");
                break;
        }

        // Log the Joker execution and ensure it's removed from play.
        Debug.Log($"{Suit} Joker executed and removed from play.");
    }

    /// <summary>
    /// Executes the Hearts Joker effect, which returns all cards to the deck and redraws the same number.
    /// </summary>
    private void ExecuteHeartsEffect(Player currentPlayer, IDeck deck)
    {
        List<ICard> returnedCards = new List<ICard>(currentPlayer.Hand);
        currentPlayer.Hand.Clear();

        foreach (var card in returnedCards)
        {
            deck.AddCard(card);
        }

        deck.Shuffle();

        for (int i = 0; i < returnedCards.Count; i++)
        {
            ICard drawnCard = deck.Draw();
            currentPlayer.ReceiveCard(drawnCard);
        }

        Debug.Log($"{currentPlayer.Name} returned all cards to the deck and redrew due to Hearts Joker.");
    }

    /// <summary>
    /// Executes the Diamonds Joker effect, which swaps hands with the target player.
    /// </summary>
    private void ExecuteDiamondsEffect(Player currentPlayer, Player targetPlayer)
    {
        List<ICard> tempHand = new List<ICard>(currentPlayer.Hand);
        currentPlayer.Hand.Clear();
        currentPlayer.Hand.AddRange(targetPlayer.Hand);
        targetPlayer.Hand.Clear();
        targetPlayer.Hand.AddRange(tempHand);

        Debug.Log($"{currentPlayer.Name} and {targetPlayer.Name} swapped hands due to Diamonds Joker.");
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : IPlayer
{
    public string Name { get; private set; }
    public List<ICard> Hand { get; private set; }
    public int Score { get; set; }
    private System.Random random;

    public Player(string name)
    {
        Name = name;
        Hand = new List<ICard>();
        random = new System.Random();
    }

    /// <summary>
    /// Receives a card and adds it to the player's hand.
    /// </summary>
    /// <param name="card">The card to be received.</param>
    public void ReceiveCard(ICard card)
    {
        if (card is JokerCard)
        {
            Debug.LogError("Jokers should not be added to a player's hand.");
            return;
        }

        Hand.Add(card);
        Debug.Log($"{Name} received {card.Name}");
    }

    public bool HasCard(CardRank cardRank)
    {
        bool hasCard = Hand.Any(card => card.Rank == cardRank);
        Debug.Log($"{Name} hand contains: {string.Join(", ", Hand.Select(card => card.Name))}");
        Debug.Log($"{Name} checking for card rank {cardRank}: {hasCard}");
        return hasCard;
    }

    public List<ICard> GiveAllCards(CardRank cardRank)
    {
        List<ICard> cardsToGive = Hand.Where(card => card.Rank == cardRank).ToList();
        Hand.RemoveAll(card => card.Rank == cardRank);
        Debug.Log($"{Name} gave {cardsToGive.Count} cards of rank {cardRank}");
        return cardsToGive;
    }

    /// <summary>
    /// Collects all complete books (four of a kind) from the player's hand.
    /// </summary>
    /// <returns>A list of the collected books' ranks.</returns>
    public List<string> CollectBooks()
    {
        var books = new List<string>();
        var groupedByRank = Hand.GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 4)
                                .ToList();

        foreach (var group in groupedByRank)
        {
            books.Add(group.Key.ToString());
            foreach (var card in group)
            {
                Hand.Remove(card);
            }
        }

        return books;
    }

    public void TakeTurn(List<IPlayer> players, IDeck deck)
    {
        var targetPlayer = players.FirstOrDefault(p => p != this);
        if (Hand.Count == 0 || targetPlayer == null)
        {
            return; // If the AI has no cards or no target player, skip the turn
        }

        var requestedCardRank = Hand[random.Next(Hand.Count)].Rank;

        if (targetPlayer != null && requestedCardRank != default)
        {
            if (targetPlayer.HasCard(requestedCardRank))
            {
                var cards = targetPlayer.GiveAllCards(requestedCardRank);
                foreach (var card in cards)
                {
                    ReceiveCard(card);
                }
            }
            else
            {
                var drawnCard = deck.Draw();
                if (drawnCard != null)
                {
                    if (drawnCard is JokerCard jokerCard)
                    {
                        jokerCard.ExecuteEffect(this, targetPlayer as Player, deck);
                        Hand.Remove(drawnCard); // Remove Joker after its effect
                        Debug.Log($"Joker {jokerCard.Name} executed and removed from {Name}'s hand.");
                    }
                    else
                    {
                        ReceiveCard(drawnCard);

                        // Check if drawing the card completes a book
                        var books = CollectBooks();
                        foreach (var book in books)
                        {
                            Debug.Log($"{Name} collected a book of {book}");
                        }
                        Score += books.Count; // Increment score by the number of books collected
                    }
                }
            }
        }
    }

    public CardRank GetRequestedCardRank()
    {
        return Hand[random.Next(Hand.Count)].Rank;
    }
}
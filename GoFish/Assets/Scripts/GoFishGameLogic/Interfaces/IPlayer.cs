using System.Collections.Generic;

public interface IPlayer
{
    string Name { get; }
    List<ICard> Hand { get; }
    int Score { get; set; }

    void ReceiveCard(ICard card);
    bool HasCard(CardRank cardRank);
    List<ICard> GiveAllCards(CardRank cardRank);
    List<string> CollectBooks();
    void TakeTurn(List<IPlayer> players, IDeck deck);
    CardRank GetRequestedCardRank();
}

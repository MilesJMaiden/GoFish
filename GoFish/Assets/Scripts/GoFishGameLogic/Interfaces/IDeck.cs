public interface IDeck
{
    void Shuffle();
    ICard Draw();
    void AddCard(ICard card);
    int Count { get; }
    void RemoveCard(ICard card);
}

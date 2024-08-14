public interface ICard
{
    string Name { get; }
    CardRank Rank { get; }
    CardSuit Suit { get; }
}

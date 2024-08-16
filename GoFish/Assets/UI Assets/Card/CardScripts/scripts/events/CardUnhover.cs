namespace events {
    /*
     * Core mechanics based on https://github.com/lelexy100/unity-card-play \
     */
    public class CardUnhover : CardEvent {
        public CardUnhover(CardWrapper card) : base(card) {
        }
    }
}

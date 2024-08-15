using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the game flow, including initializing players, dealing cards, processing turns,
/// updating the UI, and handling game state transitions for a Go Fish game.
/// </summary>
public class GameManager : MonoBehaviour
{
    //public TextMeshProUGUI playerHandText;
    public TextMeshProUGUI playerScoreText;
    //public TextMeshProUGUI[] aiHandTexts;
    public TextMeshProUGUI[] aiScoreTexts;
    public TextMeshProUGUI deckText;
    public TextMeshProUGUI messageText;
    public GameObject cardButtonPrefab;
    public Transform cardButtonParent;
    public GameObject playerSelectionButtonPrefab;
    public Transform playerSelectionButtonParent;
    public TextMeshProUGUI goFishText;
    public Button aiPlayerButton1, aiPlayerButton2, aiPlayerButton3;
    public GameObject cardBackSpritePrefab;
    public Transform[] aiPlayerCardParents;

    public TextMeshProUGUI aiPlayerSelectionMessageText; // New field for AI player selection message
    public TextMeshProUGUI gameEndMessageText; // New field for game end message

    private const int InitialHandSize = 5;
    private List<IPlayer> players;
    private IDeck deck;
    private int currentPlayerIndex;
    private System.Random random;
    private Dictionary<IPlayer, List<string>> playerBooks;
    private IPlayer selectedPlayer;
    private CardRank requestedCardRank;
    private int numberOfAIPlayers;

    /// <summary>
    /// Initializes the game by setting up the players, deck, and UI elements.
    /// </summary>
    void Start()
    {
        players = new List<IPlayer>();
        deck = new Deck();
        random = new System.Random();
        playerBooks = new Dictionary<IPlayer, List<string>>();

        ShowAIPlayerSelection();
    }

    /// <summary>
    /// Displays the AI player selection buttons to the user.
    /// </summary>
    private void ShowAIPlayerSelection()
    {
        Debug.Log("Showing AI player selection buttons.");
        aiPlayerSelectionMessageText.gameObject.SetActive(true);
        aiPlayerSelectionMessageText.text = "Please select the number of AI opponents:";
        aiPlayerButton1.gameObject.SetActive(true);
        aiPlayerButton2.gameObject.SetActive(true);
        aiPlayerButton3.gameObject.SetActive(true);
        deckText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
        goFishText.gameObject.SetActive(false);
        cardButtonParent.gameObject.SetActive(false);
        playerSelectionButtonParent.gameObject.SetActive(false);

        //playerHandText.gameObject.SetActive(false);
        playerScoreText.gameObject.SetActive(false);
        //foreach (var aiHandText in aiHandTexts)
        //{
        //    aiHandText.gameObject.SetActive(false);
        //}
        foreach (var aiScoreText in aiScoreTexts)
        {
            aiScoreText.gameObject.SetActive(false);
        }
        gameEndMessageText.gameObject.SetActive(false);
    }


    /// <summary>
    /// Called when an AI player selection is made.
    /// </summary>
    /// <param name="numAIPlayers">The number of AI players selected.</param>
    public void OnAIPlayerSelected(int numAIPlayers)
    {
        Debug.Log("AI players selected: " + numAIPlayers);
        numberOfAIPlayers = numAIPlayers;
        aiPlayerSelectionMessageText.gameObject.SetActive(false);
        aiPlayerButton1.gameObject.SetActive(false);
        aiPlayerButton2.gameObject.SetActive(false);
        aiPlayerButton3.gameObject.SetActive(false);
        deckText.gameObject.SetActive(true);
        messageText.gameObject.SetActive(true);
        cardButtonParent.gameObject.SetActive(true);
        playerSelectionButtonParent.gameObject.SetActive(false);

        //playerHandText.gameObject.SetActive(true);
        playerScoreText.gameObject.SetActive(true);
        for (int i = 0; i < numberOfAIPlayers; i++)
        {
            //aiHandTexts[i].gameObject.SetActive(true);
            aiScoreTexts[i].gameObject.SetActive(true);
        }

        InitializePlayers(numberOfAIPlayers);
        StartGame();
    }


    /// <summary>
    /// Initializes the players in the game.
    /// </summary>
    /// <param name="numberOfAIPlayers">The number of AI players to initialize.</param>
    private void InitializePlayers(int numberOfAIPlayers)
    {
        Debug.Log("Initializing players.");
        players.Add(new Player("Human Player"));

        for (int i = 0; i < numberOfAIPlayers; i++)
        {
            players.Add(new Player($"AI Player {i + 1}"));
        }
    }

    /// <summary>
    /// Starts the game by shuffling the deck, dealing cards, adding jokers to the deck, and initiating the game loop.
    /// </summary>
    private void StartGame()
    {
        Debug.Log("Starting game.");
        deck.Shuffle();
        DealCards(); // Deal initial hands without Jokers
        foreach (var player in players)
        {
            playerBooks[player] = new List<string>();
        }
        currentPlayerIndex = random.Next(players.Count);
        UpdateUI();
        CreateCardButtons(players[0]); // Create card buttons for human player
        PlayGame();
    }

    /// <summary>
    /// Deals cards to all players.
    /// </summary>
    public void DealCards()
    {
        Debug.Log("Dealing cards to players.");
        foreach (var player in players)
        {
            for (int i = 0; i < InitialHandSize; i++)
            {
                ICard card = deck.Draw();
                if (card == null)
                {
                    Debug.LogError("Drawn card is null. Check the deck initialization.");
                    continue;
                }
                player.ReceiveCard(card);
            }
        }
    }

    /// <summary>
    /// Begins the game loop.
    /// </summary>
    private void PlayGame()
    {
        Debug.Log("Playing game.");
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// Main game loop that manages player turns.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator GameLoop()
    {
        while (!IsGameOver())
        {
            var currentPlayer = players[currentPlayerIndex];
            if (currentPlayer.Hand.Count == 0)
            {
                Debug.Log($"{currentPlayer.Name} has no cards left.");
                if (deck.Count > 0)
                {
                    ICard card = deck.Draw();
                    if (card is JokerCard)
                    {
                        ((JokerCard)card).ExecuteEffect(currentPlayer as Player, null, deck);
                        continue; // Skip to the next iteration since a Joker was drawn and handled
                    }
                    currentPlayer.ReceiveCard(card);
                    UpdateUI();
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    Debug.Log("Deck is empty. Skipping turn.");
                    currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                    continue;
                }
            }

            if (currentPlayer.Name == "Human Player")
            {
                CreatePlayerSelectionButtons();
                messageText.text = "Select a player to ask.";
                playerSelectionButtonParent.gameObject.SetActive(true);
                Debug.Log("Waiting for player to select another player.");
                yield return new WaitUntil(() => selectedPlayer != null);

                EnableCardButtons(true);
                messageText.text = "Select a card to ask for.";
                Debug.Log("Waiting for player to select a card.");
                yield return new WaitUntil(() => requestedCardRank != default);

                EnableCardButtons(false);
                DestroyPlayerSelectionButtons();
                playerSelectionButtonParent.gameObject.SetActive(false);

                ProcessTurn(currentPlayer, selectedPlayer, requestedCardRank);
                requestedCardRank = default;
                selectedPlayer = null;
            }
            else
            {
                LogAIAction(currentPlayer);
                yield return new WaitForSeconds(2);
                currentPlayer.TakeTurn(players, deck);
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            UpdateUI();

            // Check if the game is over after each turn
            if (IsGameOver())
            {
                AnnounceWinner();
                yield break; // Exit the game loop
            }
        }
    }

    /// <summary>
    /// Logs the action of the AI player and ensures AI requests cards from different players.
    /// </summary>
    /// <param name="currentPlayer">The AI player taking the action.</param>
    private void LogAIAction(IPlayer currentPlayer)
    {
        var aiPlayer = currentPlayer as Player;
        if (aiPlayer != null)
        {
            var targetPlayers = players.Where(p => p != currentPlayer && p.Hand.Count > 0).ToList();
            if (targetPlayers.Count == 0)
            {
                Debug.LogError("No valid target players available.");
                return;
            }
            var targetPlayer = targetPlayers[random.Next(targetPlayers.Count)];
            var requestedCardRank = aiPlayer.GetRequestedCardRank();
            Debug.Log($"{aiPlayer.Name} is requesting {requestedCardRank} from {targetPlayer.Name}");

            ProcessTurn(aiPlayer, targetPlayer, requestedCardRank);
        }
    }

    /// <summary>
    /// Announces the winner of the game, displays player rankings, and starts the process to return to AI selection screen.
    /// </summary>
    private void AnnounceWinner()
    {
        var rankedPlayers = players.OrderByDescending(player => player.Score).ToList();
        gameEndMessageText.gameObject.SetActive(true); // Show game end message
        gameEndMessageText.text = "Game Over! Player Rankings:\n";
        for (int i = 0; i < rankedPlayers.Count; i++)
        {
            gameEndMessageText.text += $"{i + 1}. {rankedPlayers[i].Name}: {rankedPlayers[i].Score} books\n";
        }
        Debug.Log("Game Over! Player Rankings:");
        foreach (var player in rankedPlayers)
        {
            Debug.Log($"{player.Name}: {player.Score} books");
        }

        // Delay for 5 seconds and then reset the game
        Invoke("ResetGame", 5f);
    }

    /// <summary>
    /// Checks if the game is over by verifying if all books are collected or if only jokers are left.
    /// </summary>
    /// <returns>True if the game is over, false otherwise.</returns>
    private bool IsGameOver()
    {
        // Check if all 13 books have been collected
        int totalBooks = playerBooks.Values.Sum(books => books.Count);
        if (totalBooks == 13)
        {
            return true;
        }

        // Check if all players' hands and the deck are empty
        if (players.All(player => player.Hand.Count == 0) && deck.Count == 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resets the game to the initial state, ready for a new game.
    /// </summary>
    private void ResetGame()
    {
        Debug.Log("Resetting game.");
        players.Clear();
        deck = new Deck();
        playerBooks.Clear();
        currentPlayerIndex = 0;
        selectedPlayer = null;
        requestedCardRank = default(CardRank);

        // Reset UI elements
        gameEndMessageText.gameObject.SetActive(false);
        //playerHandText.gameObject.SetActive(false);
        playerScoreText.gameObject.SetActive(false);
        //foreach (var aiHandText in aiHandTexts)
        //{
        //    aiHandText.gameObject.SetActive(false);
        //}
        foreach (var aiScoreText in aiScoreTexts)
        {
            aiScoreText.gameObject.SetActive(false);
        }
        deckText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
        goFishText.gameObject.SetActive(false);
        cardButtonParent.gameObject.SetActive(false);
        playerSelectionButtonParent.gameObject.SetActive(false);

        // Show the AI player selection buttons again
        ShowAIPlayerSelection();
    }

    /// <summary>
    /// Creates buttons for selecting which player to ask for a card.
    /// </summary>
    private void CreatePlayerSelectionButtons()
    {
        Debug.Log("Creating player selection buttons.");
        DestroyPlayerSelectionButtons(); // Ensure all previous buttons are destroyed first

        foreach (var player in players.Where(p => p != players[0])) // Skip the human player
        {
            var button = Instantiate(playerSelectionButtonPrefab, playerSelectionButtonParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = player.Name;
            button.GetComponent<Button>().onClick.AddListener(() => OnPlayerButtonClicked(player));
        }
    }

    /// <summary>
    /// Destroys all player selection buttons.
    /// </summary>
    private void DestroyPlayerSelectionButtons()
    {
        Debug.Log("Destroying all player selection buttons.");
        foreach (Transform child in playerSelectionButtonParent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Called when a player selection button is clicked.
    /// </summary>
    /// <param name="player">The player that was selected.</param>
    private void OnPlayerButtonClicked(IPlayer player)
    {
        Debug.Log("Player button clicked: " + player.Name);
        selectedPlayer = player;
    }

    /// <summary>
    /// Enables or disables the card buttons.
    /// </summary>
    /// <param name="enable">True to enable the buttons, false to disable them.</param>
    private void EnableCardButtons(bool enable)
    {
        Debug.Log("Enabling card buttons: " + enable);
        foreach (Transform cardButton in cardButtonParent)
        {
            cardButton.GetComponent<Button>().interactable = enable;
        }
    }

    /// <summary>
    /// Called when a card button is clicked.
    /// </summary>
    /// <param name="cardIndex">The index of the card that was clicked.</param>
    private void OnCardButtonClicked(int cardIndex)
    {
        requestedCardRank = players[0].Hand[cardIndex].Rank; // Human player's hand is at index 0
        Debug.Log("Card button clicked: " + requestedCardRank);
    }

    /// <summary>
    /// Processes a turn where one player asks another for a card, handles Go Fish scenario, and checks for game end condition.
    /// </summary>
    /// <param name="currentPlayer">The player taking the turn.</param>
    /// <param name="nextPlayer">The player being asked for a card.</param>
    /// <param name="requestedCardRank">The rank of the card being requested.</param>
    private void ProcessTurn(IPlayer currentPlayer, IPlayer nextPlayer, CardRank requestedCardRank)
    {
        Debug.Log($"{currentPlayer.Name} is requesting '{requestedCardRank}' from {nextPlayer.Name}");
        Debug.Log($"{nextPlayer.Name} hand before request: {string.Join(", ", nextPlayer.Hand.Select(card => card.Name))}");

        if (nextPlayer.HasCard(requestedCardRank))
        {
            Debug.Log($"{nextPlayer.Name} has the card rank '{requestedCardRank}'");
            var cards = nextPlayer.GiveAllCards(requestedCardRank);
            foreach (var card in cards)
            {
                currentPlayer.ReceiveCard(card);
                Debug.Log($"{nextPlayer.Name} gave '{requestedCardRank}' to {currentPlayer.Name}");
            }
            messageText.text = $"{nextPlayer.Name} gave {requestedCardRank} to {currentPlayer.Name}";
        }
        else
        {
            Debug.Log($"{nextPlayer.Name} does not have '{requestedCardRank}'. Go Fish!");
            StartCoroutine(ShowGoFishAndDrawCard(currentPlayer, nextPlayer, requestedCardRank));
        }

        Debug.Log($"{nextPlayer.Name} hand after request: {string.Join(", ", nextPlayer.Hand.Select(card => card.Name))}");
        Debug.Log($"{currentPlayer.Name} hand after receiving cards: {string.Join(", ", currentPlayer.Hand.Select(card => card.Name))}");

        var books = ((Player)currentPlayer).CollectBooks();
        playerBooks[currentPlayer].AddRange(books);

        if (books.Count > 0)
        {
            Debug.Log($"{currentPlayer.Name} collected books: {string.Join(", ", books)}");
            messageText.text = $"{currentPlayer.Name} collected books: {string.Join(", ", books)}";
            ((Player)currentPlayer).Score += books.Count; // Increment score by the number of books collected
        }

        // Update the UI after each turn to ensure scores are reflected immediately
        UpdateUI();

        // Check if the game is over after processing a turn
        if (IsGameOver())
        {
            AnnounceWinner();
            StopAllCoroutines(); // Stop the game loop
        }
    }

    /// <summary>
    /// Shows the "Go Fish" text, allows the current player to draw a card from the deck,
    /// handles joker card effects, and updates the UI accordingly.
    /// If the drawn card matches the requested rank, the player gets another turn.
    /// </summary>
    /// <param name="currentPlayer">The player taking the turn.</param>
    /// <param name="nextPlayer">The player being asked for a card.</param>
    /// <param name="requestedCardRank">The rank of the card that was requested.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator ShowGoFishAndDrawCard(IPlayer currentPlayer, IPlayer nextPlayer, CardRank requestedCardRank)
    {
        goFishText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        goFishText.gameObject.SetActive(false);

        var drawnCard = deck.Draw();
        if (drawnCard != null)
        {
            if (drawnCard is JokerCard jokerCard)
            {
                jokerCard.ExecuteEffect(currentPlayer as Player, nextPlayer as Player, deck);
                Debug.Log($"Joker {jokerCard.Rank} executed and removed from play.");
            }
            else
            {
                currentPlayer.ReceiveCard(drawnCard);

                // Check if drawing the card completes a book
                var books = ((Player)currentPlayer).CollectBooks();
                playerBooks[currentPlayer].AddRange(books);

                if (books.Count > 0)
                {
                    Debug.Log($"{currentPlayer.Name} collected books: {string.Join(", ", books)}");
                    messageText.text = $"{currentPlayer.Name} collected books: {string.Join(", ", books)}";
                    ((Player)currentPlayer).Score += books.Count; // Increment score by the number of books collected
                }

                // If the drawn card matches the requested rank, the player takes another turn
                if (drawnCard.Rank == requestedCardRank)
                {
                    messageText.text = $"{currentPlayer.Name} drew the requested card {requestedCardRank} and gets another turn!";
                    yield break;
                }
            }
        }

        // Update the UI after drawing a card
        UpdateUI();

        // Check if the game is over after drawing a card
        if (IsGameOver())
        {
            AnnounceWinner();
            StopAllCoroutines(); // Stop the game loop
        }
    }

    /// <summary>
    /// Updates the UI to reflect the current state of the game.
    /// </summary>
    private void UpdateUI()
    {
        Debug.Log("Updating UI.");
        //playerHandText.text = $"Human Player: {string.Join(", ", players[0].Hand.Select(card => card.Name))}";
        playerScoreText.text = $"Human Player Score: {players[0].Score}";
        CreateCardButtons(players[0]); // Refresh card buttons for human player
        for (int i = 0; i < numberOfAIPlayers; i++)
        {
            //aiHandTexts[i].text = $"{players[i + 1].Name}: {players[i + 1].Hand.Count} cards"; // Show number of cards instead of card names for AI
            aiScoreTexts[i].text = $"{players[i + 1].Name} Score: {players[i + 1].Score}";
            UpdateAICardDisplay(players[i + 1], aiPlayerCardParents[i]);
        }
        deckText.text = $"Deck: {deck.Count} cards remaining"; // Use the Count property
    }

    /// <summary>
    /// Creates buttons for each card in the human player's hand.
    /// </summary>
    /// <param name="player">The player whose cards are being displayed.</param>
    private void CreateCardButtons(IPlayer player)
    {
        Debug.Log("Creating card buttons for human player's hand");
        foreach (Transform child in cardButtonParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player.Hand.Count; i++)
        {
            var cardButton = Instantiate(cardButtonPrefab, cardButtonParent);
            var cardNameText = cardButton.GetComponentInChildren<TextMeshProUGUI>();
            cardNameText.text = player.Hand[i].Name;
            int index = i; // Prevent closure issue
            cardButton.GetComponent<Button>().onClick.AddListener(() => OnCardButtonClicked(index));
        }

        // Disable card buttons initially until a player is selected
        EnableCardButtons(false);
    }

    /// <summary>
    /// Updates the display for the AI players' cards with card back sprites.
    /// </summary>
    /// <param name="player">The AI player whose cards are being updated.</param>
    /// <param name="parent">The parent transform where card back sprites are added.</param>
    private void UpdateAICardDisplay(IPlayer player, Transform parent)
    {
        Debug.Log("Updating AI card display for: " + player.Name);
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player.Hand.Count; i++)
        {
            Instantiate(cardBackSpritePrefab, parent);
        }
    }
}

using UnityEngine;

// Class responsible for Managing turns between each Character
public class TurnManager : MonoBehaviour
{
    // References to the two players/characters in the game
    public S_PlayerController player1;
    public S_PlayerController player2;

    // Keeps track of the current Player 
    private S_PlayerController _currentPlayer;

    // Duration of each turn in seconds
    [SerializeField] private float _turnTime = 3f;
    private float _timer = 0f;

    // Round System
    private int _currentRound;
    private bool _isPlayer1Turn;


    private void Start()
    {
        _currentPlayer = player1;
        HandGestureManager.Instance.SetCurrentPlayer(_currentPlayer);

        _timer = _turnTime;

        // Notify the current player that their turn has started
        _currentPlayer.StartTurn();
        
        // SYNC UI STUFF
        UIManager.Instance.SetRound(_currentRound);
        UIManager.Instance.SetRoundCountdown(_timer);
        UIManager.Instance.SetPlayerNames(player1.playerName, player2.playerName);
        UIManager.Instance.SetMaxHealth(player1.maxHealth);
        UIManager.Instance.SetHealth(player1.playerName, player1.currentHealth);
        UIManager.Instance.SetHealth(player2.playerName, player2.currentHealth);
        UIManager.Instance.SetHighlightedPlayer(_currentPlayer.playerName);

        Debug.Log($"--- Round {_currentRound} ---");
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        UIManager.Instance.SetRoundCountdown(_timer);

        // Check if the current player's turn has ended
        if (_timer <= 0f)
        {
            SwitchTurn();
        }
    }

    private void SwitchTurn()
    {
        // End the current player's turn logic
        _currentPlayer.EndTurn();

        HandGestureManager.Instance.SetCurrentPlayer(_currentPlayer);

        // clear old gestures/UI
         _currentPlayer.ClearGestures();
        UIManager.Instance.ClearGestureIcons(_currentPlayer.playerName);

        // Switch to the other player and update current round 
        if (_currentPlayer == player1)
        {
            _currentPlayer = player2;
            _isPlayer1Turn = false;
        }
        else
        {
            // One full round completed (Player1 and Player2 both had a turn)
            _currentPlayer = player1;
            _isPlayer1Turn = true;
            _currentRound++;

            // update the UI 
            UIManager.Instance.SetRound(_currentRound);

            Debug.Log($"--- Round {_currentRound} ---");
        }

        UIManager.Instance.SetHighlightedPlayer(_currentPlayer.playerName);
        HandGestureManager.Instance.SetCurrentPlayer(_currentPlayer);

        // Reset the timer and start the new turn
        _timer = _turnTime;
        _currentPlayer.StartTurn();
    }
}

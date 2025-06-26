using UnityEngine;

// Class responsible for Managing turns between each Character
public class TurnManager : MonoBehaviour
{
    // References to the two players/characters in the game
    public PlayerController player1;
    public PlayerController player2;

    // Keeps track of the current Player 
    private PlayerController _currentPlayer;

    // Duration of each turn in seconds
    [SerializeField] private float _turnTime = 3f;
    private float _timer = 0f;

    // Round System
    private int _currentRound;
    private bool _isPlayer1Turn;


    private void Start()
    {
        _currentPlayer = player1;
        _timer = _turnTime;

        // Notify the current player that their turn has started
        _currentPlayer.StartTurn();

        
        UIManager.Instance.SetRound(_currentRound);
        UIManager.Instance.SetPlayerNames(player1.playerName, player2.playerName);
        UIManager.Instance.SetMaxHealth(player1.maxHealth);
        UIManager.Instance.SetHealth(player1.playerName, player1.currentHealth);
        UIManager.Instance.SetHealth(player2.playerName, player2.currentHealth);

        Debug.Log($"--- Round {_currentRound} ---");
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

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

        // Reset the timer and start the new turn
        _timer = _turnTime;
        _currentPlayer.StartTurn();
    }
}

using UnityEngine;

// Class responsible for Managing turns between each Character
public class TurnManager : MonoBehaviour
{
    // References to the two players/characters in the game
    public Player player1;
    public Player player2;

    // Keeps track of the current Player 
    private Player _currentPlayer;

    // Duration of each turn in seconds
    [SerializeField] private float _turnTime = 3f;
    private float _timer = 0f;

    private void Start()
    {
        _currentPlayer = player1;
        _timer = _turnTime;

        // Notify the current player that their turn has started
        _currentPlayer.StartTurn();

    }

    void Update()
    {
        _timer -= Time.deltaTime;

        // Check if the current player's turn has ended
        if (_timer <= 0f)
        {
            SwitchTurn();
        }
    }

    void SwitchTurn()
    {
        // End the current player's turn logic
        _currentPlayer.EndTurn();

        // Switch to the other player
        _currentPlayer = (_currentPlayer == player1) ? player2 : player1;

        // Reset the timer and start the new turn
        _timer = _turnTime;
        _currentPlayer.StartTurn();
    }
}

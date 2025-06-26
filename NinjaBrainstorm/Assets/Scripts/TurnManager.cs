using System.Threading;
using UnityEngine;

public enum Player { P1, P2 };

// Class responsible for Managing turns between each Character
public class TurnManager : MonoBehaviour
{
    // get the player
    public Player currentPlayer = Player.P1;


    [SerializeField] private float _turnTimer = 3f;
    private float _timer = 0f;

    private void Start()
    {
        _timer = _turnTimer;
        BeginTurn();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SwitchTurn();
        }
    }

    private void BeginTurn()
    {
        Debug.Log("Current Turn to: " + currentPlayer);
        _timer = _turnTimer;
    }

    private void SwitchTurn()
    {
        if (currentPlayer == Player.P1)
        {
            currentPlayer = Player.P2;
        } else
        {
            currentPlayer = Player.P1;
        }

        BeginTurn();
    }
}

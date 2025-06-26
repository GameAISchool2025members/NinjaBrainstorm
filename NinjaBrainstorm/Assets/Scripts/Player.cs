using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;

    public void StartTurn()
    {
        Debug.Log($"{playerName}'s turn started!");
        // Enable input, highlight player, etc.
    }

    public void EndTurn()
    {
        Debug.Log($"{playerName}'s turn ended.");
        // Disable input, etc.
    }
}

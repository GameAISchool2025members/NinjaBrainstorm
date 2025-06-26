using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public float  maxHealth = 100f;
    public float  currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

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

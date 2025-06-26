using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public float  maxHealth = 100f;
    public float  currentHealth;

    [SerializeField] bool DEBUG_takeHealthDamage = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (DEBUG_takeHealthDamage)
        {
            DEBUG_takeHealthDamage = false;
            TakeHealthDamage(5);
            UIManager.Instance.SetHealth(playerName, currentHealth);
        }
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

    public void TakeHealthDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI roundCountdownText;

    [SerializeField] private TextMeshProUGUI P1_Name;
    [SerializeField] private TextMeshProUGUI P2_Name;

    [SerializeField] private Slider P1_HealthBar;
    [SerializeField] private Slider P2_HealthBar;


    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }


    public void SetRound(int round)
    {
        roundText.text = $"Round {round}";
    }

    public void SetPlayerNames(string name1, string name2)
    {
        P1_Name.text = name1;
        P2_Name.text = name2;
    }
    public void SetHealth(string playerName, float healthVal)
    {
        if (playerName == P1_Name.text)
            P1_HealthBar.value = healthVal;
        else if (playerName == P2_Name.text)
            P2_HealthBar.value = healthVal;
    }

    public void SetMaxHealth(float maxHealth)
    {
        P1_HealthBar.maxValue = maxHealth;
        P2_HealthBar.maxValue = maxHealth;
    }

    public void SetRoundCountdown(float seconds)
    {
        roundCountdownText.text = $"{seconds:F1}s";
    }

}

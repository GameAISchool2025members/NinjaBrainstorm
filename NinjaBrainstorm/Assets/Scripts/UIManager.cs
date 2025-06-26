using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI P1_Name;
    [SerializeField] private TextMeshProUGUI P2_Name;



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

    // For later: Add methods for health, stats, active player indicator, etc.
}

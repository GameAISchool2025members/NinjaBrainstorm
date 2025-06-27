using System.Collections.Generic;
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

    [SerializeField] private Transform P1_GesturePanel;
    [SerializeField] private Transform P2_GesturePanel;

    [SerializeField] private GameObject gestureIconPrefab;
    [SerializeField] private List<HandGestureIcon> gestureIcons;
    private const int maxGestureIcons = 5;

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

    public void SetHighlightedPlayer(string playerName)
    {
        if (playerName == P1_Name.text)
        {
            P1_Name.color = Color.yellow; 
            P2_Name.color = Color.black;    
        }
        else
        {
            P1_Name.color = Color.black;
            P2_Name.color = Color.yellow;
        }
    }

    public void AddGestureIcon(string playerName, HandGesture gesture)
    {
        Transform panel = (playerName == P1_Name.text) ? P1_GesturePanel : P2_GesturePanel;

        if (panel.childCount >= maxGestureIcons)
        {
            Destroy(panel.GetChild(0).gameObject); // Remove oldest
        }

        GameObject iconGO = Instantiate(gestureIconPrefab, panel);
        Image image = iconGO.GetComponent<Image>();
        image.sprite = GetSpriteForGesture(gesture);
        image.color = Color.white;

        Debug.Log($"[UI] {playerName} → spawned icon for {gesture}: {image.sprite?.name ?? "NULL"}");

    }


    public void ClearGestureIcons(string playerName)
    {
        Transform panel = (playerName == P1_Name.text) ? P1_GesturePanel : P2_GesturePanel;
        // destroy all children
        foreach (Transform child in panel)
            Destroy(child.gameObject);
        Debug.Log($"Cleared UI icons for {playerName}");
    }


    private Sprite GetSpriteForGesture(HandGesture gesture)
    {
        for (int i = 0; i < gestureIcons.Count; i++)
        {
            HandGestureIcon entry = gestureIcons[i];

            if (entry.gesture == gesture)
            {
                Sprite icon = entry.icon;
                if (icon != null)
                {
                    return icon;
                }

                Debug.LogWarning($"Gesture '{gesture}' has no icon assigned.");
                return null;
            }
        }

        Debug.LogWarning($"Gesture '{gesture}' not found in gestureIcons list.");
        return null;
    }

}

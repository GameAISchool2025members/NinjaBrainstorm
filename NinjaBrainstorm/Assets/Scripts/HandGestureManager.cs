using UnityEngine;

public class HandGestureManager : MonoBehaviour
{
    public static HandGestureManager Instance;

    [SerializeField] private S_PlayerController player1;
    [SerializeField] private S_PlayerController player2;
    private S_PlayerController _currentPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void SetCurrentPlayer(S_PlayerController player)
    {
        _currentPlayer = player;
    }

    public void DetectAndRegisterGesture(HandGesture gesture)
    {
        if (_currentPlayer != null)
        {
           _currentPlayer.RegisterGesture(gesture);
        }
    }

    // THIS FOR DEBUGGING
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) DetectAndRegisterGesture(HandGesture.Attack);
        if (Input.GetKeyDown(KeyCode.S)) DetectAndRegisterGesture(HandGesture.Block);
    }
}

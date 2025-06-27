using UnityEngine;

public class HandGestureManager : MonoBehaviour
{
    public static HandGestureManager Instance;

    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    private PlayerController _currentPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void SetCurrentPlayer(PlayerController player)
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

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Animator))]
public class S_PlayerController : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    AnimationClip[] animClips;
    [SerializeField]
    List<string> playerSequence = new List<string>();
    
    public List<GameObject> elementsEffects = new List<GameObject>();

    bool isHit = false;
    int hitCharge = 1;
    float damageToTake = 0f;

    public int HitCharge { get => hitCharge; set => hitCharge = value; }
    public bool IsHit { get => isHit; set => isHit = value; }
    public Animator Animator => animator;
    public AnimationClip[] AnimClips => animClips;
    public  List<string> PlayerSequence
    {
        get => playerSequence;

        set => playerSequence = value;
    }

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

    public void CalculateDamageAmount(int myAngle, int vsAngle)
    {
        float angleDif = (Mathf.Floor(Mathf.Abs(vsAngle - myAngle)*1.12f))*0.01f;
        float relativeDamage = Mathf.Lerp(0,1, angleDif );
        
        damageToTake = (1+relativeDamage)*hitCharge;
    }

    public void TakeHealthDamage()
    {
        currentHealth -= damageToTake;
        currentHealth = Mathf.Max(currentHealth, 0);
    }
}

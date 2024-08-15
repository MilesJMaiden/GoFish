using UnityEngine;

public class FishingRodInteraction : MonoBehaviour
{
    public enum State { Idle, Casting, Catching, Reeling }
    private State currentState = State.Idle;

    [Header("Idle")]
    public Animator rodAnimator;
    public GameObject rod;
    private AudioSource audioSource;
    private float stateTimer = 0f;
    public float animTimespan = 2f;
    public bool startGoFish = false;

    [Header("Casting")]
    public AudioClip waterSplashSound;             
    public GameObject waterSplashEffect;           
    public bool rodCasting = false;

    [Header("Catching")]
    public GameObject waterBubblingEffect;         
    public AudioClip rodTensionSound;              
    public AudioClip fishStruggleSound;            
    public bool fishOnHook = false;

    [Header("Reeling")]
    public AudioClip reelSplashSound;              
    public GameObject fishSplashEffect;           
    public bool fishComesOut = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (startGoFish && currentState == State.Idle)
        {
            StartInteraction();
        }

        HandleState();
    }

    void StartInteraction()
    {
        currentState = State.Casting;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;

            case State.Casting:
                HandleCastingState();
                break;

            case State.Catching:
                HandleCatchingState();
                break;

            case State.Reeling:
                HandleReelingState();
                break;
        }
    }

    void HandleIdleState()
    {
        stateTimer = 0f;
        fishOnHook = false;
        rod.SetActive(false);
    }

    void HandleCastingState()
    {
        PrepareRodForCasting(true);

        if (rodCasting)
        {
            
            ExecuteRodCast("CastRod", waterSplashSound, waterSplashEffect);
            currentState = State.Catching;
            stateTimer = 0f;
        }
    }

    void HandleCatchingState()
    {
        
        ExecuteCatching("CatchFish", rodTensionSound, fishStruggleSound, waterBubblingEffect);

        if (fishOnHook)
        {
            currentState = State.Reeling;
            stateTimer = 0f;
        }
    }

    void HandleReelingState()
    {
        PrepareReeling();

        if (fishComesOut)
        {
            
            ExecuteReeling("ReelInFish", reelSplashSound, fishSplashEffect);
            currentState = State.Idle;
        }
    }

    void PrepareRodForCasting(bool rodVisibility)
    {
        rod.SetActive(rodVisibility);
        InstructPlayerToCast();
    }

    void InstructPlayerToCast()
    {
        Debug.Log("Instructing the player to cast the rod!");
    }

    void ExecuteRodCast(string animName, AudioClip splashSound, GameObject splashEffect)
    {
        rodAnimator.SetTrigger(animName);
        PlaySound(splashSound);
        ActivateEffect(splashEffect);
    }

    void ExecuteCatching(string animName, AudioClip tensionSound, AudioClip struggleSound, GameObject bubblingEffect)
    {
        rodAnimator.SetTrigger(animName);
        PlaySound(tensionSound);
        PlaySound(struggleSound);
        ActivateEffect(bubblingEffect);
    }

    void PrepareReeling()
    {
        fishOnHook = true;
        rodAnimator.SetTrigger("ReelIn");
    }

    void ExecuteReeling(string animName, AudioClip splashSound, GameObject splashEffect)
    {
        rodAnimator.SetTrigger(animName);
        PlaySound(splashSound);
        ActivateEffect(splashEffect);
    }

    void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void ActivateEffect(GameObject effect)
    {
        effect.SetActive(true);
    }


}

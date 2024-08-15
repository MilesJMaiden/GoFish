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
        Debug.Log("Starting interaction: Switching to Casting state.");
        currentState = State.Casting;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case State.Idle:
                Debug.Log("Handling Idle state.");
                HandleIdleState();
                break;

            case State.Casting:
                Debug.Log("Handling Casting state.");
                HandleCastingState();
                break;

            case State.Catching:
                Debug.Log("Handling Catching state.");
                HandleCatchingState();
                break;

            case State.Reeling:
                Debug.Log("Handling Reeling state.");
                HandleReelingState();
                break;
        }
    }

    void HandleIdleState()
    {
        Debug.Log("In Idle state: Resetting state variables.");
        stateTimer = 0f;
        fishOnHook = false;
        rod.SetActive(false);
    }

    void HandleCastingState()
    {
        Debug.Log("In Casting state: Preparing rod for casting.");
        PrepareRodForCasting(true);

        if (rodCasting)
        {
            Debug.Log("Rod casting triggered.");
            ExecuteRodCast("CastRod", waterSplashSound, waterSplashEffect);
            currentState = State.Catching;
            stateTimer = 0f;
        }
    }

    void HandleCatchingState()
    {
        Debug.Log("In Catching state: Attempting to catch a fish.");
        ExecuteCatching("CatchFish", rodTensionSound, fishStruggleSound, waterBubblingEffect);

        if (fishOnHook)
        {
            Debug.Log("Fish on hook! Switching to Reeling state.");
            currentState = State.Reeling;
            stateTimer = 0f;
        }
    }

    void HandleReelingState()
    {
        Debug.Log("In Reeling state: Preparing to reel in the fish.");
        PrepareReeling();

        if (fishComesOut)
        {
            Debug.Log("Fish comes out of the water! Returning to Idle state.");
            ExecuteReeling("ReelInFish", reelSplashSound, fishSplashEffect);
            currentState = State.Idle;
        }
    }

    void PrepareRodForCasting(bool rodVisibility)
    {
        Debug.Log("Setting rod visibility to: " + rodVisibility);
        rod.SetActive(rodVisibility);
        InstructPlayerToCast();
    }

    void InstructPlayerToCast()
    {
        Debug.Log("Instructing the player to cast the rod!");
    }

    void ExecuteRodCast(string animName, AudioClip splashSound, GameObject splashEffect)
    {
        Debug.Log("Executing rod cast with animation: " + animName);
        rodAnimator.SetTrigger(animName);
        PlaySound(splashSound);
        ActivateEffect(splashEffect);
    }

    void ExecuteCatching(string animName, AudioClip tensionSound, AudioClip struggleSound, GameObject bubblingEffect)
    {
        Debug.Log("Executing catching sequence with animation: " + animName);
        rodAnimator.SetTrigger(animName);
        PlaySound(tensionSound);
        PlaySound(struggleSound);
        ActivateEffect(bubblingEffect);
    }

    void PrepareReeling()
    {
        Debug.Log("Preparing to reel in the fish.");
        fishOnHook = true;
        rodAnimator.SetTrigger("ReelIn");
    }

    void ExecuteReeling(string animName, AudioClip splashSound, GameObject splashEffect)
    {
        Debug.Log("Executing reeling with animation: " + animName);
        rodAnimator.SetTrigger(animName);
        PlaySound(splashSound);
        ActivateEffect(splashEffect);
    }

    void PlaySound(AudioClip clip)
    {
        Debug.Log("Playing sound: " + clip.name);
        audioSource.PlayOneShot(clip);
    }

    void ActivateEffect(GameObject effect)
    {
        Debug.Log("Activating effect: " + effect.name);
        effect.SetActive(true);
    }


}

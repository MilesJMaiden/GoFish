using UnityEngine;

public class FishingRodInteraction : MonoBehaviour
{
    public enum State { Idle, Casting, Catching, Reeling }
    State currentState = State.Idle;

    [Header("Idle")]
    public Animator rodAnimator;
    public GameObject rod;
    private AudioSource audioSource;
    private float stateTimer = 0f;
    public float animTimespan = 2f;
    public bool startGoFish = false;

    [Header("Casting")]
    public AudioClip waterSplash;
    public GameObject splashEffect;
    public bool rodCasting = false;

    [Header("Catching")]
    public GameObject bubbleEffect;
    public AudioClip rodTension;
    public AudioClip fishStruggle;
    public bool bubblePoppinEnd = false;

    [Header("Reeling")]
    public GameObject fishOnHook;
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
                stateTimer = 0f;
                fishOnHook.SetActive(false);
                rod.SetActive(false);
                break;

            case State.Casting:
                
                BeforeCast(true);

                if (rodCasting)
                {
                    HandleRodCast("waterSplash",waterSplash, splashEffect);
                    

                    currentState = State.Catching;
                    stateTimer = 0f;
                }
             
                break;

            case State.Catching:
                
                BeforeCatching("waterSplash", waterSplash, splashEffect);

                if (bubblePoppinEnd)
                {
                    HandleCatching("waterSplash", waterSplash, splashEffect);

                    currentState = State.Reeling;
                    stateTimer = 0f;                  
                }               
                
                break;

            case State.Reeling:

                BeforeReeling();

                if (fishComesOut)
                {
                    HandleReeling("ReelIn", waterSplash, splashEffect);

                }
                
                break;
        }
    }


    void BeforeCast(bool rodvisibility)
    {
        rod.SetActive(rodvisibility);// Enable the rod
        
        // Instruct player to cast
        IntructPlayerToCast();

    }


    void IntructPlayerToCast()
    {
        // Some vfx again
        Debug.Log("I'm intructing the player to cast rod!");
    }

    void HandleRodCast(string animName, AudioClip clip, GameObject vfx)
    {

        rodAnimator.SetTrigger(animName);

        audioSource.PlayOneShot(clip);

        vfx.SetActive(true);
    }
   

    void BeforeCatching(string animName, AudioClip clip, GameObject vfx)
    {
        // Instantiate the vfx

        // Wait 2 second

        // Destroy the bubble poppin vfx

        // Turn the bool to true
    }

    void HandleCatching(string animName, AudioClip clip, GameObject vfx)
    {
        rodAnimator.SetTrigger(animName);

        audioSource.PlayOneShot(clip);

        vfx.SetActive(true);

    }

    void BeforeReeling()
    {
        fishOnHook.SetActive(true);// Enable fish gameobject

        // Check if the fish comes out
    }

    void HandleReeling(string animName, AudioClip clip, GameObject vfx)
    {
        rodAnimator.SetTrigger(animName);

        audioSource.PlayOneShot(clip);

        vfx.SetActive(true);


        // After a few amount of time, turn everything to idle

    }


}

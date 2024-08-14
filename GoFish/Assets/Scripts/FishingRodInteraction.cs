using Oculus.Interaction.Locomotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRodInteraction : MonoBehaviour
{
    public enum State { Idle, Casting, Catching, Reeling }
    State currentState = State.Idle;

    [Header("Casting")]
    public AudioClip waterSplash;

    [Header("Catching")]
    public AudioClip rodTension;
    [Header("Reeling")]
    
    
    public AudioClip fishStruggle;
    public GameObject splashEffect;
    public GameObject bubbleEffect;
    public GameObject fishOnHook;
    public GameObject rod;
    public Animator rodAnimator;

    private AudioSource audioSource;
    private float stateTimer = 0f;
    public float animTimespan = 2f;

    public bool startGoFish = false;
    public bool rodCasting = false;
    public bool bubblePoppinEnd = false;
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
            case State.Casting:
                // Enable the rod
                HandleRod(true);
                // Instruct player to cast
                IntructPlayerToCast();
                // If the rod touch the collider, trigger the effect and play the sound
                if (rodCasting)
                {
                    HandleRodCast();
                    
                    // Change the state
                    currentState = State.Catching;

                    // Reset the timer
                    stateTimer = 0f;
                }
             
                break;

            case State.Catching:
                // Intantiate bubble poppin vfx for 2 sec
                BubblePoppin();


                // Instantiate biting hook animation 
                // if the selecting rod bool is true, swtich to reeling
                if (bubblePoppinEnd)
                {
                    PlayAnimation("BitingHook");
                    stateTimer = 0f;
                    currentState = State.Reeling;
                }               
                
                break;

            case State.Reeling:
                // Enable fish gameobject
                fishOnHook.SetActive(true);

                // Play sound and effects for reeling
                if (fishComesOut)
                {
                    PlaySound(waterSplash);
                    TriggerEffect(splashEffect);
                    PlayAnimation("ReelIn");

                    // Count the time of animation
                    stateTimer += Time.deltaTime;
                    if (stateTimer > animTimespan) // Assuming reeling takes 2 seconds
                    {
                        currentState = State.Idle; // Reset to idle
                        HandleRod(false);
                    }
                }
                
                break;
        }
    }



    void HandleRod(bool rodvisibility)
    {
        rod.SetActive(rodvisibility);
    }

    void IntructPlayerToCast()
    {
        // Some vfx again
        Debug.Log("I'm intructing the player to cast rod!");
    }

    void HandleRodCast()
    {
        PlaySound(waterSplash);
        TriggerEffect(splashEffect);
        PlayAnimation("CastRod");
    }

    void BubblePoppin()
    {
        // Instantiate the vfx

        // Wait 2 second

        // Destroy the bubble poppin vfx

        // Turn the bool to true
    }

    void PlayAnimation(string name)
    {
        rodAnimator.SetTrigger(name);
    }

    void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void TriggerEffect(GameObject effect)
    {
        // Intantiate the VFX prefab in the dedicated place
        effect.SetActive(true);

        // Destroy it after a cerrtain timespan
    }


}

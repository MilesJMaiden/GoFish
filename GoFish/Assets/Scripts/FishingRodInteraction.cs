using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;



public class FishingRodInteraction : MonoBehaviour
{
    public enum State { Idle, Casting, Catching, Reeling }
    private State currentState = State.Idle;

    public GameObject gamespace;
    private Collider waterCollision;
    Vector3 leftControllerVelocity;
    Vector3 rightControllerVelocity;

    [Header("Idle")]
    public Animator rodAnimator;
    public GameObject rod;
    private AudioSource audioSource;
    public AudioClip popAudio;
    private float stateTimer = 0f;
    public float animTimespan = 2f;
    public bool startGoFish = false;

    [Header("Casting")]
    public AudioClip whooshAudio;
    public AudioClip waterSplashSound;
    public GameObject waterSplashEffect;
    public bool rodCasting = false;
    public GameObject lure;
    private Rigidbody lureRB;
    public float forwardVelocityThreshold = 2.0f;


    [Header("Catching")]
    public GameObject waterBubblingEffect;
    public AudioClip fishStruggleSound;
    public bool fishOnHook = false;

    public float hapticDuration = 5f;
    public float vibrationFrequency = 1.0f;
    public float vibrationAmplitude = 1.0f;



    [Header("Reeling")]
    public AudioClip reelSplashSound;
    public GameObject fishSplashEffect;
    public bool fishComesOut = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        waterCollision = gamespace.GetComponent<Collider>();
    }

    private IEnumerator TriggerHapticFeedback(OVRInput.Controller controller)
    {
        // Start the vibration
        OVRInput.SetControllerVibration(vibrationFrequency, vibrationAmplitude, controller);

        // Wait for the specified duration
        yield return new WaitForSeconds(hapticDuration);

        // Stop the vibration
        OVRInput.SetControllerVibration(0, 0, controller);
    }


    void Update()
    {
        if (startGoFish && currentState == State.Idle)
        {
            StartFishingInteraction();
        }

        HandleState();


        void StartFishingInteraction()
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

            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                Debug.Log("Trigger is pressed");
                leftControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
                if (leftControllerVelocity.z > forwardVelocityThreshold)
                {
                    castLure(true);
                    rodCasting = true;

                }
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            {
                Debug.Log("Trigger is pressed");
                rightControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
                if (rightControllerVelocity.z > forwardVelocityThreshold)
                {
                    castLure(false);
                    rodCasting = true;
                }
            }
            else {
                rodCasting = false;
            }


            if (rodCasting)
            {
                if (lureRB.isKinematic)
                {
                    currentState = State.Catching;
                }
            }

        }


        void castLure(bool isLeft)
        {
            PlaySound(whooshAudio);
            lureRB = lure.GetComponent<Rigidbody>();
            lureRB.useGravity = true;
            if (isLeft) {
                lureRB.AddForce(leftControllerVelocity*10);
            } else
            {
                lureRB.AddForce(rightControllerVelocity*10);
            }
        }

        void HandleCatchingState()
        {
            Debug.Log("In Catching state: Attempting to catch a fish.");
            //ExecuteCatching("CatchFish", rodTensionSound, fishStruggleSound, waterBubblingEffect);
            ExecuteFishStruggle(fishStruggleSound);

            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                Debug.Log("Trigger is pressed");
                leftControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
                if (leftControllerVelocity.z < -forwardVelocityThreshold)
                {
                    currentState = State.Reeling;
                }
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            {
                Debug.Log("Trigger is pressed");
                rightControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
                if (rightControllerVelocity.z < -forwardVelocityThreshold)
                {
                    currentState = State.Reeling;
                }
            }
        }

        void HandleReelingState()
        {
            Debug.Log("In Reeling state: Preparing to reel in the fish.");
            ExecuteReeling();

            if (fishComesOut)
            {
                Debug.Log("Fish comes out of the water! Returning to Idle state.");
                currentState = State.Idle;
            }
        }

        void PrepareRodForCasting(bool rodVisibility)
        {
            Debug.Log("Setting rod visibility to: " + rodVisibility);
            //Add Smoke effect
            if (!rod.activeSelf) {
                PlaySound(popAudio);
                rod.SetActive(rodVisibility);
            }
            InstructPlayerToCast();
        }

        void InstructPlayerToCast()
        {
            Debug.Log("Instructing the player to cast the rod!");
        }


        /*
        void ExecuteRodCast(string animName, AudioClip splashSound, GameObject splashEffect)
        {
            Debug.Log("Executing rod cast with animation: " + animName);
            rodAnimator.SetTrigger(animName);
            PlaySound(splashSound);
            ActivateEffect(splashEffect);
        } */

        void ExecuteFishStruggle(AudioClip struggleSound)
        {
            Debug.Log("Executing catching sequence with animation: ");

            StartCoroutine(TriggerHapticFeedback(OVRInput.Controller.RTouch));
            //PlaySound(struggleSound);
        }

        void ExecuteReeling()
        {
            Debug.Log("Preparing to reel in the fish.");
            fishOnHook = true;

            StopCoroutine(TriggerHapticFeedback(OVRInput.Controller.RTouch));
            //play exit sound
            PlaySound(reelSplashSound);

            //turn off center reel
            
            
            //Turn off rod
            rod.SetActive(false);

            //rodAnimator.SetTrigger("ReelIn");
            fishComesOut = true;
        }
        
        /*
        void ExecuteReeling(string animName, AudioClip splashSound, GameObject splashEffect)
        {
            Debug.Log("Executing reeling with animation: " + animName);
            rodAnimator.SetTrigger(animName);
            PlaySound(splashSound);
            ActivateEffect(splashEffect);
        } */

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
}

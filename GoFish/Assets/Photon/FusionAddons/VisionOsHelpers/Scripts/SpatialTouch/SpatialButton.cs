using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.VisionOsHelpers
{
    /**
    * 
    *  SpatialButton is an enhanced version of SpatialTouchable class
    *  It can be used as a press, toggle or radio button.
    *  It provides visual & audio feedback when the button is touched
    *  
    **/
    public class SpatialButton : SpatialTouchable
    {
        [Header("Current state")]
        public bool isButtonPressed = false;
        private MeshRenderer meshRenderer;

        public enum ButtonType
        {
            PressButton,
            RadioButton,
            ToggleButton
        }

        [Header("Button")]
        public ButtonType buttonType = ButtonType.PressButton;
        public bool toggleStatus = false;

        [SerializeField]
        List<SpatialButton> radioGroupButtons = new List<SpatialButton>();

        [Header("Anti-bounce")]
        public float timeBetweenTouchTrigger = 0.3f;

        [Header("Feedback")]
        private Material materialAtStart;
        [SerializeField] private Material touchMaterial;
        [SerializeField] private bool playSoundWhenTouched = true;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;

        [Tooltip("Set this to true if a toucher is also used, to avoid double callback triggering")]
        public bool ignoreSpatialContact = false;

        [Header("Sibling button")]
        [SerializeField]
        bool doNotallowTouchIfSiblingTouched = true;
        [SerializeField]
        bool doNotallowTouchIfSiblingWasRecentlyTouched = true;
        [SerializeField]
        bool automaticallyDetectSiblings = true;
        [SerializeField]
        List<SpatialButton> siblingButtons = new List<SpatialButton>();


        float lastTouchEnd = -1;

        public bool WasRecentlyTouched => lastTouchEnd != -1 && (Time.time - lastTouchEnd) < timeBetweenTouchTrigger;
        public bool IsToggleButton => buttonType == ButtonType.ToggleButton;
        public bool IsRadioButton => buttonType == ButtonType.RadioButton;



        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer) materialAtStart = meshRenderer.material;

            if (audioSource == null)
                audioSource = GetComponentInParent<AudioSource>();
            if (audioSource == null)
                Debug.LogError("AudioSource not found");
        }

        private void OnEnable()
        {
            // We need to clear if component was disabled 
            isButtonPressed = false;
            UpdateButton();
        }

        private void Start()
        {
            if (automaticallyDetectSiblings && transform.parent)
            {
                foreach (Transform child in transform.parent)
                {
                    if (child == transform) continue;
                    if (child.TryGetComponent<SpatialButton>(out var sibling))
                    {
                        siblingButtons.Add(sibling);
                    }
                }
            }


            if (IsRadioButton && radioGroupButtons.Count == 0)
            {
                foreach (var siblingButton in siblingButtons)
                {
                    if (siblingButton.IsRadioButton)
                    {
                        radioGroupButtons.Add(siblingButton);
                    }
                }
            }
        }



        bool CheckIfTouchIsAllowed()
        {
            if (WasRecentlyTouched)
            {
                // Local anti-bounce 
                return false;
            }
            if (doNotallowTouchIfSiblingTouched)
            {
                foreach (var sibling in siblingButtons)
                {
                    if (sibling.isButtonPressed)
                    {
                        Debug.LogError("Preventing due to active " + sibling);
                        return false;
                    }
                    else if (doNotallowTouchIfSiblingWasRecentlyTouched && sibling.WasRecentlyTouched)
                    {
                        // Sibling anti-bounce 
                        Debug.LogError("Preventing due to recently active" + sibling);
                        return false;

                    }
                }
            }
            return true;
        }

        public void ChangeButtonStatus(bool status)
        {
            toggleStatus = status;
            if (status)
            {
                base.OnTouchStart();
            }

            UpdateButton();
        }

        void ActivateRadioButton()
        {
            ChangeButtonStatus(true);
            foreach (var button in radioGroupButtons)
            {
                button.ChangeButtonStatus(false);
            }
        }

        [ContextMenu("OnTouchStart")]
        public override void OnTouchStart()
        {
            if (CheckIfTouchIsAllowed() == false) return;
            isButtonPressed = true;

            if (IsToggleButton)
            {
                ChangeButtonStatus(!toggleStatus);
            }
            else if (IsRadioButton)
            {
                ActivateRadioButton();
            }
            else
            {
                ChangeButtonStatus(true);
            }

            if (playSoundWhenTouched)
                PlayAudioFeeback();
        }

        [ContextMenu("OnTouchEnd")]
        public override void OnTouchEnd()
        {
            var buttonWasActive = isButtonPressed;
            isButtonPressed = false;


            if (buttonWasActive)
            {
                base.OnTouchEnd();
                lastTouchEnd = Time.time;
                if (buttonType == ButtonType.PressButton)
                {
                    ChangeButtonStatus(false);
                }
            }

            UpdateButton();

        }

        private void PlayAudioFeeback()
        {
            if (audioSource && audioClip && audioSource.isPlaying == false)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }

        void UpdateButton()
        {
            if (!meshRenderer) return;

            bool boutonActivated = isButtonPressed || toggleStatus;

            if (touchMaterial && boutonActivated)
            {
                meshRenderer.material = touchMaterial;
            }
            else if (materialAtStart && boutonActivated == false)
            {
                RestoreMaterial();
            }
        }

        private async void RestoreMaterial()
        {
            await System.Threading.Tasks.Task.Delay(100);
            meshRenderer.material = materialAtStart;
        }
    }
}

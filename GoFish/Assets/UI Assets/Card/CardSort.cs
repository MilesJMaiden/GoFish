using config;
using DefaultNamespace;
using events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSort : MonoBehaviour
{


    [Header("Rotation")]
    [SerializeField]
    [Range(-90f, 90f)]
    private float maxCardRotation;

    [SerializeField]
    private float maxHeightDisplacement;

    [SerializeField]
    private ZoomConfig zoomConfig;

    [Header("Events")]
    [SerializeField]
    private EventsConfig eventsConfig;


    private RectTransform rectTransform;
    private CardWrapper currentDraggedCard;
    private List<CardWrapper> allCards = new();

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        InitCards();
    }

    private void InitCards()
    {
        SetUpCards();
        SetCardsAnchor();
    }



    void SetUpCards()
    {
        allCards.Clear();
        foreach (Transform card in transform)
        {
            var wrapper = card.GetComponent<CardWrapper>();
            if (wrapper == null)
            {
                wrapper = card.gameObject.AddComponent<CardWrapper>();
            }

            allCards.Add(wrapper);

            AddOtherComponentsIfNeeded(wrapper);

            // Pass child card any extra config it should be aware of
            wrapper.zoomConfig = zoomConfig;
            //wrapper.animationSpeedConfig = animationSpeedConfig;
            wrapper.eventsConfig = eventsConfig;
            //wrapper.preventCardInteraction = preventCardInteraction;
            wrapper.container = this;
        }
    }

    private void AddOtherComponentsIfNeeded(CardWrapper wrapper)
    {
        var canvas = wrapper.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = wrapper.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;

        if (wrapper.GetComponent<GraphicRaycaster>() == null)
        {
            wrapper.gameObject.AddComponent<GraphicRaycaster>();
        }
    }


    private void SetCardsAnchor()
    {
        foreach (CardWrapper child in allCards)
        {
            child.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        }
    }




    // Update is called once per frame
    void Update()
    {
        
    }

    private float GetCardRotation(int index)
    {
        if (allCards.Count < 3) return 0;
        // Associate a rotation based on the index in the cards list
        // so that the first and last cards are at max rotation, mirrored around the center
        return -maxCardRotation * (index - (allCards.Count - 1) / 2f) / ((allCards.Count - 1) / 2f);
    }


    private void SetCardsRotation()
    {
        for (var i = 0; i < allCards.Count; i++)
        {
            allCards[i].targetRotation = GetCardRotation(i);
            allCards[i].targetVerticalDisplacement = GetCardVerticalDisplacement(i);
        }
    }


    private float GetCardVerticalDisplacement(int index)
    {
        if (allCards.Count < 3) return 0;
        // Associate a vertical displacement based on the index in the cards list
        // so that the center card is at max displacement while the edges are at 0 displacement
        return maxHeightDisplacement *
               (1 - Mathf.Pow(index - (allCards.Count - 1) / 2f, 2) / Mathf.Pow((allCards.Count - 1) / 2f, 2));
    }

    public void OnCardDragStart(CardWrapper card)
    {
        currentDraggedCard = card;
    }


    public void OnCardDragEnd()
    {
        currentDraggedCard = null;
    }
}

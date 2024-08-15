using config;
using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSort : MonoBehaviour
{

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
            //wrapper.zoomConfig = zoomConfig;
            //wrapper.animationSpeedConfig = animationSpeedConfig;
            //wrapper.eventsConfig = eventsConfig;
            //wrapper.preventCardInteraction = preventCardInteraction;
            //wrapper.container = this;
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
}

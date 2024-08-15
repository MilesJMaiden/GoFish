using config;
using DefaultNamespace;
using events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    void Update()
    {
        UpdateCards();
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

    private void UpdateCards()
    {
        if (transform.childCount != allCards.Count)
        {
            InitCards();
        }

        if (allCards.Count == 0)
        {
            //PULL 5 cards out of pool

            //if pool < 5 cards, End Game.
            return;
        }

        SetCardsPosition();
        SetCardsRotation();
        SetCardsUILayers();
        UpdateCardOrder();
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


    private void SetCardsPosition()
    {
        // Compute the total width of all the cards in global space
        var cardsTotalWidth = allCards.Sum(card => card.width * card.transform.lossyScale.x);

        // Compute the width of the container in global space
        var containerWidth = rectTransform.rect.width * transform.lossyScale.x;
        if (cardsTotalWidth > containerWidth)
        {
            DistributeChildrenToFitContainer(cardsTotalWidth);
        }
        else
        {
            DistributeChildrenWithoutOverlap(cardsTotalWidth);
        }
    }

    private void SetCardsUILayers()
    {
        for (var i = 0; i < allCards.Count; i++)
        {
            allCards[i].uiLayer = zoomConfig.defaultSortOrder + i;
        }
    }


    private void UpdateCardOrder()
    {
        if (currentDraggedCard == null) return;

        // Get the index of the dragged card depending on its position
        var newCardIdx = allCards.Count(card => currentDraggedCard.transform.position.x > card.transform.position.x);
        var originalCardIdx = allCards.IndexOf(currentDraggedCard);
        if (newCardIdx != originalCardIdx)
        {
            allCards.RemoveAt(originalCardIdx);
            if (newCardIdx > originalCardIdx && newCardIdx < allCards.Count - 1)
            {
                newCardIdx--;
            }

            allCards.Insert(newCardIdx, currentDraggedCard);
        }
        // Also reorder in the hierarchy
        currentDraggedCard.transform.SetSiblingIndex(newCardIdx);
    }


    private void DistributeChildrenToFitContainer(float childrenTotalWidth)
    {
        // Get the width of the container
        var width = rectTransform.rect.width * transform.lossyScale.x;
        // Get the distance between each child
        var distanceBetweenChildren = (width - childrenTotalWidth) / (allCards.Count - 1);
        // Set all children's positions to be evenly spaced out
        var currentX = transform.position.x - width / 2;
        foreach (CardWrapper child in allCards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentX + adjustedChildWidth / 2, transform.position.y);
            currentX += adjustedChildWidth + distanceBetweenChildren;
        }
    }

    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth)
    {
        var currentPosition = transform.position.x - childrenTotalWidth / 2;
        foreach (CardWrapper child in allCards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y);
            currentPosition += adjustedChildWidth;
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

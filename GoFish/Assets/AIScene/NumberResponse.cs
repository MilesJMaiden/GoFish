using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberResponse : MonoBehaviour
{
    public GameObject threeCard; 
    public GameObject fourCard;  
    public GameObject sevenCard; 
    public GameObject nineCard;  

    public float moveSpeed = 1f;   
    public float rotationSpeed = 90f;

    public void RespondToCommand(string[] response)
    {
        if (response.Length == 0)
            return;

        switch (response[0].ToLower())
        {
            case "three":
                StartCoroutine(MoveAndRotateCard(threeCard));
                break;

            case "four":
                StartCoroutine(MoveAndRotateCard(fourCard));
                break;

            case "seven":
                StartCoroutine(MoveAndRotateCard(sevenCard));
                break;

            case "nine":
                StartCoroutine(MoveAndRotateCard(nineCard));
                break;
        }
    }

    private IEnumerator MoveAndRotateCard(GameObject card)
    {
        Vector3 targetPosition = new Vector3(card.transform.position.x, card.transform.position.y, -5f);
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        while (Vector3.Distance(card.transform.position, targetPosition) > 0.01f || Quaternion.Angle(card.transform.rotation, targetRotation) > 0.01f)
        {
            // Position movement
            card.transform.position = Vector3.MoveTowards(card.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Rotation movement
            card.transform.rotation = Quaternion.RotateTowards(card.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            yield return null;
        }

        // Ensure final position and rotation are exact
        card.transform.position = targetPosition;
        card.transform.rotation = targetRotation;
    }
}
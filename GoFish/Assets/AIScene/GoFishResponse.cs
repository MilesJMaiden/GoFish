using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoFishResponse : MonoBehaviour
{
    [SerializeField] List<GameObject> shapes;

    public void SetGoFish(string[] response)
    {

        if (response.Length > 0 && response[0] == "Go fish")
        {
            foreach (var shape in shapes)
            {
                shape.SetActive(true);
            }
        }
    }
}

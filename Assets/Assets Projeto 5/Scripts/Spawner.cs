using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public bool isFree = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isFree = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isFree = true;
        }
    }
}

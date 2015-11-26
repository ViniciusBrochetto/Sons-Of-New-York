using UnityEngine;
using System.Collections;

public class PayloadObjective : MonoBehaviour
{
    public Team team;

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bed")
        {
            GameObject.FindObjectOfType<GameMenuController>().ObjectiveAccomplished(team);
        }
    }
}

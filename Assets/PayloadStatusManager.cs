using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PayloadStatusManager : MonoBehaviour
{

    public Transform[] waypointsTA;
    public Transform[] waypointsTB;

    public float teamADistance;
    public float teamBDistance;

    public float teamATotalDistance = 0;
    public float teamBTotalDistance = 0;

    public int currWaypointTA, currWaypointTB;
    public float teamACurrDistance = 0f, teamBCurrDistance = 0f;

    public Transform bed;
    public Slider sld_Status;

    private bool gameStarted = false;
    public Text txtGameTimer;

    void Start()
    {
        if (waypointsTA != null && waypointsTB != null)
        {

            for (int i = 1; i < waypointsTA.Length; i++)
            {
                teamATotalDistance += Vector3.Distance(waypointsTA[i - 1].position, waypointsTA[i].position);
            }
            for (int i = 1; i < waypointsTB.Length; i++)
            {
                teamBTotalDistance += Vector3.Distance(waypointsTB[i - 1].position, waypointsTB[i].position);
            }

            teamADistance = (int)teamADistance;
            teamBDistance = (int)teamBDistance;

            currWaypointTA = 0;
            currWaypointTB = 0;
        }
    }

    void Update()
    {
        txtGameTimer.text = PhotonNetwork.room.GetRoomTimeInMinutes();
        if (!gameStarted && PhotonNetwork.room.GetRoomTime() / 600f < 1f)
        {
            gameStarted = true;
            GameObject g = GameObject.Find("SpawnLockers");
            if (g != null)
                Destroy(g);
        }

        if (PhotonNetwork.room.GetRoomTime() <= 0f)
        {
            GameObject.FindObjectOfType<GameMenuController>().ObjectiveAccomplished(teamACurrDistance < teamBCurrDistance ? Team.TeamA : Team.TeamB);
        }


        if (bed == null)
        {
            if (GameObject.Find("Bed(Clone)"))
                bed = GameObject.Find("Bed(Clone)").transform;
            else
                Debug.Log("Bed not found");
        }
        else
        {
            teamACurrDistance = 0f;
            teamBCurrDistance = 0f;
            float aux = 0f;
            bool aux2 = false;

            aux = Vector3.Distance(bed.position, waypointsTA[0].position);
            for (int i = 1; i < waypointsTA.Length; i++)
            {
                if (Vector3.Distance(waypointsTA[i - 1].position, waypointsTA[i].position) < Vector3.Distance(bed.position, waypointsTA[i].position))
                {
                    teamACurrDistance += Vector3.Distance(waypointsTA[i - 1].position, waypointsTA[i].position);
                }
                else
                {
                    aux = Vector3.Distance(bed.position, waypointsTA[i].position);
                    aux2 = true;
                    teamACurrDistance = aux;
                }
            }

            teamBCurrDistance += Vector3.Distance(bed.position, waypointsTB[0].position);
            aux = Vector3.Distance(bed.position, waypointsTB[0].position);
            aux2 = false;
            for (int i = 1; i < waypointsTB.Length; i++)
            {
                if (Vector3.Distance(waypointsTB[i - 1].position, waypointsTB[i].position) < Vector3.Distance(bed.position, waypointsTB[i].position))
                {
                    teamBCurrDistance += Vector3.Distance(waypointsTB[i - 1].position, waypointsTB[i].position);
                }
                else
                {
                    aux = Vector3.Distance(bed.position, waypointsTB[i].position);
                    aux2 = true;
                    teamBCurrDistance = aux;
                }
            }

            if (teamACurrDistance < teamBCurrDistance)
            {
                sld_Status.value = (teamACurrDistance / teamATotalDistance / 2f);
            }
            else
            {
                sld_Status.value = 1f - (teamBCurrDistance / teamBTotalDistance / 2f);
            }
        }
    }
}

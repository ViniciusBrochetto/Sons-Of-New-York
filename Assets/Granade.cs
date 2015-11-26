using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Granade : MonoBehaviour
{
    public double timer;

    public float maxRange;
    public float maxDamage;
    public float minDamage;

    public AudioClip audioExplosion;
    public GameObject particleExplosion;

    void Start()
    {
        timer += PhotonNetwork.time;
    }

    void Update()
    {
        if (PhotonNetwork.time > timer)
        {
            AudioSource.PlayClipAtPoint(audioExplosion, transform.position, 1f);
            Instantiate(particleExplosion, transform.position, particleExplosion.transform.rotation);

            if (PhotonNetwork.isMasterClient)
            {
                List<Player> playersHit = new List<Player>();
                foreach (Collider c in Physics.OverlapSphere(transform.position, 10f, 1 << LayerMask.NameToLayer("Player")))
                {
                    if (playersHit.IndexOf(c.transform.GetComponent<Player>()) == -1)
                    {
                        playersHit.Add(c.transform.GetComponent<Player>());
                        c.transform.GetComponent<PlayerInput>().InformDamage((int)Mathf.Lerp(maxDamage, minDamage, Vector3.Distance(transform.position, c.transform.position) / maxRange));
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}

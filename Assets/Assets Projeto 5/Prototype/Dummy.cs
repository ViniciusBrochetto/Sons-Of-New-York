using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour
{
    public float speed;

    void FixedUpdate()
    {
        Vector3 v = Vector3.zero;
        v += transform.forward * speed * Time.fixedDeltaTime + this.transform.position;
        transform.GetComponent<Rigidbody>().MovePosition(v);
    }
}

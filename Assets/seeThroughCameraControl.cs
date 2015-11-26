using UnityEngine;
using System.Collections;

public class seeThroughCameraControl : MonoBehaviour
{
    public Shader shader;

    double time;
    Camera cam;

    void Start()
    {
        cam = this.GetComponent<Camera>();
        cam.SetReplacementShader(shader, null);
    }

    void Update()
    {
        cam.enabled = PhotonNetwork.time < time;
    }

    public void SetTimer(double t)
    {
        time = t;
    }
}

using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float cameraSpeed;

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
            return;

        transform.position += new Vector3(cameraSpeed * Time.deltaTime, 0, 0);
    }
}
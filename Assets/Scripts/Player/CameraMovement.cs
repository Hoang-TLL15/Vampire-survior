using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            // Set the desired position for the Player
            playerObject.transform.position = new Vector3(0, 0, 0);

            // Set the camera to track the Player
            CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
            if (cameraMovement != null)
            {
                cameraMovement.target = playerObject.transform;
            }
        }
    }

    void Update()
    {
        transform.position = target.position + offset;
    }
}

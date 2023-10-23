using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public float movementMultiplier;
    private Transform pCamera;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        pCamera = Camera.main.transform;
        lastCameraPosition = pCamera.position;
    }

    private void LateUpdate()
    {
        Vector3 bgMovement = pCamera.position - lastCameraPosition;
        transform.position += new Vector3(bgMovement.x * movementMultiplier, bgMovement.y, 0);
        lastCameraPosition = pCamera.position;
    }
}

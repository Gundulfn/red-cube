using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 120f;
    
    private Transform playerTransform;
    private float xRotation = 0f;

    public void SetPlayerTransform(Transform t) {
        playerTransform = t;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);
    }
}

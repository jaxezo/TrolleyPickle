using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform playerCamera;

    public float sensitivity = 1f;

    public float upperLimit = 80f;
    public float lowerLimit = -80f;

    private float rotationX = 0f; // Vertical rotation
    private float rotationY = 0f; // Horizontal rotation

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    public void OnLook (InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();

        // Rotate camera based on mouse movement
        rotationY += mouseDelta.x * sensitivity * Time.deltaTime;
        rotationX -= mouseDelta.y * sensitivity * Time.deltaTime;

        // Clamp vertical rotation to prevent roll over
        rotationX = Mathf.Clamp(rotationX, lowerLimit, upperLimit);

        // Apply the rotation
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    public void OnFire (InputAction.CallbackContext context)
    {
        GameManager.singleton.FlipSwitch ();
    }
}

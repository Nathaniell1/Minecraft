using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public float mouseSensitivity = 180;

	public Transform playerBody;

	private float xRotation = 0f;

	// Start is called before the first frame update
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		mouseSensitivity = 180;

		if (Application.isEditor)
			mouseSensitivity = 400;
	}

	float mx;

	// Update is called once per frame
	void Update()
	{


		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		if (Mathf.Abs(mouseX) > 40 || Mathf.Abs(mouseY) > 40)
			return;

		//camera's x rotation (look up and down)
		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

		//player body's y rotation (turn left and right)
		playerBody.Rotate(Vector3.up * mouseX);

	}
}

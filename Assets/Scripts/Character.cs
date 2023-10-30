using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
	private CharacterController controller;

	public float speed;
	public float fallSpeed;

	// Start is called before the first frame update
	void Start()
	{
		controller = GetComponent<CharacterController>();
    }

	// Update is called once per frame
	void Update()
	{
		Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		controller.Move(Time.deltaTime * move * speed);

		//fake gravity
		Vector3 gravity = new Vector3(0, -fallSpeed, 0);
		controller.Move(gravity * Time.deltaTime);
	}
}

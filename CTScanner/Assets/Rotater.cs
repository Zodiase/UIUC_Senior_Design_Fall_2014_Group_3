using UnityEngine;
using System.Collections;

public class Rotater : MonoBehaviour {

	public Camera cam;
	public float sensitivity;
	Vector3 mouseAnchor;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			if (!Input.GetMouseButtonDown(0)) {
				//this.transform.Rotate (Input.mousePosition - mouseAnchor);
				Vector3 delta = Input.mousePosition - mouseAnchor;
				this.transform.RotateAround (this.transform.position, cam.transform.up, -delta.x * 0.1f * sensitivity);
				this.transform.RotateAround (this.transform.position, cam.transform.right, delta.y * 0.1f * sensitivity);
			}
			mouseAnchor = Input.mousePosition;
		}
	}
}

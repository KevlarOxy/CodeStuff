using UnityEngine;
using System.Collections;

public class ZoomInOut : MonoBehaviour {

	private Camera eyes;
	private float defaultFOV;
	private float zoomTime = 1f;
	private float increment;
	
	void Start () {
		eyes = GetComponent<Camera> ();
		defaultFOV = eyes.fieldOfView;
	}
	
	void Update () {
		if (Input.GetButton ("Zoom")) {
			Zooming();
		} else {
			ZoomingOut();
		}
	}
	
	void Zooming() {
		if (eyes.fieldOfView >= 40f) {
			eyes.fieldOfView = increment - zoomTime;
			increment = eyes.fieldOfView;
		} else {
			increment = eyes.fieldOfView;
		}
	}
	
	void ZoomingOut() {
		if (eyes.fieldOfView < defaultFOV) {
			eyes.fieldOfView = increment + (zoomTime * 3);
			increment = eyes.fieldOfView;
		}
	}
}


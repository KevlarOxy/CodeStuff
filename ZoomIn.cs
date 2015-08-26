using UnityEngine;
using System.Collections;

public class ZoomIn : MonoBehaviour {

	private Camera eyes;
	private float defaultFOV;
	private float FOVstore;
	
	public float fieldOfView;
	public float zoomSpeed = 0.1f;
	public int minZoom = 10;
	
	void Start () {
		eyes = GetComponent<Camera> ();
		defaultFOV = eyes.fieldOfView;
	}
	
	void Update () {
		if (Input.GetButton("Zoom")) {
			Gozoom();
		} else {
			eyes.fieldOfView = defaultFOV;
			FOVstore = defaultFOV;
			}
		}
	
	void Gozoom () {
		if (eyes.fieldOfView >= 20) {
			eyes.fieldOfView = FOVstore - zoomSpeed;
			FOVstore = eyes.fieldOfView;
			} else {
			FOVstore = eyes.fieldOfView;
		} 
	}
}

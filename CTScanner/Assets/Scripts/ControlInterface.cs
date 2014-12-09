using UnityEngine;
using System.Collections;

public class ControlInterface : MonoBehaviour {

	public Tasker tk;
	public MCU mc;
	public WebCam wc;
	public bool showMCUI;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	const int CamListStartX = 10;
	const int CamListStartY = 40;
	const int CamListItemWidth = 200;
	const int CamListItemHeight = 30;
	const int CamListGapBetweenItem = 5;

	Rect cameraListRect = new Rect (10, 10, 220, 200);
	Rect cameraCaptureButtonRect = new Rect (160, 0, 60, 20);
	Vector2 cameraListScrollPosition = Vector2.zero;
	int selectedCamId = -1;

	Rect portListRect = new Rect (Screen.width - 10 - 250, 10, 250, 150);
	Rect portRescanButtonRect = new Rect (170, 0, 80, 20);
	Vector2 portListScrollPosition = Vector2.zero;
	int selectedPortId = -1;

	Rect mcuControlRect = new Rect (Screen.width - 10 - 200, 10 + 150 + 10, 200, Screen.height - ((10 + 150 + 10) + 10));

	void OnGUI() {

		/*================= WebCam UI =================*/
		GUILayout.BeginArea (cameraListRect);
		GUILayout.BeginVertical ();
		GUILayout.Label ("Select Camera Feed:");
		if (!wc.ready) {
			GUI.enabled = false;
		} // if
		if (GUI.Button (cameraCaptureButtonRect, "Capture")) {
			wc.TakeSnapshot();
		} // if
		GUI.enabled = true;

		cameraListScrollPosition = GUILayout.BeginScrollView (cameraListScrollPosition);
		selectedCamId = GUILayout.SelectionGrid (selectedCamId, wc.deviceNames, 1);
		selectedCamId = wc.SwitchCameraFeed (selectedCamId);
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();

		/*================= MCU UI =================*/
		GUILayout.BeginArea (portListRect);
		GUILayout.BeginVertical ();
		if (GUI.Button (portRescanButtonRect, "Rescan")) {
			mc.ScanSerialPorts ();
			selectedPortId = -1;
		} // if
		GUILayout.Label (string.Format ("{0} serial port{1} available: ", mc.ports.Length, (mc.ports.Length > 1) ? "s" : ""));
		portListScrollPosition = GUILayout.BeginScrollView (portListScrollPosition);
		selectedPortId = GUILayout.SelectionGrid (selectedPortId, mc.ports, 1);
		selectedPortId = mc.SwitchPort (selectedPortId);
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();

		GUILayout.BeginArea (mcuControlRect);
		GUILayout.BeginVertical ();
		if (!mc.isConnected && !showMCUI) {
			// UI for connecting MCU.
			GUILayout.Label ("Connect to a serial port to reveal the control options.");
		} else {
			// UI for controlling MCU.
			GUILayout.Label (string.Format ("Controls {0}", mc.isBusy ? "(MCU is busy)" : ""));
			if (mc.isBusy) {
				GUI.enabled = false;
			} // if
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("LED On")) {
				mc.LEDTurnOn ();
			}
			if (GUILayout.Button ("LED Off")) {
				mc.LEDTurnOff ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.Label ("Perform set of shots:");
			if (GUILayout.Button ("45-degree shots")) {
				mc.PerformSuit (4);
			}
			if (GUILayout.Button ("30-degree shots")) {
				mc.PerformSuit (6);
			}
			if (GUILayout.Button ("15-degree shots")) {
				mc.PerformSuit (12);
			}
			GUILayout.Label ("Debugging");
			if (GUILayout.Button ("Face")) {
				mc.GetBoxFace ();
			}
			if (GUILayout.Button ("Angle")) {
				mc.GetServoAngle (-1);
			}
			GUI.enabled = true;
		}
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
	}
}

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using UnityEngine.UI;

public class MCU : MonoBehaviour {
	
	public WebCam wc;
	public Button RescanButton;
	public RectTransform SerialPortList;
	public Button ListItem_Button_Prefab;

	public RectTransform NoConnectionWarning;
	public RectTransform SerialCommandControls;
	
	int activePortId = -1;
	string activePortName = "";
	static SerialPort stream;
	const string MCUHelloWorld = "CTSCANNER:Hello_World.";
	public bool isBusy {
		get;
		private set;
	}
	public string[] ports {
		get;
		private set;
	}
	public bool isConnected {
		get {
			if (stream == null) {
				return false;
			} else {
				return isOpen;
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		ScanSerialPorts ();
		isBusy = false; //! for debugging purpose. change to false when done.
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ClearSerialPortList () {
		foreach (Transform child in SerialPortList.transform) {
			GameObject.Destroy (child.gameObject);
		}
	}

	public void ScanSerialPorts () {
		RescanButton.interactable = false;

		// Updating ports resets connections.
		SerialClose ();
		// Resetting.
		ClearSerialPortList ();
		
		// OSX and Windows uses different port names.
		// And SerialPort.GetPortNames() does not work on Unix. Shame on you MacroHard!
		#if UNITY_STANDALONE_OSX
		string[] ttys = Directory.GetFiles ("/dev/", "tty.*");
		List<string> portList = new List<string> ();
		foreach (string dev in ttys) {
			if (dev.StartsWith ("/dev/tty.")) {
				portList.Add (dev);
			}
		}
		ports = portList.ToArray ();
		#elif UNITY_STANDALONE_WIN
		ports = SerialPort.GetPortNames();
		#endif

		for (int i = 0, n = ports.Length; i < n; i++) {
			int portId = i;
			// Create the button for selecting this camera feed.
			Button thisSerialPortButton = Button.Instantiate (ListItem_Button_Prefab) as Button;
			// Add the button to the camera feed list.
			RectTransform thisSerialPortButtonRect = thisSerialPortButton.GetComponent<RectTransform> ();
			thisSerialPortButtonRect.SetParent(SerialPortList);
			thisSerialPortButtonRect.localScale = new Vector3 (1, 1, 1);
			// Change button text.
			thisSerialPortButtonRect.GetComponentInChildren<Text> ().text = ports[i];
			// Set button behaviour.
			thisSerialPortButton.onClick.AddListener (() => {
				ColorBlock normalColorBlock = ListItem_Button_Prefab.colors;
				foreach (Button button in SerialPortList.GetComponentsInChildren<Button> ()) {
					button.colors = normalColorBlock;
				}
				SwitchPort (portId);
				ColorBlock pressedColorBlock = ListItem_Button_Prefab.colors;
				pressedColorBlock.normalColor = pressedColorBlock.highlightedColor = pressedColorBlock.pressedColor;
				thisSerialPortButton.colors = pressedColorBlock;
			});
		}

		print ("Serial ports updated.");

		RescanButton.interactable = true;
	}
	
	public int SwitchPort (int index) {
		if (index < 0 || index >= ports.Length) {
			return activePortId;
		} // if
		// else
		
		string newPortName = ports [index];
		SerialOpen (newPortName);
		if (isConnected) {
			return index;
		} else {
			return -1;
		}
	}
	
	#region Suits
	public void PerformSuit (int size) {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (PerformingSuit (size:size));
	}
	IEnumerator PerformingSuit (int size) {
		float stepSize = 180f / size;
		
		// Turn LED Off.
		yield return StartCoroutine (LEDTurningOff (resetBusy: false));
		
		// Get facing.
		float facing = GetBoxFace ();
		int faceId = Mathf.FloorToInt (facing);
		int rotation = Mathf.FloorToInt ((facing - faceId) * 10);
		print (string.Format ("Face: {0}:{1}.", faceId, rotation));

		// Resetting platform to origin.
		yield return StartCoroutine (PlatformResettingToBase ());

		for (int i = 0; i < size; i++) {
			float degree = stepSize * i;
			print (string.Format ("Turning platform to {0} degrees.", degree));
			yield return StartCoroutine (PlatformTurningTo (degree: degree, resetBusy: false));
			// Turn LED On.
			yield return StartCoroutine (LEDTurningOn (resetBusy: false));
			yield return new WaitForSeconds (3f);
			// Take picture.
			float finalAngle = (rotation * 90 + degree) % 360;
			string shotName = string.Format ("{0}_{1}", faceId, finalAngle);
			wc.TakeSnapshot (shotName);
			yield return new WaitForSeconds (0.5f);
			// Turn LED Off.
			yield return StartCoroutine (LEDTurningOff (resetBusy: false));
			yield return new WaitForFixedUpdate ();
		}

		yield return StartCoroutine (PlatformTurningTo (degree: 0, resetBusy: false));

		isBusy = false;
		print ("Suit complete. Now you may rotate the object.");
		yield break;
	}
	#endregion

	#region Stepper Rotation
	/// <summary>
	/// Keep rotating the stepper in one direction until the reset switch is triggered.
	/// </summary>
	public void PlatformResetToBase () {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (PlatformResettingToBase (resetBusy:true));
	}
	IEnumerator PlatformResettingToBase (bool resetBusy = false) {
		Drop ();
		Send ("C");
		// Wait for confirmation.
		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();

		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}

	public void PlatformStepLeft () {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (PlatformSteppingLeft (resetBusy:true));
	}
	IEnumerator PlatformSteppingLeft (bool resetBusy = false) {
		Drop ();
		Send ("<");
		// Wait for confirmation.
		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();
		
		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}

	public void PlatformStepRight () {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (PlatformSteppingRight (resetBusy:true));
	}
	IEnumerator PlatformSteppingRight (bool resetBusy = false) {
		Drop ();
		Send (">");
		// Wait for confirmation.
		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();
		
		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}

	public void PlatformTurnTo (float degree) {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (PlatformTurningTo (degree:degree, resetBusy:true));
	}
	IEnumerator PlatformTurningTo (float degree, bool resetBusy = false) {
		Drop ();
		Send (string.Format ("D{0}", degree));
		// Wait for confirmation.
		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();
		
		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}
	public void SetStepSpeed (int rpm) {
		Drop ();
		Send (string.Format ("S{0}", rpm));
		ReceiveLine ();
		Drop ();
	}
	#endregion

	#region Light Switch
	public void LEDToggle (bool isChecked) {
		if (isChecked) {
			LEDTurnOn ();
		} else {
			LEDTurnOff ();
		}
	}

	public void LEDTurnOn () {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (LEDTurningOn (resetBusy:true));
	}
	IEnumerator LEDTurningOn (bool resetBusy = false) {
		Drop ();
		Send ("L1");
		// Assume the next received line is the confirmation and no validation is done.
		// [TODO] Change this.
		
		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();
		
		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}
	
	public void LEDTurnOff () {
		// Only one task allowed at a time.
		if (isBusy) {
			return;
		} // if
		// else
		
		isBusy = true;
		StartCoroutine (LEDTurningOff (resetBusy:true));
	}
	IEnumerator LEDTurningOff (bool resetBusy = false) {
		Drop ();
		Send ("L0");
		// Assume the next received line is the confirmation and no validation is done.
		// [TODO] Change this.

		// Let the coroutine wait for the data so the app doesn't need to be blocked (for too long).
		while (noData) {
			yield return new WaitForFixedUpdate ();
		}
		// This line might still block because the data may not contain a line.
		ReceiveLine ();
		Drop ();
		
		if (resetBusy) {
			isBusy = false;
		}
		yield break;
	}
	#endregion
	
	#region Orientation Detection
	public double GetServoAngle () {
		Drop ();
		Send ("A");
		// Assume the next received line is the response and no validation is done.
		// [TODO] Change this.
		string resp = ReceiveLine ();
		Drop ();
		try {
			return double.Parse (resp);
		} catch (System.Exception) {
			print (string.Format ("GetServoAngle - Error when parsing to double: {0}", resp));
			return -1;
		}
	}
	
	/// <summary>
	/// Returns a numerical value indicating the facing and orientation of the box.
	/// The integral part of the value represents which face is facing down,
	/// while the decimal part of the value represents the rotation of the face.
	/// Note: This function communicates with the serial device and blocks.
	/// </summary>
	/// <returns>The box facing value.</returns>
	public float GetBoxFace () {
		Drop ();
		Send ("F");
		// Assume the next received line is the response and no validation is done.
		// [TODO] Change this.
		// Drop the first line which says "Scanning for face pattern.".
		ReceiveLine ();
		string resp = ReceiveLine ();
		Drop ();
		try {
			return float.Parse (resp);
		} catch (System.Exception) {
			print (string.Format ("GetBoxFace - Error when parsing to float: {0}", resp));
			return 0;
		}
	}

	public void TestFaceScanning () {
		Drop ();
		Send ("P");
		// Assume the next received line is the response and no validation is done.
		// [TODO] Change this.
		ReceiveLine ();
		Drop ();
	}
	#endregion
	
	#region Serial Macros
	/// <summary>
	/// Open a new serial connection. If a connection is present, it will be closed first.
	/// </summary>
	/// <param name="portName">Port name.</param>
	void SerialOpen (string portName) {
		if (portName == activePortName) {
			return;
		} // if
		// else

		// Clean up before opening.
		SerialClose ();

		print (string.Format ("Connecting to serial port {0}.", portName));
		try {
			stream = new SerialPort(portName, 9600); // Set the port name and the baud rate (9600, is standard on most devices)
		} catch (System.IO.IOException) {
			print ("Port not found.");
		}

		if (stream == null) {
			return;
		} // if
		// else

		stream.NewLine = "\n";

		try {
			// http://msdn.microsoft.com/en-us/library/system.io.ports.serialport.open(v=vs.110).aspx
			stream.Open(); // Open the Serial Stream.
		} catch (System.UnauthorizedAccessException) {
			print ("Port access denied.");
		} catch (System.ArgumentOutOfRangeException) {
			print ("Invalid arguments.");
		} catch (System.ArgumentException) {
			print ("Invalid arguments.");
		} catch (System.IO.IOException) {
			print ("Port in invalid state.");
		} catch (System.InvalidOperationException) {
			print ("Port already open. Possibly occupied by another application.");
		}

		if (!isOpen) {
			print ("Releasing port.");
			stream = null;
			return;
		} // if
		// else

		// Flush whatever is not expected.
		/*
		if (bytesToRead > 0) {
			print (stream.ReadExisting ()); // This blocks.
		}
		*/
		stream.DiscardInBuffer ();
		stream.DiscardOutBuffer ();


		int timeout = 5;
		while (timeout > 0) {
			timeout--;
			if (ReceiveLine (timeout: 1000) == MCUHelloWorld) break;
		}
		//! it's possible that the previous code doesn't get the correct response
		if (timeout == 0) {
			print ("Connection timed out.");
		}

		// Send identification command.
		Send ("?");

		if (ReceiveLine (timeout: 300) == MCUHelloWorld) {
			// Connected
			activePortName = portName;
			isBusy = false;
			print ("Connected.");

			// UI changes
			SerialCommandControls.gameObject.SetActive(true);
			NoConnectionWarning.gameObject.SetActive(false);
		} else {
			// Wrong
			print ("Connect protocal dismatch.");
			SerialClose ();
		}
	}
	
	/// <summary>
	/// Closes the serial port if open.
	/// Also resets port reference.
	/// </summary>
	void SerialClose () {
		if (stream != null) {
			if (isOpen) {
				print (string.Format ("Disconnecting serial port {0}.", activePortName));
				try {
					stream.Close(); // Close the Serial Stream.
				} catch (System.Exception) {}
				print ("Disconnected.");
			} // if
			stream = null;
		} // if
		activePortName = "";
		activePortId = -1;
		isBusy = false;

		// UI changes
		SerialCommandControls.gameObject.SetActive(false);
		NoConnectionWarning.gameObject.SetActive(true);
	}

	int bytesToRead {
		get {
			if (stream == null) {
				return 0;
			} // if
			// else

			try {
				return stream.BytesToRead;
			} catch (System.Exception) {
				// BytesToRead is not working on Windows, try returning 1 to make it work.
				return 1;
			}
		}
	}
	bool isOpen {
		get {
			try {
				return stream.IsOpen;
			} catch (System.Exception) {
				return false;
			}
		}
	}
	bool noData {
		get {
			return isConnected && (bytesToRead == 0);
		}
	}
	
	void Drop () {
		try {
			if (stream.BytesToRead > 0) {
				print (stream.ReadExisting ()); // This blocks.
			} // if
		} catch (System.Exception) {}
	}

	/// <summary>
	/// Send the specified command to the microcontroller.
	/// Note: this method will suppress any exceptions.
	/// </summary>
	/// <param name="cmd">Content of the command.</param>
	void Send (string cmd) {
		try {
			stream.WriteLine (cmd);
			print (string.Format ("mcu <<< {0}", cmd));
		} catch (System.Exception) {}
	}

	/// <summary>
	/// Receives a line from the microcontroller.
	/// Note: this method will suppress any exceptions.
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="timeout">Timeout.</param>
	string ReceiveLine (int timeout = SerialPort.InfiniteTimeout) {
		string resp = "Timed out";
		try {
			stream.ReadTimeout = timeout;
			resp = stream.ReadLine ();
			print (string.Format ("mcu >>> {0}", resp));
		} catch (System.Exception) {}
		return resp;
	}
	#endregion
}

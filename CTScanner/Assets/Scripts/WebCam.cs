using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WebCam : MonoBehaviour {

	public bool PauseInactiveDevice = false;


	int activeCamId = -1;
	WebCamTexture activeWebCamTexture;

	WebCamDevice[] cachedDevices;
	List<WebCamTexture> cachedWebCamTextures;
	List<string> cachedDeviceNames;
	
	// Use this for initialization
	void Start () {
		_SavePath = (Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/shots/";
		// Cache device list.
		cachedDevices = WebCamTexture.devices;
		// Cache device names.
		cachedDeviceNames = new List<string> ();
		cachedWebCamTextures = new List<WebCamTexture> ();
		foreach (WebCamDevice cam in cachedDevices) {
			cachedDeviceNames.Add (cam.name);
			WebCamTexture thisWebTexture = new WebCamTexture(cam.name, 400, 300, 12);
			// Activate all cameras on default.
			// This could go really wrong...
			thisWebTexture.Play ();
			if (PauseInactiveDevice) {
				thisWebTexture.Pause ();
			} // if
			cachedWebCamTextures.Add (thisWebTexture);
		}
	}

	// For saving to the _savepath
	private string _SavePath = ""; //Change the path here!
	int _CaptureCounter = 0;

	public int deviceCount {
		get {
			return cachedDevices.Length;
		}
	}

	public string[] deviceNames {
		get {
			return cachedDeviceNames.ToArray ();
		}
	}

	public bool ready {
		get {
			return (activeWebCamTexture != null) && activeWebCamTexture.isPlaying;
		}
	}

	public void TakeSnapshot (string preferredName = "") {
		if (activeWebCamTexture == null) {
			return;
		} // if
		// else

		Texture2D snap = new Texture2D(activeWebCamTexture.width, activeWebCamTexture.height);
		snap.SetPixels(activeWebCamTexture.GetPixels());
		snap.Apply();

		System.IO.Directory.CreateDirectory(_SavePath);

		string filename = _SavePath + ((preferredName == "") ? _CaptureCounter.ToString () : preferredName) + ".png";

		System.IO.File.WriteAllBytes(filename, snap.EncodeToPNG());
		++_CaptureCounter;

		print (string.Format ("Shot saved at {0}.", filename));
	}

	/// <summary>
	/// Switchs the camera feed.
	/// </summary>
	/// <returns>The effective camera feed id.</returns>
	/// <param name="index">Index.</param>
	public int SwitchCameraFeed (int index) {
		if (index < 0 || index >= cachedDevices.Length) {
			return activeCamId;
		} // if
		// else

		if (index == activeCamId) {
			return activeCamId;
		} // if
		// else

		activeCamId = index;

		if (activeWebCamTexture != null && PauseInactiveDevice) {
			activeWebCamTexture.Pause ();
		}

		activeWebCamTexture = cachedWebCamTextures [activeCamId];
		renderer.material.mainTexture = activeWebCamTexture;

		activeWebCamTexture.Play();

		return activeCamId;
	}
}
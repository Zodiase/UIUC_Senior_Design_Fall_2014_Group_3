using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WebCam : MonoBehaviour {

	public bool PauseInactiveDevice = false;
	public Button CaptureButton;
	public Button RescanButton;
	public RectTransform CameraFeedList;
	public Button ListItem_Button_Prefab;

	Texture defaultTexture;

	int activeCamId = -1;
	WebCamTexture activeWebCamTexture;

	WebCamDevice[] cachedDevices;
	List<WebCamTexture> cachedWebCamTextures;
	List<string> cachedDeviceNames;
	
	// Use this for initialization
	void Start () {
		_SavePath = (Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/shots/";

		defaultTexture = renderer.material.mainTexture;

		// Cache device names.
		cachedDeviceNames = new List<string> ();
		cachedWebCamTextures = new List<WebCamTexture> ();

		RescanCameraFeeds ();
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

	public void ClearSnapshots () {
		string[] files = System.IO.Directory.GetFiles (_SavePath);
		foreach (string file in files) {
			System.IO.File.Delete (file);
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

	public void ClearCameraFeedList () {
		DisableCapturing ();

		foreach (Transform child in CameraFeedList.transform) {
			GameObject.Destroy (child.gameObject);
		}

		activeWebCamTexture = null;
		renderer.material.mainTexture = defaultTexture;
		foreach (WebCamTexture webTexture in cachedWebCamTextures) {
			webTexture.Stop ();
		}
		cachedWebCamTextures.Clear ();
		cachedDeviceNames.Clear ();
		activeCamId = -1;
	}

	public void RescanCameraFeeds () {
		RescanButton.interactable = false;

		// Resetting.
		ClearCameraFeedList ();
		// Cache device list.
		cachedDevices = WebCamTexture.devices;

		foreach (WebCamDevice cam in cachedDevices) {
			int thisCamId = cachedDeviceNames.Count;

			// Create the button for selecting this camera feed.
			Button thisCameraFeedButton = Button.Instantiate (ListItem_Button_Prefab) as Button;
			// Add the button to the camera feed list.
			RectTransform thisCameraFeedButtonRect = thisCameraFeedButton.GetComponent<RectTransform> ();
			thisCameraFeedButtonRect.SetParent(CameraFeedList);
			thisCameraFeedButtonRect.localScale = new Vector3 (1, 1, 1);
			// Change button text.
			thisCameraFeedButton.GetComponentInChildren<Text> ().text = cam.name;
			// Set button behaviour.
			thisCameraFeedButton.onClick.AddListener (() => {
				ColorBlock normalColorBlock = ListItem_Button_Prefab.colors;
				foreach (Button button in CameraFeedList.GetComponentsInChildren<Button> ()) {
					button.colors = normalColorBlock;
				}
				SwitchCameraFeed (thisCamId);
				ColorBlock pressedColorBlock = ListItem_Button_Prefab.colors;
				pressedColorBlock.normalColor = pressedColorBlock.highlightedColor = pressedColorBlock.pressedColor;
				thisCameraFeedButton.colors = pressedColorBlock;
			});

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
		//LayoutRebuilder.MarkLayoutForRebuild (CameraFeedList); // Not needed.
		print ("Camera feeds updated.");

		RescanButton.interactable = true;
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

		renderer.material.mainTexture = defaultTexture;
		if (activeWebCamTexture != null && PauseInactiveDevice) {
			activeWebCamTexture.Pause ();
			DisableCapturing ();
		}

		activeWebCamTexture = cachedWebCamTextures [activeCamId];
		renderer.material.mainTexture = activeWebCamTexture;

		activeWebCamTexture.Play();
		EnableCapturing ();

		return activeCamId;
	}

	private void EnableCapturing () {
		if (CaptureButton != null) {
			CaptureButton.interactable = true;
		}
	}
	private void DisableCapturing () {
		if (CaptureButton != null) {
			CaptureButton.interactable = false;
		}
	}
}
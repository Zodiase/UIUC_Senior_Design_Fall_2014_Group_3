using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class Logger : MonoBehaviour {

	public ScrollRect scrollRect;
	Text text;

	// Use this for initialization
	void Start () {
		text = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void LateUpdate () {

	}

	void OnEnable () {
		Application.RegisterLogCallback (HandleLog);
	}
	
	void OnDisable () {
		// Remove callback when object goes out of scope
		Application.RegisterLogCallback(null);
	}

	void HandleLog (string logString, string stackTrace, LogType type) {
		text.text += "\n" + logString;
		if (type != LogType.Log) {
			text.text += "\n" + stackTrace;
		}
		StartCoroutine (updateScrollPositionNextFrame ());
	}

	IEnumerator updateScrollPositionNextFrame () {
		yield return new WaitForEndOfFrame ();
		//yield return new WaitForEndOfFrame ();
		scrollRect.verticalNormalizedPosition = 0;
		yield break;
	}

	public void Clear () {
		text.text = "";
	}
}

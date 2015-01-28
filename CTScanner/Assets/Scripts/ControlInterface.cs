using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlInterface : MonoBehaviour {

	public Tasker tk;
	public MCU mc;
	public WebCam wc;

	public Slider ShoutsCountSlider;
	public Text ShoutsCountText;

	int shotsCount = 20;

	// Use this for initialization
	void Start () {
		ShoutsCountText.text = shotsCount.ToString ();
		ShoutsCountSlider.value = (float) shotsCount;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShotsCountSliderOnChange (float value) {
		shotsCount = Mathf.FloorToInt (value);
		ShoutsCountText.text = shotsCount.ToString ();
	}
	public void FetchObjectFacing () {
		print(mc.GetBoxFace ());
	}
	public void FetchPlatformAngle () {
		print (mc.GetServoAngle ());
	}
	public void StartScanButtonClicked () {
		mc.PerformSuit (shotsCount);
	}
}

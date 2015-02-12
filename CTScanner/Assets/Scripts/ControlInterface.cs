using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlInterface : MonoBehaviour {

	public Tasker tk;
	public MCU mc;
	public WebCam wc;

	public Slider ShoutsCountSlider;
	public Text ShoutsCountText;

	int shotsCount = -1;
	// This array is used to map slider values to shots count values
	int[] shotsCountSet = new int[] {
		4,
		5,
		10,
		20,
		25,
		50,
		100
	};

	int sliderMinValue {
		get {
			return 0;
		}
	}
	int sliderMaxValue {
		get {
			return shotsCountSet.Length - 1;
		}
	}

	// Use this for initialization
	void Start () {
		ShoutsCountSlider.maxValue = sliderMaxValue;
		ShoutsCountSlider.minValue = sliderMinValue;
		ShoutsCountSlider.value = Mathf.RoundToInt ((sliderMaxValue + sliderMinValue) / 2);
		ShotsCountSliderOnChange (ShoutsCountSlider.value);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShotsCountSliderOnChange (float value) {
		int sliderIntegralValue = Mathf.FloorToInt (value);
		sliderIntegralValue = Mathf.Clamp (sliderIntegralValue, sliderMinValue, sliderMaxValue);
		shotsCount = shotsCountSet [sliderIntegralValue];
		ShoutsCountText.text = shotsCount.ToString ();
	}
	public void FetchObjectFacing () {
		mc.TestFaceScanning ();
	}
	public void FetchPlatformAngle () {
		print (mc.GetServoAngle ());
	}
	public void StartScanButtonClicked () {
		mc.PerformSuit (shotsCount);
	}
}

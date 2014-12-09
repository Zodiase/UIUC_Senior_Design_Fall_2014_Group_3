using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tasker : MonoBehaviour {

	public Modeller mod;

	public TextAsset particles;
	List<Vector3> particleList;

	// Use this for initialization
	void Start () {
		particleList = new List<Vector3> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BuildTestModel () {
		string[] points = particles.text.Split ('\n');
		particleList.Clear ();
		for (int i = 0, n = points.Length; i < n; i++) {
			string[] values = points[i].Split (',');
			if (values.Length < 3) continue;
			particleList.Add (new Vector3 (float.Parse (values[0]), float.Parse (values[1]), float.Parse (values[2])));
		}
		mod.Build (particleList);
	}
	public void ExitApp () {
		Application.Quit ();
	}
}

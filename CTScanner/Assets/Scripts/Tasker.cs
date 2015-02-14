using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tasker : MonoBehaviour {

	public Modeller mod;

	public TextAsset particles;
	List<Vector3> particleList;

	public float normalizedRadius;

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
			string[] values = points[i].Contains (",") ? points[i].Split (',') : points[i].Split (' ');
			if (values.Length < 3) continue;
			float x = float.Parse (values[0]);
			float y = float.Parse (values[1]);
			float z = float.Parse (values[2]);

			particleList.Add (new Vector3 (x, y, z));
		}
		mod.Build (particleList);
	}
	public void ExitApp () {
		Application.Quit ();
	}
}

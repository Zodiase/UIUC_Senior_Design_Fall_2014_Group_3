using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Tasker : MonoBehaviour {

	public Modeller mod;

	public TextAsset particles;
	List<Vector3> particleList;

	public float normalizedRadius;

	private string dataFileContent = "";
	private string _ModelDataFileDir = "";
	private string _ModelDataFilePath = "";
	private DateTime _ModelDataFileLastUpdateTime;

	// Use this for initialization
	void Start () {
		particleList = new List<Vector3> ();

		_ModelDataFileDir = (Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/data";
		if (!System.IO.Directory.Exists (_ModelDataFileDir)) {
			System.IO.Directory.CreateDirectory (_ModelDataFileDir);
		}
		_ModelDataFilePath = _ModelDataFileDir + "/output.txt";

		ResetTestModelData ();
		dataFileContent = particles.text;
	}

	private int fileCheckTimer = 0;
	void Update () {
		fileCheckTimer++;
		if (fileCheckTimer > 10) {
			fileCheckTimer = 0;
			DateTime checkTime = System.IO.File.GetLastWriteTimeUtc (_ModelDataFilePath);
			if (checkTime.CompareTo (_ModelDataFileLastUpdateTime) > 0) {
				// Data file got update.
				// Read the file.
				dataFileContent = System.IO.File.ReadAllText (_ModelDataFilePath);
				BuildTestModel ();
				_ModelDataFileLastUpdateTime = checkTime;
			}
		}
	}

	public void ResetTestModelData () {
		System.IO.File.WriteAllText (_ModelDataFilePath, "");
		_ModelDataFileLastUpdateTime = System.IO.File.GetLastWriteTimeUtc (_ModelDataFilePath);
		dataFileContent = "";
	}

	public void BuildTestModel () {
		string[] points = dataFileContent.Split ('\n');

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

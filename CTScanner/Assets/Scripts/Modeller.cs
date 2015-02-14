using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using iCradleNet.Rebuild.Pooling;

[RequireComponent (typeof (ParticleSystem))]
public class Modeller : MonoBehaviour {

	public Color particleColor = Color.white;
	public float particleSize = 1;
	public float normalizedRadius = 30;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Clear () {
		particleSystem.Clear ();
	}

	public void Build (List<Vector3> coords) {
		Clear ();
		ParticleSystem.Particle[] cloud = new ParticleSystem.Particle[coords.Count];

		// Max and min values are used to normalize coordinates
		float maxVal_x, maxVal_y, maxVal_z;
		maxVal_x = maxVal_y = maxVal_z = Mathf.NegativeInfinity;
		
		float minVal_x, minVal_y, minVal_z;
		minVal_x = minVal_y = minVal_z = Mathf.Infinity;

		coords.ForEach ((Vector3 coord) => {
			maxVal_x = Mathf.Max (maxVal_x, coord.x);
			maxVal_y = Mathf.Max (maxVal_y, coord.y);
			maxVal_z = Mathf.Max (maxVal_z, coord.z);
			
			minVal_x = Mathf.Min (minVal_x, coord.x);
			minVal_y = Mathf.Min (minVal_y, coord.y);
			minVal_z = Mathf.Min (minVal_z, coord.z);
		});

		// Use max and min values to normalize coordinates
		float midVal_x, midVal_y, midVal_z;
		midVal_x = (maxVal_x + minVal_x) * 0.5f;
		midVal_y = (maxVal_y + minVal_y) * 0.5f;
		midVal_z = (maxVal_z + minVal_z) * 0.5f;
		
		float radius_x, radius_y, radius_z;
		radius_x = maxVal_x - midVal_x;
		radius_y = maxVal_y - midVal_y;
		radius_z = maxVal_z - midVal_z;
		
		float overRadius_x, overRadius_y, overRadius_z;
		overRadius_x = 1 / radius_x;
		overRadius_y = 1 / radius_y;
		overRadius_z = 1 / radius_z;

		for (int i = 0, n = coords.Count; i < n; i++) {
			float x = (coords[i].x - midVal_x) * overRadius_x * normalizedRadius;
			float y = (coords[i].y - midVal_y) * overRadius_y * normalizedRadius;
			float z = (coords[i].z - midVal_z) * overRadius_z * normalizedRadius;

			cloud[i].position = new Vector3 (x, y, z);
			cloud[i].color    = particleColor;
			cloud[i].size     = particleSize;
		}
		particleSystem.SetParticles (cloud, cloud.Length);
	}
}

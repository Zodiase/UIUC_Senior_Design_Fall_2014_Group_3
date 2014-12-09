using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using iCradleNet.Rebuild.Pooling;

public class Modeller : MonoBehaviour {

	public GameObject particleSampleGO;
	GameObjectPool particlePool;
	List<KeyValuePair<int, GameObject>> particleList;

	// Use this for initialization
	void Start () {
		if (particleSampleGO == null) {
			throw new UnityException ("Particle sample game object not assigned.");
		} // if
		// else
		particlePool = new GameObjectPool (particleSampleGO);
		particleList = new List<KeyValuePair<int, GameObject>> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Clear () {
		particleList.ForEach (ReturnParticle);
		particleList.Clear ();
	}

	void ReturnParticle (KeyValuePair<int, GameObject> particleRecord) {
		particlePool.Return (particleRecord);
	}

	public void Build (List<Vector3> coords) {
		Clear ();
		coords.ForEach (AddParticle);
	}
	void AddParticle (Vector3 coord) {
		KeyValuePair<int, GameObject> particleRecord = particlePool.Borrow (autoGrow: true);
		particleList.Add (particleRecord);
		particleRecord.Value.transform.parent = this.transform;
		particleRecord.Value.transform.localPosition = coord;
	}
}

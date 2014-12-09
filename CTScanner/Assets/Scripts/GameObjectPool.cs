using UnityEngine;
using System;
using System.Collections.Generic;

namespace iCradleNet.Rebuild.Pooling {
	public class GameObjectPool : GenericPool<GameObject> {
		private GameObject _sampleGO = default(GameObject);
		private GameObject _poolGO = default(GameObject);
		public GameObject gameObject {
			get {
				return _poolGO;
			}
			private set {}
		}

		private static int _poolCount = 0;

		public GameObjectPool (GameObject sampleGO) : base() {
			_sampleGO = sampleGO;
			_poolGO = new GameObject ();
			_poolGO.name = string.Format ("GameObjectPool{0}",_poolCount);
			_poolCount++;
		}

		override public KeyValuePair<int, GameObject> Borrow (bool autoGrow) {
			KeyValuePair<int, GameObject> result = base.Borrow (autoGrow);
			if (result.Value != null) {
				result.Value.SetActive (true);
			}
			return result;
		}

		override public void Return (KeyValuePair<int, GameObject> item) {
			GameObject gO = item.Value;
			if (gO != null) {
				gO.SetActive (false);
				gO.transform.parent = _poolGO.transform;
			}

			base.Return (item);
		}

		override protected GameObject Instantiate () {
			GameObject newInstance = MonoBehaviour.Instantiate(_sampleGO) as GameObject;
			newInstance.SetActive (false);
			newInstance.transform.parent = _poolGO.transform;
			return newInstance;
		}
	}
}

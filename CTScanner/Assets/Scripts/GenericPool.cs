using System;
using System.Collections.Generic;

namespace iCradleNet.Rebuild.Pooling {
	public abstract class GenericPool<Type> {
		private class GenericPoolRecord  {
			public bool available = false;
			public Type value = default(Type);
		}

		private List<GenericPoolRecord> _pool = null;
		private int _availableCount = 0;
		private int _findIndex = 0;
		public int Size {
			get {
				return _pool.Count;
			}
			private set {}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="GenericPool"/> class.
		/// </summary>
		public GenericPool () {
			_pool = new List<GenericPoolRecord> ();
			_availableCount = 0;
			_findIndex = 0;
		}
		
		/// <summary>
		/// Clear the pool.
		/// </summary>
		public void Clear () {
			_pool.Clear ();
			_availableCount = 0;
			_findIndex = 0;
		}
		
		/// <summary>
		/// Fills the pool if the poolSize is larger than the current size.
		/// </summary>
		/// <param name="poolSize">Pool size.</param>
		public int Fill (int poolSize) {
			return Fill (poolSize, false);
		}
		/// <summary>
		/// Fill the pool. Clear the pool first if needed.
		/// </summary>
		/// <param name="poolSize">Pool size.</param>
		/// <param name="clear">If set to <c>true</c> clear.</param>
		public int Fill (int poolSize, bool clear) {
			if (poolSize < 0) {
				return 0;
			}
			// else
			if (clear) {
				Clear ();
			}
			if (poolSize < _pool.Count) {
				return 0;
			}
			// else
			int expandAmount = ((_pool.Count == 0) ? poolSize : (poolSize - _pool.Count));
			return Expand (expandAmount);
		}
		
		public int Expand (int size) {
			if (size <= 0) {
				return 0;
			}
			// else
			for (int i = 0; i < size; i++) {
				GenericPoolRecord newRecord = new GenericPoolRecord ();
				newRecord.available = true;
				newRecord.value = Instantiate ();
				_pool.Add (newRecord);
			}
			_availableCount += size;
			return size;
		}
		
		public int Shrink () {
			if (_pool.Count == 0) {
				return 0;
			}
			// else
			int endIndex = _pool.Count;
			do {
				endIndex--;
			} while (endIndex >= 0 && _pool[endIndex].available);
			endIndex++;
			int shrinkedCount = _pool.Count - endIndex;
			if (shrinkedCount > 0) {
				List<GenericPoolRecord> newPool = new List<GenericPoolRecord> ();
				for (int i = 0; i < endIndex; i++) {
					newPool.Add (_pool[i]);
				}
				_pool = newPool;
			}
			return shrinkedCount;
		}
		
		public KeyValuePair<int, Type> Borrow () {
			return Borrow (false);
		}
		public virtual KeyValuePair<int, Type> Borrow (bool autoGrow) {
			int lookupIndex;
			int resultKey = -1;
			Type resultValue = default(Type);
			if (_availableCount == 0) {
				if (autoGrow) {
					Expand (1); // This step will increase _availableCount by 1;
					lookupIndex = _pool.Count - 1;
					
					GenericPoolRecord record = _pool[lookupIndex];
					record.available = false;
					
					resultKey = lookupIndex;
					resultValue = record.value;
					
					_availableCount--;
					_findIndex = 0;
				}
			} else {
				for (int i = 0, n = _pool.Count; i < n; i++) {
					lookupIndex = (_findIndex + i) % n;
					GenericPoolRecord record = _pool[lookupIndex];
					if (record.available) {
						record.available = false;
						
						resultKey = (int)lookupIndex;
						resultValue = record.value;
						
						_availableCount--;
						_findIndex = (lookupIndex + 1) % n;
						
						break;
					}
				}
			}
			return new KeyValuePair<int, Type>(resultKey, resultValue);
		}
		
		public virtual void Return (KeyValuePair<int, Type> item) {
			int index = item.Key;
			Type value = item.Value;
			if (index < 0 || index >= _pool.Count) {
				return;
			}
			// else
			GenericPoolRecord record = _pool[index];
			if (record.available || !record.value.Equals (value)) {
				return;
			}
			// else
			record.available = true;
			_availableCount++;
		}
		
		protected abstract Type Instantiate ();
	}
	

}

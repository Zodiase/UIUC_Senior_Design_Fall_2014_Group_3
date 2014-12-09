using UnityEngine;
using System;
using System.Collections.Generic;

namespace iCradleNet {
	public class State<StateType> {

		private Stack<StateType> _stateStack;

		public enum ActionType {
			BeforeEntering,
			AfterEntering,
			BeforeExiting,
			AfterExiting,
		}

		private Dictionary<ActionType ,Dictionary<StateType, List<Action>>> _actions;

		private Dictionary<StateType, List<Action>> _actionsBeforeEnteringState;
		private Dictionary<StateType, List<Action>> _actionsAfterEnteringState;

		private Dictionary<StateType, List<Action>> _actionsBeforeExitingState;
		private Dictionary<StateType, List<Action>> _actionsAfterExitingState;

		public State (StateType initialState) {
			_stateStack = new Stack<StateType> ();
			_stateStack.Push (initialState);
			_actions = new Dictionary<ActionType, Dictionary<StateType, List<Action>>> ();
			_actions[ActionType.BeforeEntering] = new Dictionary<StateType, List<Action>> ();
			_actions[ActionType.AfterEntering] = new Dictionary<StateType, List<Action>> ();
			_actions[ActionType.BeforeExiting] = new Dictionary<StateType, List<Action>> ();
			_actions[ActionType.AfterExiting] = new Dictionary<StateType, List<Action>> ();
		}

		public int Length {
			get {
				return (_stateStack == null) ? 0 : _stateStack.Count;
			}
		}

		public void AddAction (ActionType type, StateType state, Action action) {
			Dictionary<StateType, List<Action>> actions = _actions[type];

			if (!actions.ContainsKey (state)) {
				actions.Add (state, new List<Action> ());
			} // if

			actions[state].Add (action);
		}
		public void AddAction (StateType state, Action action) {
			AddAction (ActionType.AfterEntering, state, action);
		}
		public void AddAction (StateType state, Action enterAction, Action exitAction) {
			AddAction (ActionType.AfterEntering, state, enterAction);
			AddAction (ActionType.AfterExiting, state, exitAction);
		}

		private void TriggerActions (ActionType type, StateType state) {
			Dictionary<StateType, List<Action>> actions = _actions[type];

			if (actions.ContainsKey (state)) {
				actions[state].ForEach (TriggerAction);
			} // if
		}

		private void TriggerAction (Action action) {
			action ();
		}

		#region Operator Overrides
		/// <summary>
		/// Determines whether the specified <see cref="`0 (owner=iCradleNet.State`1)"/> is equal to the current <see cref="iCradleNet.State`1"/>.
		/// </summary>
		/// <param name="someState">The <see cref="`0 (owner=iCradleNet.State`1)"/> to compare with the current <see cref="iCradleNet.State`1"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="`0 (owner=iCradleNet.State`1)"/> is equal to the current
		/// <see cref="iCradleNet.State`1"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (StateType someState) {
			if (_stateStack == null) {
				return false;
			} // if
			// else
			
			if (_stateStack.Count == 0) {
				return false;
			} // if
			// else
			
			return (_stateStack.Peek ().Equals (someState));
		}
		/// <param name="stateObject1">State object1.</param>
		/// <param name="stateObject2">State object2.</param>
		public static bool operator ==(State<StateType> stateObject1, State<StateType> stateObject2) {
			// States can't be compared with each other.
			return false;
		}
		/// <param name="stateObject1">State object1.</param>
		/// <param name="stateObject2">State object2.</param>
		public static bool operator !=(State<StateType> stateObject1, State<StateType> stateObject2) {
			return !(stateObject1 == stateObject2);
		}
		/// <param name="stateObject">State object.</param>
		/// <param name="someState">Some state.</param>
		public static bool operator ==(State<StateType> stateObject, StateType someState) {
			if (stateObject == null) {
				return false;
			} // if
			// else

			return (stateObject.Equals (someState));
		}
		/// <param name="stateObject">State object.</param>
		/// <param name="someState">Some state.</param>
		public static bool operator !=(State<StateType> stateObject, StateType someState) {
			return !(stateObject == someState);
		}
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="iCradleNet.State`1"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="iCradleNet.State`1"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="iCradleNet.State`1"/>;
		/// otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj) {
			try {
				return (this == (State<StateType>) obj);
			} catch {
				return false;
			}
		}
		/// <summary>
		/// Serves as a hash function for a <see cref="iCradleNet.State`1"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode () {
			return base.GetHashCode ();
		}
		/// The plus operator is used to enter a state.
		/// <param name="stateObject">State object.</param>
		/// <param name="someState">Some state.</param>
		public static State<StateType> operator +(State<StateType> stateObject, StateType someState) {
			if (stateObject == null) {
				return new State<StateType> (someState);
			} // if
			// else

			if ((stateObject._stateStack.Count == 0) || (!stateObject._stateStack.Peek ().Equals (someState))) {

				stateObject.TriggerActions (ActionType.BeforeEntering, someState);
				stateObject._stateStack.Push (someState);
				stateObject.TriggerActions (ActionType.AfterEntering, someState);

			} // if

			return stateObject;
		}
		/// The minus operator is used to exit a state.
		/// <param name="stateObject">State object.</param>
		/// <param name="someState">Some state.</param>
		public static State<StateType> operator -(State<StateType> stateObject, StateType someState) {
			if (stateObject == null) {
				return null;
			} // if
			// else

			if ((stateObject._stateStack.Count > 0) && (stateObject._stateStack.Peek ().Equals (someState))) {
				
				StateType oldState = stateObject._stateStack.Peek ();
				stateObject.TriggerActions (ActionType.BeforeExiting, oldState);
				stateObject._stateStack.Pop ();
				stateObject.TriggerActions (ActionType.AfterExiting, oldState);

			} // if

			return stateObject;
		}
		/// The multiply operator is used to switch to a state.
		/// <param name="stateObject">State object.</param>
		/// <param name="someState">Some state.</param>
		public static State<StateType> operator *(State<StateType> stateObject, StateType someState) {
			if (stateObject == null) {
				return new State<StateType> (someState);
			} // if
			// else

			/*
			if ((stateObject._stateStack.Count > 0) && (!stateObject._stateStack.Peek ().Equals (someState))) {
				stateObject -= someState;
			}
			*/

			if (stateObject._stateStack.Count == 0) {
				
				stateObject += someState;

			} else if (!stateObject._stateStack.Peek ().Equals (someState)) {

				StateType oldState = stateObject._stateStack.Peek ();
				stateObject -= oldState;
				stateObject += someState;

			} // if
			
			return stateObject;
		}
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="iCradleNet.State`1"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="iCradleNet.State`1"/>.</returns>
		public override string ToString () {
			if (_stateStack == null) {
				return "";
			} // if
			// else
			
			if (_stateStack.Count == 0) {
				return "";
			} // if
			// else

			return _stateStack.Peek ().ToString ();
		}
		#endregion
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PMP.Extension
{
	public class ButtonFSM : MonoBehaviour {
		public SimpleFSM mFSM;
		private void Awake() {
			mFSM = new SimpleFSM();
			var list = GetComponents<IState>();
			foreach (var state in list)
			{
				mFSM.AddState(state);
			}
			if(list[0] != null)
				mFSM.CurrentState = list[0];
		}
		private void Update() {
			// mFSM.Update();
		}

		private void OnDestroy() {
			mFSM.Dispose();
		}
	}
}

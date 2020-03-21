using System.Collections;
using System.Collections.Generic;

namespace PMP.Extension{
    public class SimpleFSM : BFSM
    {
		private Dictionary<string,IState> mStateList = new Dictionary<string, IState>();
		public IState CurrentState{
			get
			{
				return mCurrentState;
			}
			set
			{
				ChangeState(value);
			}
		}
		public SimpleFSM(){}

		public void AddState(IState state)
		{
			if(mStateList.ContainsKey(state.Name))
				return;
			mStateList.Add(state.Name,state);
		}

		public void RemoveState(IState state)
		{
			if(mStateList.ContainsKey(state.Name))
			{
				var s = mStateList[state.Name];
				if(mCurrentState != null && mCurrentState.Equals(s))
					mCurrentState = null;
				mStateList.Remove(state.Name);
			}
		}

        public override void ChangeState(IState state)
        {
			if(state == null)
				return;
			if(!mStateList.ContainsValue(state))
				mStateList.Add(state.Name,state);
			Change(state);
        }

        public override void ChangeState(string stateName)
        {
			var state = GetState(stateName);
			if(state == null)
			{
				return;
			}
			Change(state);
        }

		private void Change(IState state)
		{
			if(mCurrentState == null)
			{
				mCurrentState = state;
				mCurrentState.Enter();
				return;
			}
			if(mCurrentState.Equals(state))
				return;
			mCurrentState.Exit();
			mCurrentState = state;
			mCurrentState.Enter();
		}

		public IState GetState(string stateName)
		{
			return mStateList[stateName];
		}


        public override void Dispose()
        {
           mStateList.Clear();
        }

        public override void Update()
        {
            mCurrentState.Update();
        }


    }
}

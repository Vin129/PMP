using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PMP.Extension
{
    public class OnClickState : MonoBehaviour,IState
    {
        public string Name
        {
            get
            {
                return "OnClick";
            } 
        }
        private static SimpleFSM FSM;
        
        public UnityEngine.Events.UnityAction Click = ()=>{
            FSM.ChangeState("Interactable");
        };
        public void Enter()
        {
            UnityEngine.Debug.LogFormat("Enter:{0}",Name);
            FSM = gameObject.GetComponent<ButtonFSM>().mFSM;
            gameObject.GetComponent<Button>().onClick.AddListener(Click);
        }

        public bool Equals(IState state)
        {
            return Name == state.Name;
        }

        public void Exit()
        {
            UnityEngine.Debug.LogFormat("Exit:{0}",Name);
            gameObject.GetComponent<Button>().onClick.RemoveListener(Click);
        }

        public IState GetState()
        {
            return this;
        }

        void IState.Update()
        {
            UnityEngine.Debug.LogFormat("Update:{0}",Name);
        }
    }
}

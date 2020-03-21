using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PMP.Extension
{
    public class InteractableState : MonoBehaviour,IState
    {
		public bool Interactable = true;
        public string Name
        {
            get
            {
                return "Interactable";
            } 
        }
        public void Enter()
        {
            UnityEngine.Debug.LogFormat("Enter:{0}",Name);
			gameObject.SetActive(Interactable);
			gameObject.GetComponent<Button>().interactable = Interactable;
        }

        public bool Equals(IState state)
        {
            return Name == state.Name;
        }

        public void Exit()
        {
            UnityEngine.Debug.LogFormat("Exit:{0}",Name);
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

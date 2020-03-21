using System;
using System.Collections.Generic;
namespace PMP.Extension{
    public abstract class BFSM :IDisposable
    {
        protected IState mCurrentState;
        public abstract void ChangeState(IState state);
        public abstract void ChangeState(string state);
        public abstract void Update();
        public abstract void Dispose();
    }
}

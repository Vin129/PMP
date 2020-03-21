
namespace PMP.Extension{
	public interface  IState 
	{
		string Name{get;}
		void Enter();
		void Update();
		void Exit();
		bool Equals(IState state);
		IState GetState();
	}
}

using System.Collections;
using System.Collections.Generic;
namespace PMP.Extension{
	public interface IFSM  
	{
		IState mCurrentState{get;set;}
        List<IState> mStateList{get;set;}
        Dictionary<IState,Dictionary<string,IState>> TranslationDict{get;set;}
	}
}

using PMP.Extension;
namespace LuaKit {
	public class BaseOutlet : Singleton<BaseOutlet>
	{
		private PlugCollector mPlugCtor;
		protected BaseOutlet()
		{

		}
		public override void OnSingletonInit(){
			mPlugCtor = new PlugCollector();
		}

		public void Init(){
			mPlugCtor.InitPlug();
		}
	}
}

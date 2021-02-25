DoTween 主要流程：通过静态扩展方法/DOTween.To 创建TweenerCore,执行SetUp(设置值与Plugin)并激活(AddActiveTween(t))通过DOTweenComponent中三种Update进行驱动(Plugin.EvaluateAndApply)完成动画效果。

其中关键方法通过Return TweenerCore 完成链式结构。
驱动还是 DOTweenComponent : MonoBehaviour 中三项Update

DOTweenComponent 在初次使用时进行初始化
const int _DefaultMaxTweeners = 200;
const int _DefaultMaxSequences = 50;

TweenerCore:
        public DOGetter<T1> getter; //单向获取委托（外部为了安全使用协变 out T,防止T的写入）
        public DOSetter<T1> setter;//单向写入委托（外部为了安全使用协变 in T,防止T的获取）
        internal ABSTweenPlugin<T1, T2, TPlugOptions> tweenPlugin;//实现具体动画细节的插件模块（主要不同来自这里） Update 最终也是执行该插件的 EvaluateAndApply 方法实现过渡。（from：internal static ABSTweenPlugin<T1,T2,TPlugOptions> GetDefaultPlugin<T1,T2,TPlugOptions>() where TPlugOptions : struct）

Sequence:
    TODO
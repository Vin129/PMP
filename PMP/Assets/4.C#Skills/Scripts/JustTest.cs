using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustTest : MonoBehaviour
{
    int[] a;
    Dictionary<int,int> b;
    List<int> c;
    Queue<int> d;
    Stack<int> e;
    LinkedList<int> f;
    public abstract class ClassA
    {
        public ClassA()
        {
            Debug.LogError("ClassA");
        }

        public virtual void Test()
        {
            Debug.LogError("TestA");
        }

        public virtual void Fun()
        {
            Debug.LogError("FunA");
        }
    }

    public class ClassB:ClassA
    {
        public ClassB()
        {
            Debug.LogError("ClassB");
        }

        public new void Test()
        {
            Debug.LogError("TestB");
        }

        public override void Fun()
        {
            Debug.LogError("FunB");
        }
    }


    // Start is called before the first frame update
    void Start()
    {       
        Print100(1);
    }
    //无循环无if输出1~100
    public bool Print100(int i)
    {
        Debug.Log(i);
        return (i<100)&&Print100(++i);
    }

    private void FixedUpdate() {
        
    }

    private void OnDestroy() {
        
    }
    
    private void OnDisable() {
        
    }
}

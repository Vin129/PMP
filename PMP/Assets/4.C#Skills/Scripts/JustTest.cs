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
       
        ClassA a = new ClassB();
        a.Test();
        a.Fun();

    }

    private void FixedUpdate() {
        
    }

    private void OnDestroy() {
        
    }
    
    private void OnDisable() {
        
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;
    private Stack<System.Action> backStack = new Stack<System.Action>();
    private Stack<System.Action> forwardStack = new Stack<System.Action>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------- REGISTER STATE --------

    public void PushState(System.Action backAction)
    {
        backStack.Push(backAction);
        forwardStack.Clear();
    }

    // -------- BACK --------

    public void GoBack()
    {
        if (backStack.Count == 0) return;

        var action = backStack.Pop();
        action.Invoke();
        forwardStack.Push(action);
    }

    // -------- FORWARD --------

    public void GoForward()
    {
        if (forwardStack.Count == 0) return;

        var action = forwardStack.Pop();
        action.Invoke();
        backStack.Push(action);
    }
}

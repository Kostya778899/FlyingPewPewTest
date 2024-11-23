using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BehaviourExtensions
{
    public static IEnumerator InvokeCoroutine<T>(
        this T behaviour, float time, Action<T> action) //where T : Behaviour
    {
        yield return new WaitForSeconds(time);
        action.Invoke(behaviour);
    }
    public static Coroutine Invoke<T>(
        this T behaviour, float time, Action<T> action) where T : MonoBehaviour =>
        behaviour.StartCoroutine(behaviour.InvokeCoroutine(time, action));
}

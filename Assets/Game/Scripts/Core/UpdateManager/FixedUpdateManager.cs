using System; 
using System.Collections.Generic; 
using UnityEngine;

public class FixedUpdateManager : MonoBehaviour, IFixedUpdateManager
{
    public void Register(Action<float> cb){ if(!_subs.Contains(cb)) _subs.Add(cb);}

    public void Unregister(Action<float> cb)=>_subs.Remove(cb);
    private readonly List<Action<float>> _subs = new();

    private void FixedUpdate()
    {

        float dt = Time.fixedDeltaTime; // physics step

        for (int i=0, n=_subs.Count; i<n; i++) _subs[i]?.Invoke(dt);

    }
}


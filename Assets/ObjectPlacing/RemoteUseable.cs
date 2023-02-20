using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;


public class RemoteUseable : MonoBehaviour, IUseable
{
    public delegate void Use(Hand controller);
    public event Use OnUse;
    public event Use OnRelease;

    void IUseable.Use(Hand controller)
    {
        OnUse?.Invoke(controller);
    }

    void IUseable.UnUse(Hand controller)
    {
        OnRelease?.Invoke(controller);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;


public class RemoteGraspable : MonoBehaviour, IGraspable
{
    public delegate void Grasp(Hand controller);
    public event Grasp OnGrasp;
    public event Grasp OnRelease;

    void IGraspable.Grasp(Hand controller)
    {
        OnGrasp?.Invoke(controller);
    }

    void IGraspable.Release(Hand controller)
    {
        OnRelease?.Invoke(controller);
    }
}

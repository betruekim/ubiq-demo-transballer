using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;

namespace PlacableObjects
{
    // TODO: one day make this an abstract class so that pipes with functionality inherit
    public abstract class Placable : MonoBehaviour, INetworkComponent, INetworkObject
    {
        NetworkId INetworkObject.Id => new NetworkId();

        public abstract void ProcessMessage(ReferenceCountedSceneGraphMessage message);

        public Transform snapA, snapB;
        public List<Transform> snappedTo;

        protected virtual void Awake()
        {
            snappedTo = new List<Transform>();
            snapA = transform.GetChild(1);
            snapB = transform.GetChild(2);
        }
    }
}

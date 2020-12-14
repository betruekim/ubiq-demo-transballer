using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;

namespace Transballer.PlaceableObjects
{
    public class TrackEnd : MonoBehaviour, IUseable
    {
        public enum EndType { stop, bounce };
        public EndType type;

        Renderer stopRenderer;

        Dictionary<EndType, Color> colorSettings = new Dictionary<EndType, Color> {
            {EndType.stop,Color.white},
            {EndType.bounce, Color.green}
        };

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("TrackEnd");
            stopRenderer = transform.parent.GetComponent<MeshRenderer>();
            stopRenderer.material.color = colorSettings[type];
        }

        void IUseable.Use(Hand controller)
        {
            type = (EndType)(((int)type + 1) % 2);
            stopRenderer.material.color = colorSettings[type];
        }

        void IUseable.UnUse(Hand controller)
        {

        }
    }
}

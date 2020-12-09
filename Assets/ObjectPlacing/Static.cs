using Ubik.Messaging;
using UnityEngine;

namespace PlacableObjects
{
    public class Static : Placable
    {
        public override int materialCost => 10; // TODO, split every static object up to have different materialCost
    }
}
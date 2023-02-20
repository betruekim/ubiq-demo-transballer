using Ubiq.Messaging;
using UnityEngine;

namespace Transballer.PlaceableObjects
{
    public class Static : Placeable
    {
        public override int materialCost => 10; // TODO, split every static object up to have different materialCost
    }
}
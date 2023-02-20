using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

namespace Transballer.PlaceableObjects
{
    public static class PlaceableIndex
    {
        public static Dictionary<NetworkId, Placeable> placedObjects = new Dictionary<NetworkId, Placeable>();

        public static void AddPlacedObject(Placeable placeable)
        {
            if (placedObjects.ContainsKey(placeable.NetworkId))
            {
                throw new System.Exception($"placed id {placeable.NetworkId} already exists");
            }
            placedObjects[placeable.NetworkId] = placeable;
        }

        public static void RemovePlacedObject(Placeable placeable)
        {
            if (!placedObjects.ContainsKey(placeable.NetworkId))
            {
                throw new System.Exception($"placed id {placeable.NetworkId} does not exist");
            }
            placedObjects.Remove(placeable.NetworkId);
        }


    }
}

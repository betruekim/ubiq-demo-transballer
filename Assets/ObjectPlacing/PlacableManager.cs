using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;

namespace PlacableObjects
{
    public static class PlacableManager
    {
        public static Dictionary<NetworkId, Placable> placedObjects = new Dictionary<NetworkId, Placable>();

        public static void AddPlacedObject(Placable placable)
        {
            if (placedObjects.ContainsKey(placable.Id))
            {
                throw new System.Exception($"placed id {placable.Id} already exists");
            }
            placedObjects[placable.Id] = placable;
        }

        public static void RemovePlacedObject(Placable placable)
        {
            if (!placedObjects.ContainsKey(placable.Id))
            {
                throw new System.Exception($"placed id {placable.Id} does not exist");
            }
            placedObjects.Remove(placable.Id);
        }


    }
}

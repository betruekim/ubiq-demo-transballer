using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;

namespace Transballer.PlaceableObjects
{
    public static class PlaceableIndex
    {
        public static Dictionary<int, Placeable> placedObjects = new Dictionary<int, Placeable>();

        public static void AddPlacedObject(Placeable placeable)
        {
            if (placedObjects.ContainsKey((int)placeable.Id))
            {
                throw new System.Exception($"placed id {placeable.Id} already exists");
            }
            placedObjects[(int)placeable.Id] = placeable;
        }

        public static void RemovePlacedObject(Placeable placeable)
        {
            if (!placedObjects.ContainsKey((int)placeable.Id))
            {
                throw new System.Exception($"placed id {placeable.Id} does not exist");
            }
            placedObjects.Remove((int)placeable.Id);
        }


    }
}

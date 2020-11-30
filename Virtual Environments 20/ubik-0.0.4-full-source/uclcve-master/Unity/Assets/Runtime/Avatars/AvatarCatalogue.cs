using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ubik.Avatars
{
    [CreateAssetMenu(menuName = "Avatar Catalogue")]
    public class AvatarCatalogue : ScriptableObject
    {
        public List<GameObject> prefabs;

        public GameObject GetPrefab(string guid)
        {
            foreach (var item in prefabs)
            {
                if (item.GetComponent<Avatar>().guid == guid)
                {
                    return item;
                }
            }

            throw new KeyNotFoundException("Avatar Guid \"" + guid + "\"");
        }
    }
}
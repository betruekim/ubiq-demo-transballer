using UnityEngine;
using System.Collections.Generic;
using Ubik.Samples;

// nested prefab catalogue, can pass in sub-catalogues
// make sure to not use indexing, since indexing isn't preserved
[CreateAssetMenu(menuName = "Nested Catalogue")]
public class NestedPrefabCatalogue : PrefabCatalogue
{
    public List<PrefabCatalogue> catalogues;
    public List<GameObject> randomItems;

    private void OnValidate()
    {
        Debug.Log("nestcatalogue onValidate");
        List<GameObject> newList = new List<GameObject>();
        foreach (PrefabCatalogue catalogue in catalogues)
        {
            if (catalogue != null)
            {
                newList.AddRange(catalogue.prefabs);
            }
        }
        newList.AddRange(randomItems);
        base.prefabs = newList;
    }

    public static GameObject GetPrefabFromName(string name)
    {
        PrefabCatalogue catalogue = GameObject.FindObjectOfType<NetworkSpawner>().catalogue;
        foreach (GameObject g in catalogue.prefabs)
        {
            if (g.name.CompareTo(name) == 0)
            {
                return g;
            }
        }
        throw new System.Exception($"cannot find prefab with name {name}");
    }
}
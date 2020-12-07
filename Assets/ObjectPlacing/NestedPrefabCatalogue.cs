using UnityEngine;
using System.Collections.Generic;
using Ubik.Samples;

// nested prefab catalogue, can pass in sub-catalogues
// make sure to not use indexing, since indexing isn't preserved
[CreateAssetMenu(menuName = "Nested Catalogue")]
public class NestedPrefabCatalogue : PrefabCatalogue
{
    public List<PrefabCatalogue> catalogues;

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
        base.prefabs = newList;
    }
}
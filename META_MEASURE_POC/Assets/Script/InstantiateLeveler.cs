using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateLeveler : MonoBehaviour
{
    public void InstantiateLevelerPrefab(GameObject levelerPrefab)
    {
        Instantiate(levelerPrefab);
    }
}

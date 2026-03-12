using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Labelinteractable : MonoBehaviour
{
    [SerializeField] private string labelName;

    public void OnSelected()
    {
        LabelSelectionManager.Instance.SelectLabel(labelName);
    }
}

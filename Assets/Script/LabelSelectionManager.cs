using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelSelectionManager : MonoBehaviour
{
    public static LabelSelectionManager Instance;

    [SerializeField] private GameObject labelPanel;

    private Action<string> onLabelChosen;

    private void Awake()
    {
        Instance = this;
        labelPanel.SetActive(false);
    }

    public void ShowPanel(Action<string> callback)
    {
        onLabelChosen = callback;
        labelPanel.SetActive(true);
    }

    public void SelectLabel(string label)
    {
        labelPanel.SetActive(false);
        onLabelChosen?.Invoke(label);
    }
}

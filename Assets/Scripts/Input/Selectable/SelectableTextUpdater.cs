using Computers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectableTextUpdater : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private void Start()
    {
        textMesh = gameObject.GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (textMesh != null) 
        {
            if(ComputerData.currentFocusedMachine == -1)
                textMesh.text = ComputerData.currentSelectable?.actionText;
            else
                textMesh.text = "";
        }
    }
}

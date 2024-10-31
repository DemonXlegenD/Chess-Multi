using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTextGUI : MonoBehaviour
{
    private TMPro.TextMeshProUGUI textGUI;
    public bool ShouldUpdate = false;

    void Start()
    {
        textGUI = transform.GetComponent<TMPro.TextMeshProUGUI>();
    }

    void Update()
    {
        if (textGUI.text != "" && ShouldUpdate) 
        {
            textGUI.ForceMeshUpdate();
            ShouldUpdate = false;
        }

        if (textGUI.text == "" && !ShouldUpdate)
        {
            textGUI.ForceMeshUpdate();
            ShouldUpdate = true;
        }
    }
}

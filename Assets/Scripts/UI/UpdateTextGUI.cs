using UnityEngine;

public class UpdateTextGUI : MonoBehaviour
{
    private TMPro.TextMeshProUGUI textGUI;
    public bool ShouldUpdate = false;
    public string oldText = "";

    void Start()
    {
        textGUI = transform.GetComponent<TMPro.TextMeshProUGUI>();
    }

    void Update()
    {
        if (textGUI.text != oldText) 
        {
            textGUI.ForceMeshUpdate();
            oldText = textGUI.text;
        }
    }
}

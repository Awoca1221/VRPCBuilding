using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildStatusElement : MonoBehaviour
{
    public TextMeshProUGUI description;
    
    public void SetText(string value)
    {
        description.text = value;
    }
}

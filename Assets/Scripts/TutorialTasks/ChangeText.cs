using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeText : MonoBehaviour
{
    [field: SerializeField] private TextMeshProUGUI TextMeshProObject { get; set; } = null;

    [field: SerializeField] public string OffTextString { get; set; } = "";
    [field: SerializeField] public Color OffTextColor { get; set; } = Color.red;
    [field: SerializeField] public FontStyles OffTextStyle { get; set; } = FontStyles.Normal;

    [field: SerializeField] public string OnTextString { get; set; } = "";
    [field: SerializeField] public Color OnTextColor { get; set; } = Color.green;
    [field: SerializeField] public FontStyles OnTextStyle { get; set; } = FontStyles.Normal;

    private void Start()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        if (OffTextString == "")
            OffTextString = text.text;
        if (OnTextString == "")
            OnTextString = text.text;
        TurnOff();
    }

    public void TurnOff()
    {
        if (TextMeshProObject != null)
        {
            TextMeshProObject.text = OffTextString;
            TextMeshProObject.color = OffTextColor;
            TextMeshProObject.fontStyle = OffTextStyle;
        }
    }

    public void TurnOn()
    {
        if (TextMeshProObject != null)
        {
            TextMeshProObject.text = OnTextString;
            TextMeshProObject.color = OnTextColor;
            TextMeshProObject.fontStyle = OnTextStyle;
        }
    }
}

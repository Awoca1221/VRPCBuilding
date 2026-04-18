using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FadeUI : MonoBehaviour
{
    public float showDuration = 1f;
    public float fadeDuration = 0.3f;
    private TextMeshProUGUI text;
    private bool isWorking = false;
    
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void ShowText()
    {
        if (isWorking) return;
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        isWorking = true;
        while (text.alpha < 1f)
        {
            text.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        yield return new WaitForSeconds(showDuration);

        while (text.alpha > 0f)
        {
            text.alpha -= Time.deltaTime / fadeDuration;
            yield return null;
        }
        isWorking = false;
    }
}

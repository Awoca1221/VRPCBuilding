using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeUI : MonoBehaviour
{
    public List<GameObject> UIPanels = new();
    [Tooltip("Обязательно нужно указать начальный элемент")]
    public GameObject UIActivateAtStart;

    private GameObject lastActiveUI;

    void Start()
    {
        foreach (var UIElem in UIPanels)
        {
            if (UIElem == UIActivateAtStart)
            {
                UIElem.SetActive(true);
            } else {
                UIElem.SetActive(false);
            }
        }

        lastActiveUI = UIActivateAtStart;
    }

    public void ActivatePanel(string name)
    {
        foreach (var UIElem in UIPanels)
        {
            if (UIElem.name == name)
            {
                lastActiveUI.SetActive(false);
                lastActiveUI = UIElem;
                lastActiveUI.SetActive(true);
                break;
            }
        }
    }

    public void ActivatePanel(GameObject obj)
    {
        foreach (var UIElem in UIPanels)
        {
            if (UIElem == obj)
            {
                lastActiveUI.SetActive(false);
                lastActiveUI = UIElem;
                lastActiveUI.SetActive(true);
                break;
            }
        }
    }

    public void ActivatePanel(int index)
    {
        if (index < 0 || index >= UIPanels.Count) return;

        lastActiveUI.SetActive(false);
        lastActiveUI = UIPanels[index];
        lastActiveUI.SetActive(true);
    }
}

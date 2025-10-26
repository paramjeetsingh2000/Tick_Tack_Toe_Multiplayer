using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private List<TMP_Text> allTexts = new();
    public bool canClick = true;

    private void Awake()
    {
        FirebaseManager.Instance.ButtonManager = this;
    }

    private void Start()
    {
        foreach (Transform t in transform)
        {
            TMP_Text tmp = t.GetChild(0).GetComponent<TMP_Text>();
            if (tmp != null)
            {
                allTexts.Add(tmp);
            }
        }
    }

    public void SendUpdatedData()
    {
        var data = new List<string>();

        foreach (var v in allTexts)
        {
            data.Add(v.text);
        }

        string json = JsonUtility.ToJson(new Wrapper { items = data });

        _ = FirebaseManager.Instance.UpdateNodeValue(json);
    }

    public void SetUpdatedData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            Debug.LogWarning("SetUpdatedData received empty JSON.");
            return;
        }

        Wrapper wrapper = JsonUtility.FromJson<Wrapper>(data);

        if (wrapper == null || wrapper.items == null)
        {
            Debug.LogWarning("Failed to parse JSON into list.");
            return;
        }

        int count = Mathf.Min(allTexts.Count, wrapper.items.Count);

        for (int i = 0; i < count; i++)
        {
            if (allTexts[i].text != wrapper.items[i])
            {
                allTexts[i].text = wrapper.items[i];
                canClick = true;
            }
        }

        Debug.Log("Updated TMP_Texts from JSON successfully.");
    }

    private class Wrapper
    {
        public List<string> items;
    }
}

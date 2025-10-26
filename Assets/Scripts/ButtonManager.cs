using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _winText;
    [SerializeField] private Transform _overlay;
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

        string winner = CheckWinner();
        if (winner != null)
        {
            if (winner == "") _winText.text = "Match Draw";
            else if ((winner == "O" && FirebaseManager.Instance.PlayerIndex == 1) || (winner == "X" && FirebaseManager.Instance.PlayerIndex == 2)) _winText.text = "You Win!";
            else _winText.text = "You lose";
            
            _overlay.gameObject.SetActive(true);
        }

        Debug.Log("Updated TMP_Texts from JSON successfully.");
    }

    public string CheckWinner()
    {
        int[][] winCombinations = new int[][]
        {
            new int[]{0,1,2}, // Row 1
            new int[]{3,4,5}, // Row 2
            new int[]{6,7,8}, // Row 3
            new int[]{0,3,6}, // Column 1
            new int[]{1,4,7}, // Column 2
            new int[]{2,5,8}, // Column 3
            new int[]{0,4,8}, // Diagonal 1
            new int[]{2,4,6}  // Diagonal 2
        };

        foreach (var combo in winCombinations)
        {
            string a = allTexts[combo[0]].text;
            string b = allTexts[combo[1]].text;
            string c = allTexts[combo[2]].text;

            if (!string.IsNullOrEmpty(a) && a == b && b == c)
            {
                return a;
            }
        }

        bool isDraw = true;
        foreach (var cell in allTexts)
        {
            if (string.IsNullOrEmpty(cell.text))
            {
                isDraw = false;
                break;
            }
        }

        if (isDraw)
            return "";

        return null;
    }

    private class Wrapper
    {
        public List<string> items;
    }
}

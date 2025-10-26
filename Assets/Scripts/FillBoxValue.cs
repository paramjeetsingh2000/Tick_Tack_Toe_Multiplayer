using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillBoxValue : MonoBehaviour
{
    private TMP_Text _value;
    private ButtonManager buttonManager;

    private void Awake()
    {
        _value = transform.GetChild(0).GetComponent<TMP_Text>();
        buttonManager =  transform.parent.GetComponent<ButtonManager>();
        transform.GetComponent<Button>().onClick.AddListener(() => { OnBtnClick(); });
    }

    private void OnBtnClick()
    {
        if (!buttonManager.canClick) return;

        if (string.IsNullOrEmpty(_value.text))
        {
            if (FirebaseManager.Instance.PlayerIndex == 1) _value.text = "O";
            else _value.text = "X";

            buttonManager.SendUpdatedData();
            buttonManager.canClick = false;
        }
    }
}

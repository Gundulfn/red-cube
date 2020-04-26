using UnityEngine;
using TMPro;

public class StateText : MonoBehaviour
{
    private TextMeshProUGUI stateText;

    void Awake()
    {
        stateText = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateStateText(string state)
    {
        stateText.SetText(state);  
    }
}
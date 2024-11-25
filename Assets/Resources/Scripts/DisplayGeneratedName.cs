using Lexic;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayGeneratedName : MonoBehaviour
{
    public NameGenerator nameGenerator;
    private TextMeshProUGUI display;
    // Start is called before the first frame update
    public void DisplayName()
    {
        display.text = NameGenerator.generatedName;
    }

    public void Start()
    {
        string userName;
        if (nameGenerator != null)
        {
            userName = "Generate Your Name";
        }
        else
        {
            userName = "Name Generator Down";
        }
        
        display = GetComponent<TextMeshProUGUI>();
        display.text = userName;    
    }
}

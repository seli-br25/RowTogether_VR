using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.Unity;

public class AudioSettingsManager : MonoBehaviour
{

    [SerializeField]
    Button VolumeUp;
    [SerializeField]
    Button VolumeDown;
    [SerializeField]
    Button Mute;
    [SerializeField]
    private Recorder recorder;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

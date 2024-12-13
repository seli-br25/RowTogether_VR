using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.Unity;
using TMPro;
using Photon.Voice.Unity.UtilityScripts;

public class AudioSettingsManager : MonoBehaviour
{

    [SerializeField]
    private Button muteButton;
    [SerializeField]
    private MicAmplifier amplifier;
    [SerializeField]
    private TextMeshProUGUI volumeDisplay;
    [SerializeField]
    private Sprite muteSprite;
    [SerializeField]
    private Sprite unMuteSprite;

    private Image muteButtonImage;

    private uint volume;

    // Start is called before the first frame update
    void Start()
    {
        volume = 5;
        volumeDisplay.text = volume.ToString();
        amplifier.AmplificationFactor = volume * 2;
        muteButtonImage = muteButton.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnClickVolDown()
    {
        if (volume > 1)
        {
            volume -= 1;
            DisplayVolume();
            SetGain();
        }
    }

    public void OnClickVolUp()
    {
        if (volume < 10)
        {
            volume += 1;
            DisplayVolume();
            SetGain();
        }
    }

    private void DisplayVolume()
    {
        volumeDisplay.text = volume.ToString();
    }

    private void SetGain()
    {
        amplifier.AmplificationFactor = volume * 2;
    }

    public void OnClickMute()
    {
        // Mute
        if (amplifier.AmplificationFactor > 0)
        {
            amplifier.AmplificationFactor = 0;

            muteButtonImage.sprite = muteSprite;
        } 
        // UnMute
        else
        {
            amplifier.AmplificationFactor = volume * 2;

            muteButtonImage.sprite = unMuteSprite;
        }
    }
}

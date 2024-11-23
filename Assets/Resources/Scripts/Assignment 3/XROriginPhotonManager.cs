using Photon.Pun;
using TMPro;
using UnityEngine;

public class XROriginPhotonManager : MonoBehaviour
{

    private PhotonView photonViewCharacter;
    private GameObject playerTrack;
    [SerializeField]
    private GameObject UI;
    private bool hasFinished = false;

    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal") && !hasFinished)
        {
            UI.SetActive(true);
            hasFinished = true;
            if (UI.activeInHierarchy) 
            {
                photonViewCharacter.RPC("HandleFinish", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            }

        }

    }

    // Must be called after a player has joined the room.
    public void setPhotonPlayerView(PhotonView playerPhotonView)
    {
        photonViewCharacter = playerPhotonView;
        if (photonViewCharacter == null)
        {
            Debug.LogError("Spawned Player Photon View is Null");
        } 
    }

    public void setClientPlayerTrack(GameObject spawnedPlayerTrack)
    {
        playerTrack = spawnedPlayerTrack;
        if (playerTrack == null)
        {
            Debug.LogError("Spawned Player Track is Null");
        }
    }


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (UI != null && UI.activeInHierarchy)
        {
            if (UI.GetComponentInChildren<TextMeshProUGUI>().text == "Go!")
            {
                playerTrack.GetComponent<StartSignal>().RemoveBarrier();
            }
        }
    }
}

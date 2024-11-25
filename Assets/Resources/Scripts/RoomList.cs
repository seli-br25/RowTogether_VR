using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class RoomList : MonoBehaviourPunCallbacks
{

    public Transform roomSelectionMenuContent;
    public GameObject roomListItemPrefab;


    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();



    // Photon only provides list of rooms whose information has changed.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);

        Debug.Log("RoomList updated");

        // initialize room list
        if (cachedRoomList.Count <= 0)
        {
            cachedRoomList = roomList;
        }
        // for every changed room on photon side
        // iterate find the changed room on the chached list and either:
            // remove the room if no longer exist (room.RemovedFromList)
            // update the cached list with updated room information
        else
        {
            foreach (var room in roomList)
            {
                // we are not manipulating the list while its still iterating
                // with every room that has changed, we start a new iteration of cachedRoomList
                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if(cachedRoomList[i].masterClientId == room.masterClientId)
                    {
                        List<RoomInfo> newList = cachedRoomList;


                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }

                        cachedRoomList = newList;
                    }
                }
            }
        }
        UpdateMenu();

    }


    void UpdateMenu()
    {
        //clear out room items in menu
        foreach(Transform roomItem in roomSelectionMenuContent)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomSelectionMenuContent);
            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/ 6";
            // Configure onclick callback with correct masterclient id for each room
            roomItem.GetComponent<Button>().onClick.AddListener(() => GetComponent<RoomManager>().OnClickJoinRoom(room.Name));
        }
    }


}

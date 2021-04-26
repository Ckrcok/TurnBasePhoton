using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
  // instance
  public static NetworkManager instance;

  void Awake ()
  {
    instance = this;
    DontDestroyOnLoad(gameObject);
  }

  void Start ()
  {
    // connect to the master server
    PhotonNetwork.ConnectUsingSettings();
  }

  public override void OnConnectedToMaster()
  {
    CreateOrJoinRoom();
  }

  public override void OnJoinedRoom()
  {
    Debug.Log("In Room");
  }

  //joins a random room or creates a new room
  public void CreateOrJoinRoom ()
  {
      // if there are avilable rooms, join a random one
      if(PhotonNetwork.CountOfRooms > 0)
          PhotonNetwork.JoinRandomRoom();
      //otherwise, create a new one
      else
      {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, options);
      }
    }

    // changes the scene using Photon's system
    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

}

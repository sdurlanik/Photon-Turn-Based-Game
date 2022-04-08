using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //instance
    public static NetworkManager instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // master servera bağlanma
        PhotonNetwork.ConnectUsingSettings();
    }
    

    // Yeni bir oda kurar veya odaya katılır
    public void CreateOrJoinRoom()
    {
        // Katılabilecek bir oda varsa katılır
        if (PhotonNetwork.CountOfRooms > 0)
            PhotonNetwork.JoinRandomRoom();
        
        // Katılabilecek oda yoksa yeni bir oda kurar
        else
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;

            PhotonNetwork.CreateRoom(null, options);
        }
    }

    // Sahne geçişinde kullanılır
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    [SerializeField] private GameObject _mainScreen;
    [SerializeField] private GameObject _lobbyScreen;

    [Header("Main Screen")]
    [SerializeField] private Button _playButton;

    [Header("Lobby Screen")]
    [SerializeField] private TextMeshProUGUI _player1NameText;
    [SerializeField] private TextMeshProUGUI _player2NameText;
    [SerializeField] private TextMeshProUGUI _gameStartingText;

    void Start ()
    {
        // sunucuya bağlı değilken play butonunu deaktif eder
        _playButton.interactable = false;
        _gameStartingText.gameObject.SetActive(false);
    }

    // master servera bağlandığında çağrılır
    public override void OnConnectedToMaster ()
    {
        _playButton.interactable = true;
    }

    // aktif sahneyi ayarlar
    public void SetScreen (GameObject screen)
    {
        // tüm ekranları kapatır
        _mainScreen.SetActive(false);
        _lobbyScreen.SetActive(false);

        // istenen ekran açılır
        screen.SetActive(true);
    }

    // oyuncunun kullanıcı adını günceller
    public void OnUpdatePlayerNameInput (TMP_InputField nameInput)
    {
        PhotonNetwork.NickName = nameInput.text;
    }

    // play butonu tıklandığında çağrılır
    public void OnPlayButton ()
    {
        NetworkManager.instance.CreateOrJoinRoom();
    }

    // bir odaya giriş yapıldığında çağrılır
    public override void OnJoinedRoom ()
    {
        SetScreen(_lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    // odadan bir oyuncu ayrıldığında çağrılır
    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    // lobby ekranındaki verileri günceller
    [PunRPC]
    void UpdateLobbyUI ()
    {
        // player isimlerini ayarlar
        _player1NameText.text = PhotonNetwork.CurrentRoom.GetPlayer(1).NickName;
        _player2NameText.text = PhotonNetwork.PlayerList.Length == 2 ? PhotonNetwork.CurrentRoom.GetPlayer(2).NickName : "waiting for player2";

        // game starting textini ayarlar
        if(PhotonNetwork.PlayerList.Length == 2)
        {
            _gameStartingText.gameObject.SetActive(true);

            if(PhotonNetwork.IsMasterClient)
                Invoke("TryStartGame", 3.0f);
        }
    }

    // lobbyde iki oyuncu bulunuyorsa oyunu başlatır
    void TryStartGame ()
    {
        // lobbyde iki oyuncu bulunuyorsa game sahnesine geçiş yapar
        if(PhotonNetwork.PlayerList.Length == 2)
            NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
        
        else _gameStartingText.gameObject.SetActive(false);
    }

    // leave butonu tıklandığında çağrılır
    public void OnLeaveButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(_mainScreen);
    }
}
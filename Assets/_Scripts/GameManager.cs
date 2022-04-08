using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    public PlayerController leftPlayer;
    public PlayerController rightPlayer;

    public PlayerController currentPlayer;      // saldırı sırasının hangi oyuncuda olduğunu tutar

    [SerializeField] private float _postGameTime;              // oyun bittiğinde menüye dönme süresi

    // instance
    public static GameManager instance;

    void Awake ()
    {
        instance = this;
    }

    void Start ()
    {
        // master player seçimini ayarlar
        if(PhotonNetwork.IsMasterClient)
            SetPlayers();
    }

    // player verilerini oluşturur ve unit spawnlar
    void SetPlayers ()
    {
        // tanımlanan playerlara photon viewlarını atar
        leftPlayer.photonView.TransferOwnership(1);
        rightPlayer.photonView.TransferOwnership(2);

        // Playerları oluşturur
        leftPlayer.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.CurrentRoom.GetPlayer(1));
        rightPlayer.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.CurrentRoom.GetPlayer(2));

        // ilk oyuncunun sırasını başlatır
        photonView.RPC("SetNextTurn", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SetNextTurn ()
    {
        // ilk tur kontrolü
        if(currentPlayer == null)
            currentPlayer = leftPlayer;
        else
            currentPlayer = currentPlayer == leftPlayer ? rightPlayer : leftPlayer;

        // sıra bizde mi
        if (currentPlayer == PlayerController.me)
        {
            PlayerController.me.BeginTurn();
        }

        // sıra bizdeyse end turn butonunu açar
        GameUI.instance.ToggleEndTurnButton(currentPlayer == PlayerController.me);
    }

    // Diğer playerı seçer
    public PlayerController GetOtherPlayer (PlayerController player)
    {
        return player == leftPlayer ? rightPlayer : leftPlayer;
    }

    // kendi uniti öldüğünde player tarafından çağrılır
    // RPC ile diğer enemye gönderilir
    public void CheckWinCondition ()
    {
        if(PlayerController.me.units.Count == 0)
            photonView.RPC("WinGame", RpcTarget.All, PlayerController.enemy == leftPlayer ? 0 : 1);
    }

    // Diğer playerın tüm unitleri öldürüldüğünde çağrılır
    [PunRPC]
    void WinGame (int winner)
    {
        // kazanan oyuncuyu al
        PlayerController player = winner == 0 ? leftPlayer : rightPlayer;

        // win textini ayarla
        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        // belirtilen süreden sonra menüye dön
        Invoke("GoBackToMenu", _postGameTime);
    }

    // menüye dönme işlemi
    void GoBackToMenu ()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
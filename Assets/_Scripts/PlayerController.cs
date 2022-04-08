using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    public Player photonPlayer;
    [SerializeField] private string[] _unitsToSpawn;
    [SerializeField] private Transform[] _unitSpawnPositions;      // spawn noktalarını inspector üzerinden belirler

    public List<Unit> units = new List<Unit>(); // sahip olunan tüm ünitlerin listesi
    private Unit selectedUnit;                  // seçili unit

    public static PlayerController me;          // local player
    public static PlayerController enemy;       // enemy player (non-local)

    // oyun başladığında çağrılır
    [PunRPC]
    void Initialize (Player player)
    {
        photonPlayer = player;

        // eğer local player ise unitleri spawnlar
        if(player.IsLocal)
        {
            me = this;
            SpawnUnits();
        }
        else
            enemy = this;

        // player name textlerini ayarlar
        GameUI.instance.SetPlayerText(this);
    }

    // player unitlerini spawnlar
    void SpawnUnits ()
    {
        for(int x = 0; x < _unitsToSpawn.Length; ++x)
        {
            GameObject unit = PhotonNetwork.Instantiate(_unitsToSpawn[x], _unitSpawnPositions[x].position, Quaternion.identity);
            unit.GetPhotonView().RPC("Initialize", RpcTarget.Others, false);
            unit.GetPhotonView().RPC("Initialize", photonPlayer, true);
        }
    }

    void Update ()
    {
        // işlemler sadece local player tarafından yapılır
        if(!photonView.IsMine)
            return;

        // sıra bizdeyse ve sol tıka basıldıysa
        if(Input.GetMouseButtonDown(0) && GameManager.instance.currentPlayer == this)
        {
            // tıklanınal yeri hesaplar ve seçmeye çalışır (TrySelect fonksiyonu)
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TrySelect(new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), 0));
        }
    }

    void TrySelect (Vector3 selectPos)
    {
        // kendi unitimiz seçiliyse unit değişkenine atar seçili değilse null döner
        Unit unit = units.Find(x => x.transform.position == selectPos);

        // null değilse uniti seç (SelectUnit)
        if(unit != null)
        {
            SelectUnit(unit);
            return;
        }

        // unit seçili değilse bir şey yapma
        if(!selectedUnit) return;

        // düşman uniti seçili ise saldırmayı dene (TryAttack)
        Unit enemyUnit = enemy.units.Find(x => x.transform.position == selectPos);

        if(enemyUnit != null)
        {
            TryAttack(enemyUnit);
            return;
        }

        // unit seçmediysek ve düşmana saldırılmıyorsa hareket etmeyi dene
        TryMove(selectPos);
    }

    // unite tıklandığında çağrılır
    void SelectUnit (Unit unitToselect)
    {
        // uniti seçebilir miyiz
        if(!unitToselect.CanSelect())
            return;

        // seçimi kaldır
        if(selectedUnit != null)
            selectedUnit.ToggleSelect(false);

        // yeni unit seç
        selectedUnit = unitToselect;
        selectedUnit.ToggleSelect(true);

        // unit info textini ayarla
        GameUI.instance.SetUnitInfoText(unitToselect);
    }

    // seçimi kaldırır
    void DeSelectUnit ()
    {
        selectedUnit.ToggleSelect(false);
        selectedUnit = null;

        // unit info textini kapatır
        GameUI.instance.unitInfoText.gameObject.SetActive(false);
    }

    // hareket edebilen ya da saldırabilen bir unit seçer
    // (ilk seçim yapıldıktan sonra hala hakkı varsa otomatik olarak diğer uniti seçmek için)
    void SelectNextAvailableUnit ()
    {
        Unit availableUnit = units.Find(x => x.CanSelect());

        if(availableUnit != null)
            SelectUnit(availableUnit);
        else
            DeSelectUnit();
    }

    // seçili enemye saldırmayı dener
    void TryAttack (Unit enemyUnit)
    {
        // enemy unite saldırabilir miyiz
        if(selectedUnit.CanAttack(enemyUnit.transform.position))
        {
            selectedUnit.Attack(enemyUnit);
            SelectNextAvailableUnit();
            GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
    }

    // seçili pozisyona ilerlemeyi dener
    void TryMove (Vector3 movePos)
    {
        // pozisyona ilerleyebilir mi
        if(selectedUnit.CanMove(movePos))
        {
            selectedUnit.Move(movePos);
            SelectNextAvailableUnit();
            GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
    }

    // sıramız bittiğinde çağrılır
    public void EndTurn ()
    {
        // seçimi kaldır
        if(selectedUnit != null)
            DeSelectUnit();

        // sonraki sırayı başlat
        GameManager.instance.photonView.RPC("SetNextTurn", RpcTarget.All);
    }

    // sıramız geldiğinde çağrılır
    public void BeginTurn ()
    {
        foreach(Unit unit in units)
            unit.usedThisTurn = false;

        // UI günceller
        GameUI.instance.UpdateWaitingUnitsText(units.Count);
    }
}
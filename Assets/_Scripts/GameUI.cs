using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private TextMeshProUGUI _leftPlayerText;
    [SerializeField] private TextMeshProUGUI _rightPlayerText;
    [SerializeField] private TextMeshProUGUI _waitingUnitsText;
    [SerializeField] private TextMeshProUGUI _winText;
    public TextMeshProUGUI unitInfoText;

    // instance
    public static GameUI instance;

    void Awake ()
    {
        instance = this;
    }

    // end turn butonu basıldığında çağrılır
    public void OnEndTurnButton ()
    {
        PlayerController.me.EndTurn();
    }

    // end turn butonunun tıklanabilirliğini ayarlar
    public void ToggleEndTurnButton (bool toggle)
    {
        _endTurnButton.interactable = toggle;
        _waitingUnitsText.gameObject.SetActive(toggle);
    }

    // kullanılabilecek unit sayısını gösterir
    public void UpdateWaitingUnitsText (int waitingUnits)
    {
        _waitingUnitsText.text = waitingUnits + " Units Waiting";
    }

    // player name textini ayarlar
    public void SetPlayerText (PlayerController player)
    {
        TextMeshProUGUI text = player == GameManager.instance.leftPlayer ? _leftPlayerText : _rightPlayerText;
        text.text = player.photonPlayer.NickName;
    }

    // unit info textini ayarlar
    public void SetUnitInfoText (Unit unit)
    {
        unitInfoText.gameObject.SetActive(true);
        unitInfoText.text = "";

        unitInfoText.text += string.Format("<b>Hp:</b> {0} / {1}", unit.curHp, unit.maxHp);
        unitInfoText.text += string.Format("\n<b>Move Range:</b> {0}", unit.maxMoveDistance);
        unitInfoText.text += string.Format("\n<b>Attack Range:</b> {0}", unit.maxAttackDistance);
        unitInfoText.text += string.Format("\n<b>Damage:</b> {0} - {1}", unit.minDamage, unit.maxDamage);
    }

    // win textini ekranda gösterir
    public void SetWinText (string winnerName)
    {
        _winText.gameObject.SetActive(true);
        _winText.text = winnerName + " Wins";
    }
}
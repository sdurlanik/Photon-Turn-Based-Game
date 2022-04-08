using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Unit : MonoBehaviourPun
{
    public int curHp;               // şuanki can
    public int maxHp;               // maximum can
    public float moveSpeed;         // yürüme hızı
    public int minDamage;           // minimum saldırı gücü
    public int maxDamage;           // maximum saldırı gücü

    public int maxMoveDistance;     // max yürüme mesafesi
    public int maxAttackDistance;   // max saldırı mesafesi

    public bool usedThisTurn;       // unit sırasını kullandı mı (Player için değil unit için geçerli)

    [SerializeField] private GameObject _selectedVisual;   // seçim dairesi
    [SerializeField] private SpriteRenderer _spriteVisual; // unit spriteı

    [Header("UI")]
    [SerializeField] private Image _healthFillImage;       // kırmızı can barı

    [Header("Sprite Variants")]
    [SerializeField] private Sprite _leftPlayerSprite;     // sol player (mavi)
    [SerializeField] private Sprite _rightPlayerSprite;    // sağ player (kırmızı)

    // unit spawnlandığında çağrılır
    [PunRPC]
    void Initialize (bool isMine)
    {
        if(isMine) PlayerController.me.units.Add(this);
        else GameManager.instance.GetOtherPlayer(PlayerController.me).units.Add(this);

        _healthFillImage.fillAmount = 1.0f;

        // spriteları ayarlar
        _spriteVisual.sprite = transform.position.x < 0 ? _leftPlayerSprite : _rightPlayerSprite;

        // unitlerin birbirlerine dönmesini sağlar
        _spriteVisual.transform.up = transform.position.x < 0 ? Vector3.left : Vector3.right;
    }

    // seçim yapabilir mi
    public bool CanSelect ()
    {
        if(usedThisTurn) return false;
        else return true;
    }

    // pozisyona ilerlenebilir mi
    public bool CanMove (Vector3 movePos)
    {
        if(Vector3.Distance(transform.position, movePos) <= maxMoveDistance)
            return true;
        else return false;
    }

    // pozisyona saldırılabilir mi
    public bool CanAttack (Vector3 attackPos)
    {
        if(Vector3.Distance(transform.position, attackPos) <= maxAttackDistance)
            return true;
        else return false;
    }

    // seçim yapıldığında ya da kaldırıldığında çağrılır
    public void ToggleSelect (bool selected)
    {
        _selectedVisual.SetActive(selected);
    }

    public void Move (Vector3 targetPos)
    {
        usedThisTurn = true;

        // hareket etmeden önce hedefe yönelir
        Vector3 dir = (transform.position - targetPos).normalized;
        _spriteVisual.transform.up = dir;

        StartCoroutine(MoveOverTime());

        IEnumerator MoveOverTime ()
        {
            while(transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    // enemy unite saldırır
    public void Attack (Unit unitToAttack)
    {
        usedThisTurn = true;
        unitToAttack.photonView.RPC("TakeDamage", PlayerController.enemy.photonPlayer, Random.Range(minDamage, maxDamage + 1));
    }

    // enemy unitten saldırı aldığımızda çağrılır
    [PunRPC]
    void TakeDamage (int damage)
    {
        curHp -= damage;

        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
        else
        {
            // can barını günceller
            photonView.RPC("UpdateHealthBar", RpcTarget.All, (float)curHp / (float)maxHp);
        }
    }

    // can barını günceller
    [PunRPC]
    void UpdateHealthBar (float fillAmount)
    {
        _healthFillImage.fillAmount = fillAmount;
    }

    // unit canı 0 olduğunda çağrılır
    [PunRPC]
    void Die ()
    {
        if(!photonView.IsMine)
            PlayerController.enemy.units.Remove(this);
        else
        {
            PlayerController.me.units.Remove(this);

            // win koşulunu kontrol eder
            GameManager.instance.CheckWinCondition();

            // network tabanlı destroy işlemi
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
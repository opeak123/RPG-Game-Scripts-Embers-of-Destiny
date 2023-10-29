using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillSystem;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField]
    PlayerState P_State;

    private int CLASS { get; set; } // Ŭ����
    private int LV { get; set; } // ����
    private int EXP { get; set; } // ����ġ
    private int HP { get; set; }    // ü��
    private int MaxMP { get; set; }    // ����
    private int CurrentMp { get; set; }
    private int DEF { get; set; }  // ����
    private int ATK { get; set; }  // ���ݷ�
    private int STATDMG { get; set; } // ���ȵ�����
    private int TOTALDMG { get; set; } // ��Ż������
    private float MOVESPEED { get; set; } // �̵��ӵ�
    private float ATKSPEED { get; set; }  // ���ݼӵ�

    private bool STUN { get; set; } // ����
    private bool BURN { get; set; } // ȭ��
    private bool POISION { get; set; } // ��
    private bool DIE { get; set; }  // ����
    private bool DASH { get; set; } // �뽬

    // Getter �޼���
    public int GetCLASS() => CLASS;
    public int GetLV() => LV;
    public int GetEXP() => EXP;
    public int GetHP() => HP;
    public int GetMaxMP() => MaxMP;
    public int GetCurrentMp() => CurrentMp;
    public int GetDEF() => DEF;
    public int GetATK() => ATK;
    public int GetSTATDMG() => STATDMG;
    public int GetTOTALDMG() => TOTALDMG;
    public float GetMOVESPEED() => MOVESPEED;
    public float GetATKSPEED() => ATKSPEED;
    public bool GetSTUN() => STUN;
    public bool GetBURN() => BURN;
    public bool GetPOISION() => POISION;
    public bool GetDIE() => DIE;
    public bool GetDASH() => DASH;

    // Setter �޼���
    public void SetCLASS(int value) => CLASS = value;
    public void SetLV(int value) => LV = value;
    public void SetEXP(int value) => EXP = LevelManager(value);
    public void SetHP(int value) => HP = 900 + (100 * StatManager()) + value;
    public void SetMaxMP(int value) => MaxMP = 400 + (100 * StatManager()) + value;
    public void SetCurrentMp(int value) => CurrentMp = value;
    public void SetDEF(int value) => DEF = 9 + (1 * StatManager()) + value;
    public void SetATK(int value) => ATK = 9 + (1 * StatManager()) + value;
    public void SetMOVESPEED(float value) => MOVESPEED = (float)2.9 + (float)(0.1 * StatManager()) + value;
    public void SetATKSPEED(float value) => ATKSPEED = (float)0.9 + (float)(0.1 * StatManager()) + value;
    public void SetSTUN(bool value) => STUN = value;
    public void SetBURN(bool value) => BURN = value;
    public void SetPOISION(bool value) => POISION = value;
    public void SetDIE(bool value) => DIE = value;
    public void SetDASH(bool value) => DASH = value;

    private bool isAttack = false;
    private bool isBackStep = false;

    private void Awake()
    {
        SetLV(1);
    }

    private void Start()
    {
        SetHP(0);
        SetMaxMP(0);
        SetDEF(0);
        SetATK(0);
        SetATKSPEED(0);
        SetMOVESPEED(0);
        P_State.SettingHpMp();
    }

    private int LevelManager(int exp)
    {
        int maxExp = 1000 + (100 * GetLV());
        int curExp = GetEXP() + exp;

        if (maxExp <= curExp)
        {
            curExp = curExp - maxExp;
            SetLV(GetLV() + 1);
            SetMaxMP(0);
            P_State.currentHp = GetHP() + 100;
            P_State.currentMp = GetMaxMP();

            QuickSkillUI.Instance.AllQuickSlotCheck();
            SkillBook.Instance.AllSkillSlotCheck();
        }
        return curExp;
    }

    public int StatManager()
    {
        int statAdd = GetLV();
        return statAdd;
    }

    public void FindStateManager()
    {
        P_State = FindObjectOfType<PlayerState>();
    }

    private void Update()
    {
        if (!DIE && Input.GetKeyDown(KeyCode.Alpha5))
            HitPlayer(100);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            SetEXP(500);
            
        if (Input.GetKeyDown(KeyCode.F5))
            SetCLASS(0);
        if (Input.GetKeyDown(KeyCode.F6))
            SetCLASS(1);
        if (Input.GetKeyDown(KeyCode.F7))
            SetCLASS(2);
        
    }

    public void BackStep()
    {
        isBackStep = true;
    }

    public bool IsBackStep()
    {
        return isBackStep;
    }

    public void Attack()
    {
        isAttack = true;
    }

    public void AttackEnd()
    {
        isBackStep = false;
        isAttack = false;
    }

    public bool IsAttack()
    {
        return isAttack;
    }

    //���������
    public void HitPlayer(int _damage)
    {
        P_State.HitPlayer(_damage, true);
    }

    public void MaxHp(int _Hp)
    {
        SetHP(_Hp);
    }

}

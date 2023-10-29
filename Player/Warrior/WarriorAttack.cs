using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorAttack : MonoBehaviour
{
    Animator anim;
    WeaponSheath weaponBayonet;
    Transform lightswordslashTr;
    Transform originswordTr;
    [SerializeField]
    ParticleSystem attack1Trail;
    [SerializeField]
    ParticleSystem attack2Trail;
    [SerializeField]
    ParticleSystem attack3Trail;
    [SerializeField]
    ParticleSystem attack4Trail;
    [SerializeField]
    GameObject lightswordSlash;

    [SerializeField]
    Transform[] swordPos;
    Transform SwordPos;
    PlayerStateManager stateManager;
    [SerializeField]
    GameObject whirlwindEffect;
    [SerializeField]
    GameObject multiSlashes;
    [SerializeField]
    GameObject multiSlashsLeft;
    [SerializeField]
    GameObject swordJudgementWait;

    private float atkSpeed;
    private float whirlWinding; //������ ���ӽð�
    private float lightswordslashCoolTime = 3.0f;
    private float whirlwindCoolTime = 4.0f;
    private float multislashCoolTime = 5.0f;
    private float swordjudgementCoolTime = 10.0f;

    private int comboCount = 0;

    private bool normalAttack = false;
    private bool nextAttack = false;
    private bool isSlash = false;
    private bool isWhirlWind = false;
    private bool whirlwindOn = false;
    private bool isMultiSlash = false;
    private bool isSwordJudge = false;
    
    private enum Skill
    {
        stop,
        slash,
        whirlwind,
        multislash,
        swordjudgement,
        attack,
        whirlwindend,
        swordjudgementend
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        weaponBayonet = GetComponent<WeaponSheath>();
        whirlwindEffect = transform.GetChild(13).gameObject;
        lightswordSlash = transform.GetChild(15).gameObject;
        attack1Trail = transform.GetChild(14).GetChild(0).GetComponent<ParticleSystem>();
        attack2Trail = transform.GetChild(14).GetChild(1).GetComponent<ParticleSystem>();
        attack3Trail = transform.GetChild(14).GetChild(2).GetComponent<ParticleSystem>();
        attack4Trail = transform.GetChild(14).GetChild(3).GetComponent<ParticleSystem>();

        lightswordslashTr = GameObject.FindWithTag("SwordPos").transform.GetChild(8).transform;
        originswordTr = GameObject.FindWithTag("SwordPos").transform.GetChild(9).transform;
    }

    private void Start()
    {
        stateManager = FindObjectOfType<PlayerStateManager>();
        atkSpeed = stateManager.GetATKSPEED();
        for (int i = 0; i < swordPos.Length; i++)
        {
            swordPos[i] = GameObject.FindWithTag("SwordPos").transform.GetChild(i + 1).transform;
        }
        SwordPos = GameObject.FindWithTag("Weapon_Hand").transform;

        SwordPos.position = swordPos[0].position;
        SwordPos.rotation = swordPos[0].rotation;
    }

    void Update()
    {
        if (GetComponent<PlayerState>().currentMp < stateManager.GetMaxMP())
        {
            GetComponent<PlayerState>().currentMp += 5 * Time.deltaTime;
        }
        else
        {
            GetComponent<PlayerState>().currentMp = stateManager.GetMaxMP();
        }

        if (GetComponent<PlayerState>().currentHp < stateManager.GetHP())
        {
            GetComponent<PlayerState>().currentHp += 10 * Time.deltaTime;
        }
        else
        {
            GetComponent<PlayerState>().currentHp = stateManager.GetHP();
        }

        SkillCoolTime();
        if (!weaponBayonet.Bayonet())
        {
            //WarriorDamage();
            Slash();
            WhirlWind();
            MultiSlashes();
            SwordJudgement();
            if (anim.GetInteger("Skill") == (int)Skill.stop && !stateManager.IsAttack() && anim.GetInteger("Movement") != 2)
                Attack();
            else if (anim.GetInteger("Skill") == (int)Skill.attack && stateManager.IsAttack() && anim.GetInteger("Movement") != 2)
                Attack();
        }
    }

    #region Attack/Skill
    //����
    private void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            nextAttack = true;
            anim.SetInteger("Skill", (int)Skill.attack);
        }
    }

    public bool NormalAtk()
    {
        return normalAttack;
    }

    private void Slash() //������
    {
        if (anim.GetInteger("Movement") != 2 && !isMultiSlash && !isSlash && !isWhirlWind && !isSwordJudge)
        {
            if (Input.GetKeyDown(KeyCode.Q) && lightswordslashCoolTime >= 3.0f && stateManager.GetCurrentMp() >= 30)
            {
                GetComponent<PlayerState>().currentMp -= 30;
                isSlash = true;
                lightswordslashCoolTime = 0;
                anim.speed = 2.0f;
                stateManager.Attack();
                SwordPos.position = swordPos[4].position;
                SwordPos.rotation = swordPos[4].rotation;
                anim.SetInteger("Skill", (int)Skill.slash);
            }
            else if (lightswordslashCoolTime < 3.0f && Input.GetKeyDown(KeyCode.Q))
                GetComponent<PlayerSoundPlay>().SkillCoolTime();
        }
    }

    public void SlashEffectOn() //������ ����Ʈ
    {
        lightswordSlash.GetComponent<ParticleSystem>().Play();
        SwordPos.GetComponent<BoxCollider>().enabled = true;
        SwordPos.position = lightswordslashTr.position;
        SwordPos.rotation = lightswordslashTr.rotation;
        lightswordSlash.transform.parent = null;
    }

    public void SlashEffectOff()
    {
        isSlash = false;
        SwordPos.GetComponent<BoxCollider>().enabled = false;
        anim.SetInteger("Skill", (int)Skill.stop);
        lightswordSlash.GetComponent<ParticleSystem>().Stop();
        stateManager.AttackEnd();
        anim.speed = 1.0f;

        Invoke("LightSwordReset", 1.5f);
    }

    private void LightSwordReset()
    {
        lightswordSlash.transform.parent = transform;
        lightswordSlash.transform.position = transform.position + transform.forward * 7.0f;
        lightswordSlash.transform.rotation = transform.rotation;
    }

    private void WhirlWind() //������
    {
        if (anim.GetInteger("Movement") != 2 && !isMultiSlash && !isSlash && !isWhirlWind && !isSwordJudge)
        {
            if (!whirlwindOn && (whirlwindCoolTime >= 4.0f) && stateManager.GetCurrentMp() >= 70) //��Ÿ���� �� á�ٸ�
            {
                if (Input.GetKeyDown(KeyCode.E)) //EŰ�� �����ٸ�
                {
                    GetComponent<PlayerState>().currentMp -= 70;
                    whirlwindOn = true;
                    isWhirlWind = true;
                    stateManager.AttackEnd();
                    whirlWinding = 0;
                    whirlwindCoolTime = 0;
                    whirlwindEffect.GetComponent<ParticleSystem>().Play();
                    anim.SetInteger("Skill", (int)Skill.whirlwind);
                    GetComponent<PlayerSoundPlay>().PlayerBigSound1_Male();
                }
            }
            else if(whirlwindCoolTime < 4.0f && Input.GetKeyDown(KeyCode.E))
                GetComponent<PlayerSoundPlay>().SkillCoolTime();
        }

        if (isWhirlWind) //������ ���ӽð�
        {
            whirlWinding += Time.deltaTime;
            if (Input.GetMouseButtonDown(0)) //���ӽð� ���� ���ݹ�ư
            {
                isWhirlWind = false;
                whirlWinding = 0;
                stateManager.Attack();
                anim.SetInteger("Skill", (int)Skill.attack);
            }
        }

        if (whirlWinding >= 3.0f) //���ӽð��� �����ٸ�
        {
            isWhirlWind = false;
            whirlWinding = 0;
            stateManager.Attack();
            anim.SetInteger("Skill", (int)Skill.whirlwindend);
        }
            
    }

    public bool WHIRLWIND()
    {
        return whirlwindOn;
    }

    public void WhirlWindEnd()
    {
        //whirlwindEffect.SetActive(false);
        whirlwindEffect.GetComponent<ParticleSystem>().Stop();
    }

    public void WhirlWindPos() //������ ���� ��ġ
    {
        SwordPos.position = swordPos[3].position;
        SwordPos.rotation = swordPos[3].rotation;
    }

    private void MultiSlashes() //���� �˱� ������
    {
        if (anim.GetInteger("Movement") != 2 && !isMultiSlash && !isSlash && !isWhirlWind && !isSwordJudge)
        {
            if (Input.GetKeyDown(KeyCode.R) && multislashCoolTime >= 5.0f && stateManager.GetCurrentMp() >= 120)
            {
                GetComponent<PlayerState>().currentMp -= 120;
                anim.SetInteger("Skill", (int)Skill.multislash);
                isMultiSlash = true;
                multislashCoolTime = 0;
                anim.speed = 1.8f;
                stateManager.Attack();
                Invoke("SwordOrigin", 3.2f);
            }
            else if(Input.GetKeyDown(KeyCode.R) && multislashCoolTime < 5.0f)
                GetComponent<PlayerSoundPlay>().SkillCoolTime();
        }
    }
    public void MultiSlashsToRight()
    {
        Instantiate(multiSlashes, transform.position + transform.up * 1.2f + transform.forward * 2.0f, Quaternion.Euler(-90, transform.rotation.eulerAngles.y, 0));
    }

    public void MultiSlashsToLeft()
    {
        Instantiate(multiSlashsLeft, transform.position + transform.up * 1.2f + transform.forward * 2.0f, Quaternion.Euler(-90, transform.rotation.eulerAngles.y, 0));
    }

    //�ñر�
    private void SwordJudgement()
    {
        if (anim.GetInteger("Movement") != 2 && !isMultiSlash && !isSlash && !isWhirlWind && !isSwordJudge)
        {
            if (Input.GetKeyDown(KeyCode.F) && swordjudgementCoolTime >= 10.0f && stateManager.GetCurrentMp() >= 200)
            {
                GetComponent<PlayerState>().currentMp -= 200;
                swordjudgementCoolTime = 0;
                isSwordJudge = true;
                stateManager.Attack();
                anim.SetInteger("Skill", (int)Skill.swordjudgement);
                SwordPos.position = swordPos[5].position;
                SwordPos.rotation = swordPos[5].rotation;
                Instantiate(swordJudgementWait, transform.position + transform.forward * 5.0f, Quaternion.Euler(-90, transform.rotation.eulerAngles.y, 0),transform);
                GetComponent<PlayerSoundPlay>().PlayerBigSound1_Male();
            }
            else if(Input.GetKeyDown(KeyCode.F) && swordjudgementCoolTime < 10.0f)
                GetComponent<PlayerSoundPlay>().SkillCoolTime();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (isSwordJudge)
            {
                anim.SetInteger("Skill", (int)Skill.swordjudgementend);
                isSwordJudge= false;
            }
        }
    }

    private void SkillCoolTime()
    {
        if (!isSlash && lightswordslashCoolTime <= 3.0f)
            lightswordslashCoolTime += Time.deltaTime;

        if (!whirlwindOn && (whirlwindCoolTime <= 4.0f)) //�����尡 ������ ��Ÿ���� ���� �ʾ�����
            whirlwindCoolTime += Time.deltaTime;

        if (!isMultiSlash && (multislashCoolTime <= 5.0f)) //���� �˱� �����Ⱑ ������ ��Ÿ���� ���� �ʾ�����
        {
            multislashCoolTime += Time.deltaTime;
        }

        if (!isSwordJudge && swordjudgementCoolTime <= 10.0f) //�ñرⰡ ������ ��Ÿ���� ���� �ʾ�����
        {
            swordjudgementCoolTime += Time.deltaTime;
        }
    }

    public bool IsSwordJudge()
    {
        return isSwordJudge;
    }

    public void NextAttack() //�޺����� �Ұ�����
    {
        GetComponent<WeaponSheath>().Weapon_Col_AtkEnd();
        normalAttack = false;
        if (nextAttack)
            anim.SetInteger("Skill", (int)Skill.attack);
        else
        {
            if(!isSlash && !isWhirlWind)
                anim.SetInteger("Skill", (int)Skill.stop);
        }
    }

    public void SkillFinish() //��ų ���� �ʱ�ȭ(�ݶ��̴� ����)
    {
        isSlash = false;
        isWhirlWind = false;
        whirlwindOn = false;
        anim.SetInteger("Skill", (int)Skill.stop);
        lightswordSlash.GetComponent<ParticleSystem>().Stop();
        GetComponent<WeaponSheath>().Weapon_Col_AtkEnd();
        stateManager.AttackEnd();
        anim.speed = 1.0f;
    }

    public void OnSkill() //��ų ����(�ݶ��̴� ��)
    {
        GetComponent<WeaponSheath>().Weapon_Col_Attack();
        SwordPos.position = originswordTr.position;
        SwordPos.rotation = originswordTr.rotation;
    }
    #endregion

    #region Weapon_Position/Dash
    public void Dash()
    {
        isSlash = false;
        isWhirlWind = false;
        isMultiSlash = false;
        anim.speed = 1.5f;
        comboCount = 0;
        stateManager.AttackEnd();
        SwordPos.GetComponent<BoxCollider>().enabled = false;
        anim.SetInteger("Skill", (int)Skill.stop);
        SwordPos.position = originswordTr.position;
        SwordPos.rotation = originswordTr.rotation;
    }

    //������ �⺻ position(Animtion Event)
    public void SwordOrigin()
    {
        stateManager.SetDASH(false);
        isSlash = false;
        isWhirlWind = false;
        isMultiSlash = false;
        isSwordJudge = false;
        anim.SetInteger("Skill", (int)Skill.stop);
        anim.applyRootMotion = true;
        anim.speed = 1.0f;
        stateManager.AttackEnd();
        comboCount = 0;
        SwordPos.position = originswordTr.position;
        SwordPos.rotation = originswordTr.rotation;
        GetComponent<WeaponSheath>().BayonetState();
    }

    //�޺� ���� (Animation Event)
    public void AttackStart()
    {
        normalAttack = true;
        GetComponent<WeaponSheath>().Weapon_Col_Attack();
        nextAttack = false;
        anim.speed = atkSpeed;
        stateManager.Attack();

        if (comboCount == 0) //ù ����
        {
            AudioManager.Instance.PlaySFX(3, 1f); //playersound_male_attack1
            ++comboCount;
            SwordPos.position = swordPos[0].position;
            SwordPos.rotation = swordPos[0].rotation;

        }
        else if (comboCount == 1)  //�� ��° ����
        {
            AudioManager.Instance.PlaySFX(4, 1f); //playersound_male_attack2
            ++comboCount;
            SwordPos.position = swordPos[1].position;
            SwordPos.rotation = swordPos[1].rotation;
        }
        else //�� ��° ����
        {
            AudioManager.Instance.PlaySFX(5, 1f); //playersound_male_attack3
            comboCount = 0;
            SwordPos.position = swordPos[2].position;
            SwordPos.rotation = swordPos[2].rotation;
        }
    }

    // ���� Ʈ����
    public void AttackTrail()
    {
        
        if (comboCount == 1)
        {
            AudioManager.Instance.PlaySFX(0, 1f); //MissHit
            attack1Trail.Play();
        }
        else if (comboCount == 2)
        {
            AudioManager.Instance.PlaySFX(1, 1f); //SwordOnGround
            attack2Trail.Play();
            attack2Trail.transform.parent = null;
        }
        else
        {
            AudioManager.Instance.PlaySFX(1, 1f);
            attack3Trail.Play();
            attack3Trail.transform.parent = null;
        }
    }

    public void WhirlwindAttackTrail()
    {
        attack4Trail.Play();
        attack4Trail.transform.parent = null;
    }

    public void AttackTrailEnd()
    {
        attack1Trail.Stop();
        attack2Trail.Stop();
        attack3Trail.Stop();
        attack4Trail.Stop();
        Invoke("AttackTrailReset", 2.5f);
    }

    private void AttackTrailReset()
    {
        if (attack2Trail.transform.parent == null)
        {
            attack2Trail.transform.SetParent(transform.GetChild(14));
            attack2Trail.transform.SetSiblingIndex(1);
            attack2Trail.transform.position = transform.position + transform.up * 2.0f + transform.forward;
            attack2Trail.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - 90, transform.rotation.eulerAngles.z);
        }
        else if (attack3Trail.transform.parent == null)
        {
            attack3Trail.transform.SetParent(transform.GetChild(14));
            attack3Trail.transform.SetSiblingIndex(2);
            attack3Trail.transform.position = transform.position + transform.up * 2.0f + transform.forward;
            attack3Trail.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - 90, transform.rotation.eulerAngles.z);
        }
        else if (attack4Trail.transform.parent == null)
        {
            attack4Trail.transform.SetParent(transform.GetChild(14));
            attack4Trail.transform.SetSiblingIndex(3);
            attack4Trail.transform.position = transform.position + transform.up * 2.0f + transform.forward;
            attack4Trail.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - 90, transform.rotation.eulerAngles.z);
        }
    }
    #endregion

    public bool IsMultiSlash()
    {
        return isMultiSlash;
    }

    public void AttackEnd()
    {
        anim.speed = 1.0f;
        stateManager.AttackEnd();
    }
}

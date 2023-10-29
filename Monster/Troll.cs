using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(MonsterAnimation))]
public class Troll : MonsterMovement
{
    private MonsterManager monsterManager;
    //���� ������ �� 
    private MonsterData trollData;
    //���;ִϸ��̼�
    private MonsterAnimation monsterAni;
    //NaveMeshAgent
    private NavMeshAgent navMesh;

    private PlayerStateManager stateManager;
    private Transform playerTr;
    public GameObject DMGText;
    private Animator trollanim;

    protected override float moveSpeed { get; set; }
    protected override bool isMoving { get; set; }
    protected override bool isTrace { get; set; }
    protected override bool isAttack { get; set; }
    protected override bool isDamaged { get; set; }
    protected override bool isDead { get; set; }
    protected override Transform playerTransform { get; set; }
    private Vector3 originPos;

    //���� ü�� 
    private Slider slider;
    private float trollCurrentHP = 0f;

    //���� ���� ��Ÿ��
    private float trollAttackTimer = 0f;
    //���� ���� ����
    private float attackRange = 1.5f;
    //���� ���� ����
    private float traceRange = 20f;

    //�ݶ��̴��� ������ �÷��̾��� ���ݷ°� ������ ����
    private SphereCollider sphereCollider;
    //private bool debuffapplied = false;
    private int debuffActiveNum = 0;
    private int debuffDEFAmount = 0;
    private int debuffATKAmount = 0;

    private AudioSource audioSource;
    private MonsterAudioClip audioClip;
    int dirty = 0;
    private bool animDone = false;

    private void Awake()
    {
        //���
        trollanim = GetComponent<Animator>();
        originPos = this.transform.position;
        slider = GetComponentInChildren<Slider>();
        monsterAni = GetComponent<MonsterAnimation>();
        audioSource = GetComponent<AudioSource>();
        audioClip = GetComponent<MonsterAudioClip>();
        navMesh = GetComponent<NavMeshAgent>();
        monsterManager = FindObjectOfType<MonsterManager>();
        sphereCollider = transform.GetChild(0).GetComponent<SphereCollider>();
        
    }

    private void Start()
    {
        //������ Null �˻�
        if (trollData == null)
        {
            trollData = monsterManager.GetMonsterData(MonsterType.Troll);
        }
        else if (trollData != null)
        {
            Debug.Log("trollData�� �̹� �����մϴ�.");
        }
        trollCurrentHP = trollData.GetMaxHP();
        moveSpeed = monsterManager.GetMonsterData(MonsterType.Troll).GetSPEED();
        navMesh.speed += moveSpeed;
        //�÷��̾� ��ġ
        playerTransform = FindObjectOfType<PlayerMovement>().transform;
        //�÷��̾� ���� 
        stateManager = FindObjectOfType<PlayerStateManager>();
    }

    void Update()
    {
        if (!isDead)
            TrollState();
    }

    public void TrollState()
    {
        float dir = Vector3.Distance(this.transform.position, playerTransform.position);
        isTrace = dir <= traceRange && !isAttack;
        isAttack = dir < attackRange;
        isMoving = navMesh.velocity.magnitude > 0.1f;
        trollAttackTimer += Time.deltaTime;

        //�ִϸ��̼� ��� �� ��Ȳ�� ���� ���� �ӵ�����
        if (isTrace)
        {
            MoveToward();
            monsterAni.Run();
            moveSpeed = 20f;    
            navMesh.acceleration = 100f;
            navMesh.angularSpeed = 300f;
            this.transform.rotation.SetLookRotation(playerTransform.transform.position);
        }
        else if (isAttack)
        {
            navMesh.destination = this.transform.position;
            if (trollAttackTimer > 2f && !isTrace)
            {
                //audioSource.Play();
                monsterAni.Attack();
                trollAttackTimer = 0f;
            }
            else
            {
                monsterAni.Idle();
            }
        }
        else if (isMoving && !isTrace)
        {
            moveSpeed = 1f;
            navMesh.acceleration = 1f;
            navMesh.angularSpeed = 100f;
            monsterAni.Walk();
        }
        else
        {
            MoveToOrigin();
            monsterAni.Idle();
        }
    }

    //�÷��̾ ã�Ƽ� ����
    protected override void MoveToward()
    {
        navMesh.destination = playerTransform.position;
    }
    
    //���ڸ��� �ǵ��ư�
    protected override void MoveToOrigin()
    {
        navMesh.destination = originPos;
    }

    //���Ͱ� �׾��� ��
    protected override void MonsterDead()
    {
        if (dirty == 0)
        {
            dirty++;
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[3];
            audioSource.Play();
        }
        GetComponent<Collider>().enabled = false;
        DisableChildColliders(transform);
        isAttack = false;
        isDead = true;
        trollanim.SetBool("Die", true);
        //Debug.Log
        gameObject.tag = "Untagged";
        navMesh.isStopped = true;
        navMesh.height = 0;
        navMesh.radius = 0;
        monsterAni.Dead();
        stateManager.SetEXP(trollData.GetEXP());
        Destroy(this.gameObject, 20f);
        StartCoroutine(WaitForDeadAnimation());
        
    }


    //������ sphereCollider�� �ӹ��������� �÷��̾� 20%��ŭ �����(��ø)
    private void OnTriggerStay(Collider col)
    {
        if(debuffActiveNum == 0)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                if (!audioSource.isPlaying && dirty == 0)
                {
                    audioSource.volume = 0.5f;
                    audioSource.clip = audioClip.clip[2];
                    audioSource.Play();
                }
                debuffActiveNum++;
                //debuffapplied = true;
                PlayerDebuff();
            }
        }
    }
    //sphereCollider�� ������ �� ����� ���� 
    private void OnTriggerExit(Collider col)
    {
        if(debuffActiveNum > 0)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                //debuffapplied = false;
                PlayerRemoveDebuff();
                debuffActiveNum--;
            }
        }
    }
    //�÷��̾� ����� 
    private void PlayerDebuff()
    {
        int debuffATK = Mathf.RoundToInt(stateManager.GetATK() * 0.2f);
        int debuffDEF = Mathf.RoundToInt(stateManager.GetDEF() * 0.2f);
        debuffDEFAmount = debuffDEF;
        debuffATKAmount = debuffATK;

        int reducedDEF = stateManager.GetDEF() - debuffDEFAmount;
        int reduceATK = stateManager.GetATK() - debuffATKAmount;

        stateManager.SetDEF(reducedDEF - (9 + (1 * stateManager.StatManager())));
        stateManager.SetATK(reduceATK - (9 + (1 * stateManager.StatManager())));
    }
    //�÷��̾� ����� ����
    private void PlayerRemoveDebuff()
    {
        int originalDEF = stateManager.GetDEF() + debuffDEFAmount;
        int originalATK = stateManager.GetATK() + debuffATKAmount;

        stateManager.SetDEF(originalDEF - (9 + (1 * stateManager.StatManager()))); //���� ���ݷ� �� �����ؾߵ�
        stateManager.SetATK(originalATK - (9 + (1 * stateManager.StatManager()))); //���� ���ݷ� �� �����ؾߵ�

        debuffDEFAmount = 0;
        debuffATKAmount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerDamage>())
        {
            HitTroll((int)other.transform.GetComponent<PlayerDamage>().DMG(), other.transform);
        }

        if (other.CompareTag("Player") && isAttack)
            other.GetComponent<PlayerState>().HitPlayer(trollData.GetATK(), false);
    }

    private void TrollHpSlider()
    {
        slider.maxValue = (float)trollData.GetMaxHP();
        slider.value = trollCurrentHP;
    }

    private void HitTroll(float damage, Transform _pos)
    {
        if (!audioSource.isPlaying && dirty == 0)
        {
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[1];
            audioSource.Play();
        }
        isDamaged = true;
        trollCurrentHP -= (int)(damage * 0.8f);
        GameObject dmgtext = Instantiate(DMGText, _pos.position, playerTransform.rotation);
        dmgtext.GetComponent<TextMeshPro>().color = new Color(1, 0, 0);
        dmgtext.GetComponent<TextMeshPro>().fontSize = 15;
        dmgtext.GetComponent<DamageText>().damage = (int)(damage * 0.8f);
        monsterAni.Damaged();

        TrollHpSlider();
        if (trollCurrentHP <= 0)
        {
            isDead = true;
            MonsterDead();
        }
    }

    public void Attack()
    {
        audioSource.volume = 0.5f;
        audioSource.clip = audioClip.clip[0];
        audioSource.Play();
    }

    IEnumerator WaitForDeadAnimation()
    {
        yield return new WaitForSeconds(2.933f);
        animDone = true;
        if (animDone)
        {
            GameObject meshObject = transform.GetChild(3).gameObject;
            GameObject thisObject = this.gameObject;
            if (meshObject.name == "Mesh_Body")
            {
                meshObject.transform.position = transform.position;
                meshObject.transform.rotation = transform.rotation;
                meshObject.transform.parent = default;
            }
            for (int i = 0; i < thisObject.transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void DisableChildColliders(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // �ڽ� ������Ʈ�� ���ؼ��� ��������� ȣ��
            DisableChildColliders(child);
        }
    }

}

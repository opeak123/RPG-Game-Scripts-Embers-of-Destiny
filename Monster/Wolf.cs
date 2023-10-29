using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(MonsterAnimation))]
public class Wolf : MonsterMovement
{
    private MonsterManager monsterManager;
    //���� ������ �� 
    private MonsterData wolfData;
    //���;ִϸ��̼�
    private MonsterAnimation monsterAni;
    //NaveMeshAgent
    private NavMeshAgent navMesh;

    private PlayerStateManager stateManager;
    private Transform playerTr;
    public GameObject DMGText;
    protected override float moveSpeed { get; set; }
    protected override bool isMoving { get; set; } = false;
    protected override bool isTrace { get; set; } = false;
    protected override bool isAttack { get; set; } = false;
    protected override bool isDamaged { get; set; } = false;
    protected override bool isDead { get; set; } = false;
    protected override Transform playerTransform { get; set; }
    private Vector3 originPos;

    //���� ü�� 
    private Slider slider;
    private float wolfCurrentHP = 0f;
    //�������� üũ
    private float combatState = 0f;
    //���� ���� ��Ÿ��
    private float wolfAttackTimer = 0f;
    //���� ���� ����
    private float attackRange = 1.5f;
    //���� ���� ����
    private float traceRange = 20f;
    //�ٸ� ���͸� ������ ������
    private float detectionRadius = 7f;
    //������ �� Wolf ���� �� 
    private int monsterNum = 1;


    private AudioSource audioSource;
    private MonsterAudioClip audioClip;
    int dirty = 0;


    private void Awake()
    {
        //���
        originPos = this.transform.position;
        slider = GetComponentInChildren<Slider>();
        monsterAni = GetComponent<MonsterAnimation>();
        audioSource = GetComponent<AudioSource>();
        audioClip = GetComponent<MonsterAudioClip>();
        navMesh = GetComponent<NavMeshAgent>();
        monsterManager = FindObjectOfType<MonsterManager>();
        
    }

    private void Start()
    {
        //������ Ȯ��
        if (wolfData == null)
        {
            wolfData = monsterManager.GetMonsterData(MonsterType.Wolf);
        }
        else if (wolfData != null)
        {
            Debug.Log("wolfData�� �̹� �����մϴ�.");
        }
        wolfCurrentHP = wolfData.GetMaxHP();
        moveSpeed = monsterManager.GetMonsterData(MonsterType.Wolf).GetSPEED();
        navMesh.speed += moveSpeed;
        //�÷��̾� ��ġ
        playerTransform = FindObjectOfType<PlayerMovement>().transform;
        //�÷��̾� ����
        stateManager = FindObjectOfType<PlayerStateManager>();
    }

    private void Update()
    {
        if (isDead)
            return;

        WolfFarFromPlayer();
        DetectOtherMonster();
        WolfState();
    }

    //�÷��̾� ����
    protected override void MoveToward()
    {
        navMesh.destination = playerTransform.position;
    }
    //���Ͱ� �����ڸ��� �ǵ��ư�
    protected override void MoveToOrigin()
    {
        navMesh.destination = originPos;
    }

    //�÷��̾�� �Ÿ� ���
    private void WolfState()
    {
        float dir = Vector3.Distance(this.transform.position, playerTransform.position);
        isTrace = dir <= traceRange && !isAttack;
        isAttack = dir < attackRange;
        isMoving = navMesh.velocity.magnitude > 0.1f;
        wolfAttackTimer += Time.deltaTime;

        if (isDamaged)
            return;

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
        else if(isAttack)
        {
            navMesh.destination = this.transform.position;
            if(wolfAttackTimer > 2f/* && !isTrace*/)
            {
                monsterAni.Attack();
                wolfAttackTimer = 0f;
            }
            else
            {
                monsterAni.Idle();
            }
        }
        else if(isMoving && !isTrace)
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
    //�����̴��� �ִ�ü�� ����
    private void WolfHpSlider()
    {
        slider.maxValue = (float)wolfData.GetMaxHP();
        slider.value = wolfCurrentHP;
    }

    //ȣ�� �Լ�
    //������ ������ �޾�����
    private void HitWolf(float damage, Transform _pos)
    {
        isDamaged = true;
        wolfCurrentHP -= damage;
        if (!audioSource.isPlaying && dirty == 0)
        {
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[1];
            audioSource.Play();
        }
        wolfCurrentHP -= (int)(damage * 0.8f);
        GameObject dmgtext = Instantiate(DMGText, _pos.position, playerTransform.rotation);
        dmgtext.GetComponent<TextMeshPro>().color = new Color(1, 0, 0);
        dmgtext.GetComponent<TextMeshPro>().fontSize = 15;
        dmgtext.GetComponent<DamageText>().damage = (int)(damage * 0.8f);
        monsterAni.Damaged();

        WolfHpSlider();
        if (wolfCurrentHP <= 0)
        {
            isDead = true;
            MonsterDead();
        }
        if(combatState > 10)
        {
            isDamaged = false;
        }
    }
    //�÷��̾�� �ǰݽ� �ݴ�������� ����
    private void WolfFarFromPlayer()
    {
        if (isDamaged)
        {
            Vector3 farFromPlayer = transform.position + 
                (transform.position - playerTransform.position).normalized;

            navMesh.SetDestination(farFromPlayer);
            monsterAni.Run();
            combatState += Time.deltaTime;
            if (combatState > 10f)
            {
                isDamaged = false;
                combatState = 0f;
            }
        }
    }
    //���Ͱ� �׾��� ���
    protected override void MonsterDead()
    {
        if (dirty == 0)
        {
            dirty++;
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[2];
            audioSource.Play();
            monsterAni.Dead();
        }
        DisableChildColliders(transform);
        transform.GetComponent<Collider>().enabled = false;
        gameObject.tag = "Untagged";
        navMesh.isStopped = true;
        navMesh.height = 0;
        navMesh.radius = 0;
        navMesh.acceleration = 0;
        stateManager.SetEXP(wolfData.GetEXP());
        Destroy(this.gameObject, 20f);

        GameObject thisObject = this.gameObject;
        for (int i = 0; i < thisObject.transform.childCount; i++)
        {
            GameObject meshObject = transform.GetChild(3).gameObject;
            if (meshObject.name == "Mesh_Body")
            {
                meshObject.transform.parent = default;
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    //������ �� ���� ���� ����
    private void DetectOtherMonster()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        monsterNum = 1;
        foreach (Collider col in colliders)
        {
            if (col.gameObject.CompareTag("WOLF") && col.name != this.name)
            {
                monsterNum++;
            }
        }
        if (monsterNum != 1)
        {
            isDamaged = false;
        }
        else if (monsterNum == 1)
        {
            WolfFarFromPlayer();
        }
    }

    //�������� ������ �׸�
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerDamage>())
        {
            HitWolf((int)other.transform.GetComponent<PlayerDamage>().DMG(), other.transform);
        }

        if (other.CompareTag("Player") && isAttack)
        {
            other.GetComponent<PlayerState>().HitPlayer((float)wolfData.GetATK(), true);
        }
    }
    public void Attack()
    {
        audioSource.volume = 0.5f;
        audioSource.clip = audioClip.clip[0];
        audioSource.Play();
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

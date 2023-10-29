using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(MonsterAnimation))]
public class Goblin : MonsterMovement
{
    //���͸Ŵ��� 
    private MonsterManager monsterManager;
    //���� ������ �� 
    private MonsterData goblinData;
    //���;ִϸ��̼�
    private MonsterAnimation monsterAni;
    //NaveMeshAgent
    private NavMeshAgent navMesh;

    private AudioSource audioSource;
    private MonsterAudioClip audioClip;

    private PlayerStateManager stateManager;
    private Transform playerTr;
    public GameObject DMGText;
    //�߻� ���
    protected override bool isAttack { get; set; } = false;
    protected override bool isMoving { get; set; } = true; 
    protected override bool isTrace { get; set; } = false;
    protected override bool isDamaged { get; set; } = false;
    protected override bool isDead { get; set; } = false;
    protected override float moveSpeed { get; set; }
    protected override Transform playerTransform { get; set; }

    //�÷��̾���� �Ÿ� ����
    private Vector3 originPos;
    private float dirXZ;
    private float dirY;

    //���� ������Ÿ��
    private float goblinAttackTimer;

    //���� ü��
    private Slider slider;
    private float goblinCurrentHP;

    //���� ü�� �ڵ�ȸ��
    private float recoveryHpTimer;
    private void Awake()
    {
        //�Ҵ�
        originPos = this.transform.position;
        slider = GetComponentInChildren<Slider>();
        audioSource = GetComponent<AudioSource>();
        audioClip = GetComponent<MonsterAudioClip>();
        monsterAni = GetComponent<MonsterAnimation>();
        navMesh = GetComponent<NavMeshAgent>();
        monsterManager = FindObjectOfType<MonsterManager>();
        
    }

    private void Start()
    {
        //������ Ȯ��
        if (goblinData == null)
        {
            goblinData = monsterManager.GetMonsterData(MonsterType.Goblin);
        }
        else if(goblinData != null)
        {
            Debug.Log("GolbinData�� �̹� �����մϴ�.");
        }
        //������ �� ���� 
        goblinCurrentHP = goblinData.GetMaxHP();
        moveSpeed = monsterManager.GetMonsterData(MonsterType.Goblin).GetSPEED();
        navMesh.speed += moveSpeed;
        //�÷��̾� ��ġ
        playerTransform = FindObjectOfType<PlayerMovement>().transform;
        //�÷��̾� ����
        stateManager = FindObjectOfType<PlayerStateManager>();
        playerTr = GameObject.FindWithTag("Player").transform;
    }
    private void Update()
    {
        //print("moving" + isMoving);
        //print("trace" + isTrace);
        //print("attack" + isAttack);
        //if(Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    slider.value -= 0.2f;
        //    Debug.Log("����");
        //}

        if (this.isDead)
            return;
        //���Ͱ� �÷��̾��� �����̼��� �ٶ󺸰� ��
        this.transform.rotation.SetLookRotation(playerTransform.transform.position);
        //�÷��̾���� �Ÿ����
        GoblinState();
    }
    //���Ϳ� �÷��̾��� �Ÿ� �����ؼ� ��ã��
    protected override void MoveToward()
    {
        navMesh.destination = playerTransform.position;
    }
    //���Ͱ� �����ڸ��� �ǵ��ư�
    protected override void MoveToOrigin()
    {
        navMesh.destination = originPos;
    }
    
    private void GoblinState()
    {
        //�÷��̾�� ������ �������� X,Z�� ���
        dirXZ = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
            new Vector2(playerTransform.position.x, playerTransform.position.z));

        //�÷��̾�� ������ �������� Y�� ���
        dirY = Mathf.Abs(transform.position.y - playerTransform.position.y);
        //���� ���� ��Ÿ��
        goblinAttackTimer += Time.deltaTime;
        //Bool ���� : ����/����/�̵�������
        isTrace = dirXZ <= 20f && dirY <= 2f && !isAttack;
        isAttack = dirXZ < 1.5f;
        isMoving = navMesh.velocity.magnitude > 0.1f;

        //�ִϸ��̼� ���
        switch (isTrace)
        {
            case true:
                MoveToward();
                monsterAni.Run();
                break;

            case false:
                if(isMoving)
                {
                    monsterAni.Walk();
                }
                break;
        }

        //�ִϸ��̼� ���
        switch (isAttack)
        {
            case true:
                if (isAttack)
                {
                    navMesh.destination = this.transform.position;
                    if (goblinAttackTimer >= 3 && !isTrace && !isMoving)
                    {
                        monsterAni.Attack();
                        audioSource.volume = 0.5f;
                        audioSource.clip = audioClip.clip[0];
                        audioSource.Play();
                        goblinAttackTimer = 0f;
                    }
                    else
                    {
                        monsterAni.Idle();
                    }
                }
                break;
        }
        //�ִϸ��̼� ���
        if (isMoving)
        {
            monsterAni.Walk();
        }
        else if(!isMoving && !isTrace && !isAttack)
        {
            MoveToOrigin();
            monsterAni.Idle();
            //RecoveryHP();
        }
    }

    //������ ���������� �����̴��� �ִ�ü�� ����
    private void GoblinHpSlider()
    {
        slider.maxValue = (float)goblinData.GetMaxHP();
        slider.value = goblinCurrentHP;
    }

    //������� �������� ������ ��
    private void HitGoblin(float damage, Transform _pos)
    {
        if(!audioSource.isPlaying && dirty == 0)
        {
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[1];
            audioSource.Play();
        }
        goblinCurrentHP -= damage;
        goblinCurrentHP -= (int)(damage * 0.8f);
        monsterAni.Damaged();
        GameObject dmgtext = Instantiate(DMGText, _pos.position, playerTr.rotation);
        dmgtext.GetComponent<TextMeshPro>().color = new Color(1, 0, 0);
        dmgtext.GetComponent<TextMeshPro>().fontSize = 15;
        dmgtext.GetComponent<DamageText>().damage = (int)(damage * 0.8f);

        GoblinHpSlider();
        if (goblinCurrentHP <= 0)
        {
            isDead = true;
            MonsterDead();
        }
    }
    int dirty = 0;
    //����� �׾����� State
    protected override void MonsterDead()
    {
        if(dirty == 0)
        {
            dirty++;
            audioSource.volume = 1f;
            audioSource.clip = audioClip.clip[2];
            audioSource.Play();
        }
        DisableChildColliders(transform);
        transform.GetComponent<Collider>().enabled = false;
        gameObject.tag = "Untagged";
        navMesh.isStopped = true;
        navMesh.height = 0;
        navMesh.radius = 0;
        monsterAni.Dead();
        stateManager.SetEXP(goblinData.GetEXP());
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

    //�������°� �ƴ϶�� ü�� �ڵ�ȸ��
    private void RecoveryHP()
    {
        recoveryHpTimer += Time.deltaTime;
        //float amount = goblinData.GetMaxHP() / 99f;
        float amount = goblinData.GetMaxHP() * 0.0002f;
        if (recoveryHpTimer > 0.5f)
        {
            //slider.value += amount;
            slider.value = Mathf.Min(slider.value + amount,goblinData.GetMaxHP());
            recoveryHpTimer = 0f;
        }
    }


    //private void RecoveryHP()
    //{
    //    recoveryHpTimer += Time.deltaTime;
    //    //float amount = goblinData.GetMaxHP() / 99f;
    //    float amount = goblinData.GetMaxHP() * 0.0002f;
    //    if (recoveryHpTimer > 0.5f)
    //    {
    //        //slider.value += amount;
    //        slider.value = Mathf.Min(slider.value + amount, goblinData.GetMaxHP());
    //        recoveryHpTimer = 0f;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerDamage>())
        {
            HitGoblin(other.transform.GetComponent<PlayerDamage>().DMG(), other.transform);
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
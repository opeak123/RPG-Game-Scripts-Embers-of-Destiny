using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ReaperMovement : MonoBehaviour
{
    //Reaper �ִϸ��̼�
    private ReaperAnimation reaperAnimation;
    //Reaper Rigidbody
    private Rigidbody rigidBody;
    //�÷��̾� ��ġ
    private Transform playerTransform;
    //�÷��̾� �Ÿ� üũ
    private float distance;
    //�ڷ���Ʈ �÷��̾���� �Ÿ�
    private float teleportDistance = 4f;
    //�ڷ���Ʈ ��Ÿ��
    private float teleportCoolDown = 6f;
    //�ڷ���Ʈ Ÿ�̸�
    private float teleportTimer = 0f;
    //�ڷ���Ʈ ��� ���� ����
    private bool teleportOn = false;
    //���Ÿ����� spawn Ÿ�̸�
    private float spawnTimer = 0f;
    //�÷��̾� ��������
    private bool isWalk = false;
    //�÷��̾� ���ݿ���
    private bool isAttack = false;
    //���� ��ȯ ���ɿ���
    private bool isSpawn = false;
    //��ȯ ��Ÿ���� �Ϸ�ƴ��� �˷��ִ� ����
    private bool isSpawnPossible = false;
    //����� �ҽ�
    public AudioSource[] audioSource = new AudioSource[2];
    private MonsterAudioClip audioClip;
    //�ൿ ����
    bool IsStop = false;

    //���� ���� ��Ÿ��
    private float spawnCoolDown = 10f;
    //�ڷ���Ʈ �� �� ����ϴ� ��ƼŬ
    public ParticleSystem teleportParticle;
    //Indicator ���ӿ�����Ʈ
    public GameObject spawnSprite;
    //���� ������ 
    public GameObject[] spawnPrefab = new GameObject[3];
    //���� ���� �ݰ� ����
    private float spawnRadius = 10f;
    [SerializeField]
    [Range(1f, 20f)]
    private float moveSpeed = 5f;
    private bool reaperDead = false;
    public bool ReaperDead(bool value)
    {
        reaperDead = value;
        return reaperDead;
    }

    /// <summary>
    ///  Nova Particle Obj Pool
    /// </summary>

    //����� ��ƼŬ
    public ParticleSystem novaParticle;
    //������ƮǮ�� �θ�
    public Transform novaParent;
    [SerializeField]
    //Ǯ�� ������
    private int novaPoolSize = 5;
    //��ƼŬ�� ���� ����Ʈ
    private List<ParticleSystem> novarParticleList;
    //��ų ��Ÿ��
    private float skillColltime = 6f;
    //��ų Ÿ�̸�
    private float skillTimer = 0f;
    //��ų ����� �� �ִ��� ����
    private bool skillOn = false;

    //��Ʈ�� �����ٸ� ����,���� Ȱ��ȭ
    private bool reaperActivated = false;
    public bool Activated(bool activated)
    {
        reaperActivated = activated;
        return reaperActivated;
    }
    private void Awake()
    {
        CreateNovaParticlePooling();
    }
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        rigidBody = GetComponent<Rigidbody>();
        reaperAnimation = GetComponent<ReaperAnimation>();
        audioClip = GetComponent<MonsterAudioClip>();
        StartCoroutine(ResetTimer());
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,spawnRadius);
    }
    void Update()
    {
        print(reaperActivated);
        if (reaperDead) 
            return;
        //reaperDead = GetComponent<ReaperHpBarUI>().Dead();
        //print("�ڷ���Ʈ"+ teleportOn);
        //print("��ȯ" + isSpawnPossible);
        //print("��ų" + skillOn);
        ReaperMovementUpdate();
    }

    private void ReaperMovementUpdate()
    {
        if (reaperActivated && IsStop && !reaperDead)
            return;

        distance = Vector3.Distance(transform.position, playerTransform.position);
        isWalk = distance >= 4.5f && !isAttack && !teleportOn;
        isAttack = distance <= 4.5f && !isWalk;
        isSpawn = distance >= 4.5f && !isAttack && !isWalk;

        if (isWalk)
        {
            ReaperTracePlayer();
            Teleport();
            SpawnEnemy();

            if (GetComponent<ReaperHpBarUI>().reaperPhase == ReaperPhase.Phase2)
            {
                //Debug.Log("this is phase 2 ");
                Skill();
            }
        }
        else if(isAttack)
        {
            ReaperAttackPlayer();
        }
        else if(!isAttack)
        {
            reaperAnimation.ReaperResetAttack();
        }

    }

    private void ReaperTracePlayer()
    {
        reaperAnimation.ReaperForward();
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        transform.LookAt(playerTransform);
    }
    private void ReaperAttackPlayer()
    {
        reaperAnimation.ReaperAttack();
        transform.position = transform.position;
    }

    private void Teleport()
    {
        if (distance >= 4.5f)
        {
            if (!teleportOn && !isSpawnPossible && !skillOn)
            {
                teleportTimer += Time.deltaTime;
                if (teleportTimer > teleportCoolDown)
                {
                    float value = Random.value;
                    if (value <= 0.2f)
                    {
                        StartCoroutine(ReaperTeleportToPlayer());
                        teleportTimer = 0f;
                    }
                }
            }
        }
    }
    IEnumerator ReaperTeleportToPlayer()
    {
        teleportOn = true;
        audioSource[0].clip = audioClip.clip[0];

        rigidBody.constraints &=
            ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ);

        Vector3 teleportPosition = playerTransform.position 
            + playerTransform.forward * teleportDistance;

        yield return new WaitForSeconds(0.5f);
        audioSource[0].Play();
        teleportParticle.Play();
        transform.GetChild(1).GetComponent<Collider>().isTrigger = true;
        rigidBody.MovePosition(teleportPosition / 1.5f);

        yield return new WaitForSeconds(0.5f);
        audioSource[0].Play();
        teleportParticle.Play();
        Vector3 offset = playerTransform.forward * teleportDistance;
        Vector3 targetPosition = playerTransform.position + offset;
        rigidBody.MovePosition(targetPosition);

        yield return null;
        transform.rotation = Quaternion.LookRotation(playerTransform.position 
            - transform.position);

        transform.GetChild(1).GetComponent<Collider>().isTrigger = false;

        yield return new WaitForSeconds(0.5f);
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        teleportOn = false;
    }

    private void SpawnEnemy()
    {
        if (distance >= 4.5f)
        {
            if (!isSpawnPossible && !teleportOn && !skillOn)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer > spawnCoolDown)
                {
                    float value = Random.value;
                    if (value <= 0.1f)
                    {
                        StartCoroutine(ReaperSpawnEnemy());
                        spawnTimer = 0f;
                    }
                }
            }
        }
    }

    IEnumerator ReaperSpawnEnemy()
    {
        isSpawnPossible = true;
        isWalk = false;

        int randomValue = Random.Range(1, 4);
        reaperAnimation.ReaperSkillA();

        IsStop = true;
        reaperAnimation.ReaperResetForward();
        reaperAnimation.ReaperIdle(true);
        transform.position = transform.position;

        GameObject go;
        //Indicator
        GameObject spot = spawnSprite;
        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 0f;


        switch (randomValue)
        {
            case 1: //���

                go = spawnPrefab[0].gameObject;
                GameObject goblinSpotObject = 
                    Instantiate(spot, spawnPosition, Quaternion.identity);

                Material goblinSpotMat =
                    goblinSpotObject.GetComponent<MeshRenderer>().material;

                Color goblinCurrentTintColor = goblinSpotMat.color;

                float timer = 0f;
                float targetAlpha = 1f;

                while (timer < 5f)
                {
                    timer += Time.deltaTime;
                    float t = timer / 5f;
                    float currentAlpha = Mathf.Lerp(0f, targetAlpha, t);

                    goblinCurrentTintColor.a = currentAlpha;
                    goblinSpotMat.color = goblinCurrentTintColor;

                    yield return null;
                }

                if (goblinCurrentTintColor.a == 1) 
                {
                    SpawnSound();
                    Instantiate(go, goblinSpotObject.transform.position, Quaternion.identity);
                    reaperAnimation.ReaperIdle(false);
                    IsStop = false;
                }
                if (go.activeInHierarchy) { Debug.Log(go.name + " ������"); };

                break;


            case 2: //Ʈ��
                go = spawnPrefab[1].gameObject;
                GameObject trollSpotObject = Instantiate(spot, spawnPosition, Quaternion.identity);
                Material trollSpotMat = trollSpotObject.GetComponent<MeshRenderer>().material;
                Color trollCurrentTintColor = trollSpotMat.color;

                timer = 0f;
                targetAlpha = 1f;

                while (timer < 5f)
                {
                    timer += Time.deltaTime;
                    float t = timer / 5f;
                    float currentAlpha = Mathf.Lerp(0f, targetAlpha, t);

                    trollCurrentTintColor.a = currentAlpha;
                    trollSpotMat.color = trollCurrentTintColor;

                    yield return null;
                }
                if (trollCurrentTintColor.a == 1)
                {
                    SpawnSound();
                    Instantiate(go, trollSpotObject.transform.position, Quaternion.identity);
                    reaperAnimation.ReaperIdle(false);
                    IsStop = false;
                }
                        
                if (go.activeInHierarchy) { Debug.Log(go.name + " ������"); };
                break;



            case 3: //����
                go = spawnPrefab[2].gameObject;
                GameObject wolfSpotObject = Instantiate(spot, spawnPosition, Quaternion.identity);
                Material wolfSpotMat = wolfSpotObject.GetComponent<MeshRenderer>().material;
                Color wolfCurrentTintColor = wolfSpotMat.color;

                timer = 0f;
                targetAlpha = 1f;

                while (timer < 5f)
                {
                    timer += Time.deltaTime;
                    float t = timer / 5f;
                    float currentAlpha = Mathf.Lerp(0f, targetAlpha, t);

                    wolfCurrentTintColor.a = currentAlpha;
                    wolfSpotMat.color = wolfCurrentTintColor;

                    yield return null;
                }
                if (wolfCurrentTintColor.a == 1)
                {
                    SpawnSound();
                    Instantiate(go, wolfSpotObject.transform.position, Quaternion.identity);
                    reaperAnimation.ReaperIdle(false);
                    IsStop = false;
                }        
                if (go.activeInHierarchy) { Debug.Log(go.name + " ������"); };

                break;

        }
        isSpawnPossible = false;
        yield return null;
    }


    private void CreateNovaParticlePooling()
    {
        novarParticleList = new List<ParticleSystem>();

        for (int i = 0; i < novaPoolSize; i++)
        {
            ParticleSystem particle = Instantiate(novaParticle);
            particle.transform.parent = novaParent;
            particle.gameObject.SetActive(false);
            novarParticleList.Add(particle);
        }
    }

    private ParticleSystem GetNovaParticle()
    {
        for(int i=0; i<novarParticleList.Count; i++)
        {
            if (!novarParticleList[i].gameObject.activeInHierarchy)
            {
                return novarParticleList[i];
            }
        }

        ParticleSystem particle = Instantiate(novaParticle);
        particle.gameObject.SetActive(true);
        novarParticleList.Add(particle);

        return particle;
    }



    private void Skill()
    {
        if (distance >= 4.5f)
        {
            if (!isSpawnPossible && !teleportOn && !skillOn)
            {
                skillTimer += Time.deltaTime;
                if (skillTimer > skillColltime)
                {
                    float value = Random.value;
                    if (value <= 0.2f)
                    {
                        StartCoroutine(ReaperNovaParticle());
                        skillTimer = 0f;
                    }
                }
            }
        }
    }

    IEnumerator ReaperNovaParticle()
    {
        
        IsStop = true;
        skillOn = true;

        transform.LookAt(playerTransform);
        Vector3 lookVector = (playerTransform.transform.position - transform.position).normalized;
        transform.rotation.SetLookRotation(lookVector);
        reaperAnimation.ReaperSkillB();

        float posY = transform.position.y;

        yield return new WaitForSeconds(2.1f);

        for (int i = 0; i < 5; i++)
        {
            ParticleSystem particle = GetNovaParticle();
            particle.transform.localScale = novaParticle.transform.localScale;
            float posZ = (i + 1)* 10f;
            //Vector3 posX = transform.forward;
            Vector3 offset = transform.forward * posZ;
            particle.transform.position = transform.position + offset;
            //particle.transform.position = new Vector3(posX.x + transform.position.x, 1f, transform.position.z + posZ) + (transform.forward);
            particle.gameObject.SetActive(true);
            audioSource[1].volume = 0.5f;
            audioSource[1].clip = audioClip.clip[6];
            audioSource[1].Play();

            yield return new WaitForSeconds(0.5f);
            audioSource[1].volume = 1f;
        }
        StartCoroutine(RetrunNovaParticle());

        IsStop = false;
        skillOn = false;
        yield return null;
    }

    IEnumerator RetrunNovaParticle()
    {
        for (int i = 0; i < novarParticleList.Count; i++)
        {
            if (novarParticleList[i].gameObject.activeInHierarchy)
            {
                novarParticleList[i].transform.position = transform.position;
                novarParticleList[i].gameObject.SetActive(false);
                yield return new WaitForSeconds(1f);
            }
        }
    }
    IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(4f);
        spawnTimer = 0f;
        teleportTimer = 0f;
    }

    public void SpawnSound()
    {
        audioSource[0].clip = audioClip.clip[1];
        audioSource[0].Play();
    }
}
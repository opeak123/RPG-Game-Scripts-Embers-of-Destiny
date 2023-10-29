using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody rig;
    Transform rayTr; //���� �߻� ��ġ
    Transform bowTr; //Ȱ�� ��ġ
    PlayerStateManager stateManager;
    SetSecondPaze cerberusSecondPaze;

    Ray ShootRay;
    RaycastHit ShootHit;

    private float range = 1000.0f; //���� ����
    public float AtkSpeed = 1.0f; //���ݼӵ�
    private float dueTime = 0.18f; //�߻������ �ð�(����)
    private float timer;

    private bool isFire = false;
    private bool isAttack = false;

    private int enemyMask;
    void Awake()
    {
        if(FindObjectOfType<SetSecondPaze>() != null)
            cerberusSecondPaze = FindObjectOfType<SetSecondPaze>();

        stateManager = FindObjectOfType<PlayerStateManager>();
        rig = GetComponent<Rigidbody>();
        rayTr = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        bowTr = GameObject.FindWithTag("Bow").transform;
        Invoke("Fire",dueTime/AtkSpeed);
        AtkSpeed = stateManager.GetATKSPEED();
    }

    //void Throw()
    //{
    //    transform.parent = null;
    //    rig.AddForce(transform.forward * 1000f);
    //}

    void Update()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        Debug.DrawRay(rayTr.position, rayTr.forward * 1000);

        timer += Time.deltaTime;
        if (timer >= 3.0f && !isAttack)
            Destroy(gameObject);

        if (!isFire)
            transform.LookAt(bowTr);
    }

    //�߻�
    void Fire()
    {
        isFire = true;
        transform.parent = null;
        ShootRay = new Ray(rayTr.position, rayTr.forward);
        if (Physics.Raycast(ShootRay, out ShootHit, range, enemyMask))
        {
            transform.LookAt(ShootHit.point);
            rig.AddForce(transform.forward * 1000 * AtkSpeed);
        }
        else
        {
            transform.LookAt(rayTr.forward * 1000);
            rig.AddForce(transform.forward * 1000f * AtkSpeed);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" || other.tag == "WOLF")
        {
            if(cerberusSecondPaze != null)
                if (cerberusSecondPaze.SecondPazing())
                    Destroy(gameObject);
            
            this.GetComponent<BoxCollider>().enabled = false;
            isAttack = true;
            transform.SetParent(other.transform);
            Destroy(gameObject, 2.0f);
        }
    }

}

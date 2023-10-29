using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSheath : MonoBehaviour
{
    [SerializeField]
    GameObject BackSocket; //���� ���� ��ġ
    [SerializeField]
    GameObject HandSocket; //���� ���� ��ġ
    [SerializeField]
    GameObject[] BackWeapons; //��� �˵��� �����յ�(��)
    [SerializeField]
    GameObject[] HandWeapons; //��� �˵��� �����յ�(��)

    Animator anim;
    Transform SwordEquipPos; //���⸦ ���� ���� ���� ��ġ
    Transform SwordOriginPos; //������ �⺻ ��ġ

    public int item_num = 0;
    private int normalLayer;
    private int battleLayer;
    private float bayonetCoolTime = 3.0f;
    //private int temp_num;
    private bool bayonet = false;
    private bool isWarrior = false;

    private enum bayonet_state
    {
        none,
        equiping,
        unarming,
        equiped,
        unarmed
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        
        if (gameObject.GetComponent<WarriorAttack>() != null)
            isWarrior = true;
        if (isWarrior)
        {
            normalLayer = anim.GetLayerIndex("Normal");
            battleLayer = anim.GetLayerIndex("Battle");
        }
    }

    private void Start()
    {
        for (int i = 0; i < BackWeapons.Length; i++)
        {
            BackWeapons[i] = GameObject.FindWithTag("Weapon_Back").transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < HandWeapons.Length; i++)
        {
            HandWeapons[i] = GameObject.FindWithTag("Weapon_Hand").transform.GetChild(i).gameObject;
        }
        if (isWarrior)
        {
            SwordEquipPos = GameObject.FindWithTag("SwordPos").transform.GetChild(7).transform;
            SwordOriginPos = GameObject.FindWithTag("SwordPos").transform.GetChild(0).transform;
        }

        BackSocket = BackWeapons[item_num];
        HandSocket = HandWeapons[item_num];

        if(isWarrior)
            anim.SetLayerWeight(normalLayer, 1);

        //temp_num = item_num;
        WeaponBayonet();
    }

    void Update()
    {
        if (bayonetCoolTime <= 3.0f)
            bayonetCoolTime += Time.deltaTime;

        if (isWarrior)
            IsBayonet_Warrior();
        else
            IsBayonet_Player();

        ChangeWeapon();
    }

    //���� ��ü
    void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BackSocket.SetActive(false);
            HandSocket.SetActive(false);
            item_num = 0;
            BackSocket = BackWeapons[0];
            HandSocket = HandWeapons[0];
            BackSocket.SetActive(bayonet);
            HandSocket.SetActive(!bayonet);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BackSocket.SetActive(false);
            HandSocket.SetActive(false);
            item_num = 1;
            BackSocket = BackWeapons[1];
            HandSocket = HandWeapons[1];
            BackSocket.SetActive(bayonet);
            HandSocket.SetActive(!bayonet);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BackSocket.SetActive(false);
            HandSocket.SetActive(false);
            item_num = 2;
            BackSocket = BackWeapons[2];
            HandSocket = HandWeapons[2];
            BackSocket.SetActive(bayonet);
            HandSocket.SetActive(!bayonet);
        }
    }

    //�ٸ� �÷��̾� ���˿���
    void IsBayonet_Player()
    {
        if (anim.GetInteger("Skill") == 0)
        {
            if (Input.GetKeyDown(KeyCode.X) && bayonetCoolTime >= 3.0f)
            {
                bayonetCoolTime = 0;
                if (bayonet == true)
                {
                    anim.SetInteger("BayonetState", (int)bayonet_state.equiping);
                }
                else
                {
                    anim.SetInteger("BayonetState", (int)bayonet_state.unarming);
                }
            }
        }
    }

    //������ ���˿���
    void IsBayonet_Warrior()
    {
        if (Input.GetKeyDown(KeyCode.X) && anim.GetInteger("Skill") == 0 && bayonetCoolTime >= 3.0f)
        {
            bayonetCoolTime = 0;
            if (bayonet == true)
            {
                anim.SetLayerWeight(normalLayer, 0);
                anim.SetLayerWeight(battleLayer, 1);
                anim.SetInteger("BayonetState", (int)bayonet_state.equiping);
                
            }
            else
            {
                anim.SetLayerWeight(normalLayer, 1);
                anim.SetLayerWeight(battleLayer, 0);
                anim.SetInteger("BayonetState", (int)bayonet_state.unarming);
            }
                
        }
    }

    //���� ����
    public bool Bayonet()
    {
        return bayonet;
    }

    //���� (Animation Event)
    public void WeaponBayonet()
    {
        if (isWarrior)
        {
            if (bayonet)
            {
                SwordOriginPos.position = SwordEquipPos.position;
                SwordOriginPos.rotation = SwordEquipPos.rotation;
            }
        }

        bayonet = !bayonet;
        BackSocket.SetActive(bayonet);
        HandSocket.SetActive(!bayonet);
    }

    public void BayonetState()
    {
        if(!bayonet)
            anim.SetInteger("BayonetState", (int)bayonet_state.equiped);
        else
            anim.SetInteger("BayonetState", (int)bayonet_state.unarmed);
    }

    public void Weapon_Col_Attack() //���� ���۽� �ݶ��̴� ��
    {
        HandSocket.GetComponent<BoxCollider>().enabled = true;
    }

    public void Weapon_Col_AtkEnd() // ���� ����� �ݶ��̴� ����
    {
        HandSocket.GetComponent<BoxCollider>().enabled = false;
    }

}

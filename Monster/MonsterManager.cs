using System.Collections.Generic;
using UnityEngine;
// ���� Ÿ��
public enum MonsterType 
{
    Goblin,
    Wolf,
    Troll
}

public class MonsterManager : MonoBehaviour
{
    //�ν��Ͻ�ȭ
    private static MonsterManager Instance { get; set; }
    //������ ������ ��ųʸ��� ����
    private Dictionary<MonsterType, MonsterData> monsterDic = new Dictionary<MonsterType, MonsterData>();
    
    //���� ���� ����
    public MonsterData goblinData = new MonsterData();
    private MonsterData wolfData = new MonsterData();
    private MonsterData trollData = new MonsterData();

    private void Awake()
    {
        #region �̱���
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        #endregion
        //��ųʸ� �ʱ�ȭ
        monsterDic.Clear();
        //���� �Ӽ� 
        InitMonsterData();
        
    }

    private void InitMonsterData()
    {
        // Goblin data
        goblinData.SetHP(100);
        goblinData.SetMaxHP(100);
        goblinData.SetDEF(20);
        goblinData.SetATK(20);
        goblinData.SetSPEED(4);
        goblinData.SetSTUN(false);
        goblinData.SetBURN(false);
        goblinData.SetPROVOKE(false);
        goblinData.SetEXP(100);

        monsterDic.Add(MonsterType.Goblin, goblinData);

        // Wolf data
        wolfData.SetHP(80);
        wolfData.SetMaxHP(80);
        wolfData.SetDEF(10);
        wolfData.SetATK(10);
        wolfData.SetSPEED(5);
        wolfData.SetSTUN(false);
        wolfData.SetBURN(false);
        wolfData.SetPROVOKE(false);
        wolfData.SetEXP(200);

        monsterDic.Add(MonsterType.Wolf, wolfData);

        // Troll data
        trollData.SetHP(150);
        trollData.SetMaxHP(150);
        trollData.SetDEF(50);
        trollData.SetATK(30);
        trollData.SetSPEED(2);
        trollData.SetSTUN(false);
        trollData.SetBURN(false);
        trollData.SetPROVOKE(false);
        trollData.SetEXP(500);

        monsterDic.Add(MonsterType.Troll, trollData);
    }
    //��ųʸ� Ű �� Ȯ��
    public MonsterData GetMonsterData(MonsterType type)
    {
        if (monsterDic.ContainsKey(type))
        {
            return monsterDic[type];
        }
        else
        {
            Debug.LogError("MonsterData�� Monster: " + type + "�� ã�� ����.");
            return null;
        }
    }
}
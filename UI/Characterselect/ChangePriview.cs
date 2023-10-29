using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangePriview : MonoBehaviour
{
    //ĳ���� RawImage
    public RawImage characterPreview;
    //ĳ���� ���� �� ���� Text
    public GameObject infoTxtPrefab;
    public Transform infoTxtParent;
    //�� ������ Texture
    public Texture[] warriorTextures;
    public Texture[] magicianTextures;
    public Texture[] priestTextures;
    public Texture[] archerTextures;

    //Class ��ư �ǳ�
    public GameObject classPanel;
    //ĳ���� ���� �� ȭ�� Fade
    public FadeInOut fadeInOut;
    //ĳ���� ���� ���� �ҷ��� ��ũ��Ʈ
    private PlayerStateManager playerStateManager;
    //���� ĳ���� �ؽ�Ʈ
    private GameObject currentInfoTxt;
    

    private void Start()
    {
        //ĳ���͸� �������� �ʾ��� �� �ʱ� �ؽ�ó�� ���� �ؽ�ó�� ����
        characterPreview.texture = warriorTextures[0];
        //Class��ư ��Ȱ��ȭ
        classPanel.gameObject.SetActive(false);
        //PlayerStateManager���� ĳ���� ������ ���� �� �ε�
        playerStateManager = GameObject.FindObjectOfType<PlayerStateManager>();
    }

    public void ChangeCharacterTexture(int jobIndex)
    {
        Texture[] textures = null;

        switch (jobIndex)
        {
            case 0:
                textures = warriorTextures;
                CreateInfoTxt("����: ���ϰ� ������ �������Դϴ�.");
                playerStateManager.SetCLASS(0); // ���� ����
                break;
            case 1:
                textures = archerTextures;
                CreateInfoTxt("�ü�: ��Ȯ�ϰ� ��ø�� Ȱ�����Դϴ�.");
                playerStateManager.SetCLASS(1); // ���� ����
                break;
            case 2:
                textures = magicianTextures;
                CreateInfoTxt("������: ������ �ֹ��� �����Դϴ�.");
                playerStateManager.SetCLASS(2); // ���� ����
                break;
            case 3:
                textures = priestTextures;
                CreateInfoTxt("����: �ż��� ġ�������� ��ȣ���Դϴ�.");
                playerStateManager.SetCLASS(3); // ���� ����
                break;
            default:
                Debug.LogError("��ȿ���� ���� ���� �ε����Դϴ�!");
                return;
        }

        // RawImage �ؽ�ó ����
        characterPreview.texture = textures[0];

        // ���� �ε��� PlayerPrefs�� ����
        PlayerPrefs.SetInt("SelectedJobIndex", jobIndex);
        PlayerPrefs.Save();

    }

    private void CreateInfoTxt(string description)
    {
        // info_txt ��ü�� ��Ȱ��ȭ
        DeactivateInfoTxt();

        // ��ü�� ����
        GameObject infoTxtObject = Instantiate(infoTxtPrefab, infoTxtParent);
        infoTxtObject.GetComponent<Text>().text = description;

        // ���� info_txt ��ü�� �����մϴ�.
        currentInfoTxt = infoTxtObject;
    }

    private void DeactivateInfoTxt()
    {
        if (currentInfoTxt != null)
        {
            // ������ info_txt ��ü�� ��Ȱ��ȭ�ϰ� �����մϴ�.
            currentInfoTxt.SetActive(false);
            Destroy(currentInfoTxt);
        }
    }

    public void ChangeScene()
    {
        fadeInOut.StartCoroutine("FadeOutStart");
        //StartCoroutine(fadeInOut.FadeOutStart());
        Invoke("LoadScene", 3f);
    }
    public void LoadScene()
    {
        AudioManager.Instance.bgmSources[0].Stop();
        AudioManager.Instance.bgmSources[0].clip = AudioManager.Instance.bgmClips[Random.Range(1, 4)];
        AudioManager.Instance.bgmSources[0].Play();
        SceneManager.LoadScene("Map");
    }
    public void ClassButton()
    {
        classPanel.gameObject.SetActive(true);
    }

}
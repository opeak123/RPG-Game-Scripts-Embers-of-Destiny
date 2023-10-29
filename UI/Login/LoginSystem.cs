using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class LoginSystem : MonoBehaviour
{
    public InputField email;
    public InputField password;

    public Text outputText;
    public GameObject loginState;


    void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;
        FirebaseAuthManager.Instance.Init();

        UISoundPlay.Instance.LoginBGM();
        loginState.SetActive(false);
    }

    private void OnChangedState(bool sign)
    {
        outputText.text = sign ? "�α��� : " : "�α׾ƿ� : ";
        outputText.text += FirebaseAuthManager.Instance.UserId;

        if (sign)
        {
            // ���� �α��� Ȯ��â ����, ���������� �̵��ϴ� ��ư ����
            loginState.SetActive(true);
        }
    }

    public void Create()
    {
        string e = email.text;
        string p = password.text;

        FirebaseAuthManager.Instance.Create(e, p);
    }
    public void Login()
    {
        FirebaseAuthManager.Instance.Login(email.text, password.text);
    }
    public void LogOut()
    {
        FirebaseAuthManager.Instance.LogOut();
    }

    public void Acess()
    {
        StartCoroutine(SceneChange());
    }

    IEnumerator SceneChange()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("CharacterSelect");
    }
}

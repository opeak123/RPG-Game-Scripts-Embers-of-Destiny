using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System;

public class FirebaseAuthManager
{
    private static FirebaseAuthManager instance = null;

    public static FirebaseAuthManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FirebaseAuthManager();
            }

            return instance;
        }
    }

    private FirebaseAuth auth;
    private FirebaseUser user;

    public string UserId => user.UserId;

    public Action<bool> LoginState;
    public void Init()
    {
        auth = FirebaseAuth.DefaultInstance;
        if(auth.CurrentUser != null)
        {
            LogOut();
        }

        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != user && auth.CurrentUser != null)
        {
            bool signed = auth.CurrentUser != null;
            if (!signed && user != null)
            {
                Debug.Log("�α׾ƿ�");
                LoginState?.Invoke(false);
            }

            user = auth.CurrentUser;
            if (signed)
            {
                Debug.Log("�α���");
                LoginState?.Invoke(true);
                UISoundPlay.Instance.QuestDoneSound();
            }
        }
    }

    public void Create(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("ȸ������ ���");
                AudioManager.Instance.sfxSources[0].clip = AudioManager.Instance.sfxClips[0];
                AudioManager.Instance.sfxSources[0].Play();
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("ȸ������ ����");
                AudioManager.Instance.sfxSources[3].clip = AudioManager.Instance.sfxClips[23];
                AudioManager.Instance.sfxSources[3].Play();

                return;
            }

            AuthResult newUser = task.Result;
            Debug.LogError("ȸ������ �Ϸ�");
        });
    }

    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("�α��� ���");
                AudioManager.Instance.sfxSources[3].clip = AudioManager.Instance.sfxClips[23];
                AudioManager.Instance.sfxSources[3].Play();
                return;
            }
            if (task.IsFaulted)
            {

                Debug.LogError("�α��� ����");
                return;
            }

            AuthResult newUser = task.Result;
            Debug.LogError("�α��� �Ϸ�");
        });
    }

    public void LogOut()
    {
        auth.SignOut();
        Debug.Log("�α׾ƿ�");
    }
}
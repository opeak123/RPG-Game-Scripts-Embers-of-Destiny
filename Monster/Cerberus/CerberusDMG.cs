using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CerberusDMG : MonoBehaviour
{
    [SerializeField]
    GameObject particle;
    public CerberusData data;

    bool firsCheck = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !transform.CompareTag("Enemy"))
        {
            if (firsCheck == false)
            {
                // �浹 ������ ��ƼŬ �ý����� �����ϰ� ����մϴ�.
                SpawnParticleEffect(other.transform.position + other.transform.up);
                other.GetComponent<PlayerState>().HitPlayer(data.atk, true);
                GetComponent<CerberusAnimationEvent>().BiteEnd();
                GetComponent<BoxCollider>().enabled = false;
                firsCheck = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !transform.CompareTag("Enemy"))
        {
            firsCheck = false;
        }
    }
    void SpawnParticleEffect(Vector3 _position)
    {
        // ��ƼŬ �ý��� ���
        Instantiate(particle, _position, Quaternion.identity);
    }
}

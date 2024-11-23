using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Player>()?.Kill();
        other.GetComponent<Bullet>()?.Deactive();
    }
}

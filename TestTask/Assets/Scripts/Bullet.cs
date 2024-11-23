using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Action Active;
    public Action Deactive;

    [NonSerialized]
    public bool IsActive = false;

    public Tween Tween;

    [SerializeField]
    private float _damage = 2f;

    private void OnDestroy() => Tween.Kill();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Boss>() || collision.gameObject.GetComponent<Bullet>())
            return;
        var damageble = collision.gameObject.GetComponent<IDamageble>();
        if (damageble != null)
        {
            damageble.Damage(_damage);
            Deactive();
        }
    }
}

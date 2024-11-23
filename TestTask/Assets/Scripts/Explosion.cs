using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField]
    private float _damage = 30f;
    [SerializeField, Range(0f, 2f)]
    private float _lifetime = 1f;
    [SerializeField, Range(0.1f, 100f)]
    private float _scale = 1f;

    public Tween _tween;

    private void Start() =>
        _tween = transform.DOScale(_scale, _lifetime).OnComplete(() => Destroy(this.gameObject));

    private void OnDestroy() => _tween.Kill();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Bullet>())
            other.GetComponent<IDamageble>()?.Damage(_damage);
    }
}

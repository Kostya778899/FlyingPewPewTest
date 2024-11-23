using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField]
    private Transform _mesh;

    [SerializeField]
    private GameObject _hitEffectPrefab;

    private float _startMeshScale;

    private void Awake() =>
        _startMeshScale = _mesh.localScale.x;

    private void Start() =>
        _mesh.localScale = Vector3.zero;

    public void Activate()
    {
        gameObject.SetActive(true);
        _mesh.DOScale(_startMeshScale, 1.5f).SetEase(Ease.OutCubic);
    }

    public void Deactivate() =>
        _mesh.DOScale(0f, 1.5f).OnComplete(() => gameObject.SetActive(false));

    private void OnCollisionEnter(Collision collision) =>
        collision.gameObject.GetComponent<Player>()?.Kill();

    public void Hit(Vector3 position)
    {
        var effect = Instantiate(_hitEffectPrefab, position, Quaternion.LookRotation(-position));
        StartCoroutine(effect.InvokeCoroutine(1f, (effect) => Destroy(effect)));
    }
}

using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageble
{
    private const float DelayAfterKillToReload = 2f;
    private const float DamageEffectAlpha = 0.5f;
    private const float DamageEffectDuration = 0.4f;

    public float Health { get; private set; }

    [SerializeField]
    private DIContainer _dIContainer;

    [SerializeField]
    private float _maxHealth = 100f;

    [SerializeField]
    private AnimationCurve _speedByVelocityY;
    [SerializeField, Range(0f, 100f)]
    private float _rotateSpeed = 12f, _zRotateSpeed = 5f;
    [SerializeField, Range(0f, 90f)]
    private float _maxZRotation = 30f;

    [SerializeField, Range(0.01f, 1f)]
    private float _shootCooldown = 0.2f;

    [SerializeField]
    private LineRenderer _shootLinePrefab;

    [SerializeField]
    private Image _healthBar;
    [SerializeField]
    private Image _damageEffect;
    [SerializeField]
    private GameObject _mesh;
    [SerializeField]
    private Transform _blower;
    [SerializeField]
    private Transform[] _shootPoints;
    [SerializeField]
    private float _shootDistance = 1000f;
    [SerializeField]
    private float _shootDamage = 20f;
    [SerializeField]
    private float _shootLinesLifetime = 5f;

    [SerializeField]
    private UnityEvent<float> _onDamage;
    [SerializeField]
    private UnityEvent _onKill;

    private Vector2 _velocity = Vector2.zero;
    private bool _alive = true;

    private Rigidbody _rigidbody;
    private Tween _tween;
    private Tween _damageEffectTween;

    private bool _immortality = false;

    private void Awake() =>
        _dIContainer.Player = this;

    private IEnumerator Start()
    {
        _dIContainer.Boss.OnKill.AddListener(() => _immortality = true);

        _rigidbody = GetComponent<Rigidbody>();

        _tween = _blower.DOLocalRotate(new Vector3(0, 360, 90), 1500f, RotateMode.FastBeyond360)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        Health = _maxHealth;
        UpdateHealthBar();

        while (true)
        {
            yield return null;
            if (Input.GetButton("Fire1"))
            {
                Shoot();
                yield return new WaitForSeconds(_shootCooldown);
            }
        }
    }

    private void OnDestroy()
    {
        _tween.Kill();
        _damageEffectTween?.Kill();
    }

    private void Update()
    {
        _velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        if (!_alive)
            return;

        transform.position += transform.forward *
            _speedByVelocityY.Evaluate(Mathf.InverseLerp(-1, 1, _velocity.y)) * Time.fixedDeltaTime;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + _velocity.x * _rotateSpeed * Time.fixedDeltaTime,
            Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.z,
                -_velocity.x * _maxZRotation, _zRotateSpeed * Time.fixedDeltaTime));
    }

    private void Shoot()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = _shootDistance;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 direction = mouseWorldPosition;

        foreach (var shootPoint in _shootPoints)
        {
            RaycastHit hit;
            Vector3 endPoint;
            direction.y = shootPoint.position.y;
            if (Physics.Raycast(shootPoint.position, direction, out hit))
            {
                hit.collider.GetComponent<IDamageble>()?.Damage(_shootDamage);
                hit.collider.GetComponent<Shield>()?.Hit(hit.point);

                endPoint = hit.point;
            }
            else
                endPoint = direction;
            var line = Instantiate(_shootLinePrefab);
            line.positionCount = 2;
            line.SetPositions(new[] { shootPoint.position, endPoint });
            line.DOColor(new Color2(Color.red, Color.red),
                new Color2(new Color(1,0,0,0), new Color(1, 0, 0, 0)), _shootLinesLifetime)
                .OnComplete(() => Destroy(line));
            /*DOVirtual.Float(1f, 0f, _shootLinesLifetime, (x) => line.colorGradient.alphaKeys[0].alpha = x)
                .OnComplete(() => Destroy(line));*/
        }
    }

    private void UpdateHealthBar() =>
        _healthBar.fillAmount = Mathf.InverseLerp(0, _maxHealth, Health);

    public void Damage(float strength)
    {
        if (_immortality)
            return;

        Health = Mathf.Max(0, Health - strength);
        _onDamage.Invoke(Health);
        UpdateHealthBar();
        var color = _damageEffect.color;
        color.a = 0f;
        _damageEffect.color = color;
        _damageEffectTween?.Kill();
        _damageEffectTween = _damageEffect.DOFade(DamageEffectAlpha,
            DamageEffectDuration / 2).SetLoops(2, LoopType.Yoyo);
        if (Health <= 0)
            Kill();
    }

    public void Kill()
    {
        if (_immortality)
            return;

        _alive = false;
        _mesh.SetActive(false);
        _onKill.Invoke();
        this.Invoke(DelayAfterKillToReload,
            (_) => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
    }
}

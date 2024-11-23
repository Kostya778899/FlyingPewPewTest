using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Boss : MonoBehaviour, IDamageble
{
    public UnityEvent OnKill;

    private const float BulletsKillResizeDuration = 0.2f;

    [SerializeField]
    private DIContainer _dIContainer;

    [SerializeField]
    private float _health = 10000f;

    [SerializeField, Range(0f, 200f)]
    private float _rotateSpeed = 25f;
    [SerializeField, Range(1, 32)]
    private int _gunsCount = 16;
    [SerializeField, Range(0.1f, 6f)]
    private float _shootCooldown = 0.5f;

    [SerializeField, Range(0f, 30f)]
    private float _bulletsLifetime = 10f, _kamikazesLifetime = 10f;
    [SerializeField, Range(0f, 100f)]
    private float _bulletsSpeed = 15f, _kamikazesSpeed = 15f, _kamikazesRotateSpeed = 5f;

    [SerializeField, Range(0f, 50f)]
    private float _kamikazesMinDistancePlayerToExplosion = 12f;
    [SerializeField, Range(0f, 1f)]
    private float _kamikazeSpawnProbability = 0.5f;

    [SerializeField]
    private Image _healthBar;
    [SerializeField]
    private Shield _shield;

    [SerializeField]
    private BossGun _gunPrefab;
    [SerializeField]
    private Bullet _bulletPrefab;
    [SerializeField]
    private KamikazeBullet _kamikazePrefab;
    [SerializeField]
    private Explosion _explosionPrefab, _bigExplosionPrefab;

    private BossGun[] _guns;
    private List<Bullet> _activeBullets = new(100);
    private List<Bullet> _deactiveBullets = new(10);
    private List<KamikazeBullet> _kamikazes = new(30);
    private float _startHealth;

    private Life _life;

    private class Life
    {
        private int _currentStepIndex = 0;
        private Boss _boss;
        private Step[] _steps;

        private struct Step
        {
            public float MinHealth;
            public Action Action;

            public Step(float minHealth, Action action)
            {
                MinHealth = minHealth;
                Action = action;
            }
        }

        public Life(Boss boss)
        {
            _boss = boss;

            _steps = new Step[] {
                new Step(9_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(8_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(7_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(6_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                }),
                new Step(5_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._kamikazeSpawnProbability = 0.1f;
                }),
                new Step(4_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(3_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                }),
                new Step(2_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(2_500, () => {
                    _boss._kamikazeSpawnProbability *= 2f;
                }),
                new Step(1_000, () => {
                    _boss._rotateSpeed *= 1.7f;
                    _boss._shootCooldown -= 0.05f;
                }),
                new Step(0_500, () => {
                    _boss._shield.Activate();
                    _boss._shield.Invoke(30f, (shield) => shield.Deactivate());
                }),
            };
        }

        public void Update()
        {
            if (_currentStepIndex < _steps.Length &&
                _boss._health <= _steps[_currentStepIndex].MinHealth)
                _steps[_currentStepIndex++].Action();
        }
    }

    private void Awake() =>
        _dIContainer.Boss = this;

    private IEnumerator Start()
    {
        _startHealth = _health;

        _life = new Life(this);

        _guns = new BossGun[_gunsCount];
        for (int i = 0; i < _gunsCount; i++)
        {
            _guns[i] = Instantiate(_gunPrefab, transform);
            _guns[i].transform.Rotate(0f, 360f / _gunsCount * i, 0f);
        }

        while (true)
        {
            yield return new WaitForSeconds(_shootCooldown);
            yield return new WaitForFixedUpdate();
            foreach (var gun in _guns)
            {
                if (UnityEngine.Random.value > _kamikazeSpawnProbability)
                    Shoot(gun);
                else
                    ShootKamikaze(gun);
            }
        }
    }

    private void FixedUpdate()
    {
        transform.Rotate(0f, _rotateSpeed * Time.fixedDeltaTime, 0f);
        foreach (var bullet in _activeBullets)
            bullet.transform.position += bullet.transform.forward *
                _bulletsSpeed * Time.fixedDeltaTime;
        for (int i = 0; i < _kamikazes.Count; i++)
        {
            var kamikaze = _kamikazes[i];

            kamikaze.transform.position += kamikaze.transform.forward *
                _kamikazesSpeed * Time.fixedDeltaTime;

            var targetRotation = Quaternion.LookRotation(
                _dIContainer.Player.transform.position - kamikaze.transform.position);
            kamikaze.transform.rotation = Quaternion.Slerp(
                kamikaze.transform.rotation, targetRotation, _kamikazesRotateSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(kamikaze.transform.position, _dIContainer.Player.transform.position) <=
                _kamikazesMinDistancePlayerToExplosion)
            {
                kamikaze.Explosion.Invoke();
                kamikaze.Deactive();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) =>
        collision.gameObject.GetComponent<Player>()?.Kill();

    private Bullet Shoot(BossGun gun)
    {
        Bullet bullet;
        if (_deactiveBullets.Count > 0)
            bullet = _deactiveBullets[0];
        else
        {
            bullet = Instantiate(_bulletPrefab);
            bullet.Active = () => {
                if (bullet.IsActive)
                    return;
                bullet.IsActive = true;
                bullet.transform.localScale = Vector3.one;
                bullet.gameObject.SetActive(true);
                _deactiveBullets.Remove(bullet);
                _activeBullets.Add(bullet);
            };
            bullet.Deactive = () =>
            {
                if (!bullet.IsActive)
                    return;
                bullet.IsActive = false;
                bullet.StopAllCoroutines();
                bullet.Tween = bullet.transform.DOScale(0f, BulletsKillResizeDuration).OnComplete(() => {
                    bullet.gameObject.SetActive(false);
                    _activeBullets.Remove(bullet);
                    _deactiveBullets.Add(bullet);
                });
            };
        }
        bullet.Active();
        bullet.transform.position = gun.ShootPoint.position;
        bullet.transform.rotation = gun.ShootPoint.rotation;
        bullet.Invoke(_bulletsLifetime, (bullet) => bullet.Deactive());
        return bullet;
    }

    private KamikazeBullet ShootKamikaze(BossGun gun)
    {
        var kamikaze = Instantiate(_kamikazePrefab, gun.ShootPoint.position, gun.ShootPoint.rotation);

        kamikaze.Active = () => {
            if (kamikaze.IsActive)
                return;
            kamikaze.IsActive = true;
            _kamikazes.Add(kamikaze);
        };
        kamikaze.Deactive = () =>
        {
            if (!kamikaze.IsActive)
                return;
            kamikaze.IsActive = false;
            _kamikazes.Remove(kamikaze);
            Destroy(kamikaze.gameObject);
        };
        kamikaze.Explosion.AddListener(() => Instantiate(_explosionPrefab,
            kamikaze.transform.position, kamikaze.transform.rotation));
        kamikaze.BigExplosion.AddListener(() => Instantiate(_bigExplosionPrefab,
            kamikaze.transform.position, kamikaze.transform.rotation));

        kamikaze.Active();

        kamikaze.Invoke(_kamikazesLifetime, (kamikaze) =>
        {
            kamikaze.Explosion.Invoke();
            kamikaze.Deactive();
        });
        return kamikaze;
    }

    public void Damage(float strength)
    {
        _health -= strength;
        _health = Mathf.Max(0, _health - strength);
        UpdateHealthBar();
        _life.Update();
        if (_health <= 0)
            Kill();
    }

    private void UpdateHealthBar() =>
        _healthBar.fillAmount = Mathf.InverseLerp(0, _startHealth, _health);

    private void Kill()
    {
        OnKill.Invoke();
        _dIContainer.WinScreen.Activate();
    }
}

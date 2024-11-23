using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KamikazeBullet : Bullet, IDamageble
{
    public UnityEvent Explosion;
    public UnityEvent BigExplosion;

    public void Damage(float strength)
    {
        BigExplosion.Invoke();
        Deactive();
    }
}

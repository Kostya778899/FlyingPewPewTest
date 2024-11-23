using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DIContainer", menuName = "DI/Container")]
public class DIContainer : ScriptableObject
{
    public Player Player;
    public Boss Boss;
    public WinScreen WinScreen;
    
}

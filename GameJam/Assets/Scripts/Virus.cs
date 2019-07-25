using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Virus
{
    void Move();

    void Infect();

    void Die();
    
    void Spawn();

    void Hit(int amount);

    float GetLife();
}

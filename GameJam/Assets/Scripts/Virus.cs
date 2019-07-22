using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Virus
{
    void Jump();

    void Move();

    void Infect();

    void Die();

    void Respawn();

    void Spawn();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TickableObject {
    public GameScript main;
    public abstract void Tick();
    public abstract void Disable();
}

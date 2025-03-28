using System;
using UnityEngine;
public class CharacterState : MonoBehaviour {
    CharacterStateType type;
    CharacterStateDirection direction;
    
    public event Action<CharacterStateType, CharacterStateDirection> OnStateChanged = delegate { };
    
    public CharacterStateType Type {
        get => type;
        set {
            if (type == value) return;
            type = value;
            OnStateChanged(type, direction);
        }
    }
    
    public CharacterStateDirection Direction {
        get => direction;
        set {
            if (direction == value) return;
            direction = value;
            OnStateChanged(type, direction);
        }
    }

    /// <summary>
    /// If you're planning to set both the type and direction at the same time, use this method. so that the event will only be called once.
    /// </summary>
    public void Set(CharacterStateType type, CharacterStateDirection direction) {
        this.type = type;
        this.direction = direction;
        OnStateChanged(type, direction);
    }
}
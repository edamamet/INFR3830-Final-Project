using System.Text;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] CharacterState state;
    StringBuilder sb;

    void Awake() {
        sb = new();
    }
    void OnValidate() {
        animator ??= GetComponent<Animator>();
        state ??= GetComponent<CharacterState>();
    }

    void OnEnable() {
        state.OnStateChanged += PlayAnimation;
    }
    
    void OnDisable() {
        state.OnStateChanged -= PlayAnimation;
    }

    void PlayAnimation(CharacterStateType type, CharacterStateDirection direction) {
        sb.Clear();
        sb.Append("Player_");
        sb.Append(type switch {
            CharacterStateType.Idle => "Idle_",
            CharacterStateType.Run => "Run_",
            CharacterStateType.Interact => "Interact_",
            _ => "Idle_",
        });
        sb.Append(direction switch {
            CharacterStateDirection.Front => "Front",
            CharacterStateDirection.Back => "Back",
            CharacterStateDirection.Left => "Left",
            CharacterStateDirection.Right => "Right",
            _ => "Front",
        });
        animator.CrossFade(sb.ToString(), 0);
    }
}

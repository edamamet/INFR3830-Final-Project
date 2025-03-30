using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CharacterAnimator animator;
    [SerializeField] float moveSpeed = 5, disableTime;
    [SerializeField] CharacterState state;
    [SerializeField] CharacterInteract interact;
    [SerializeField] PlayerInteract playerInteract;
    bool canMove = true;
    Vector2 forceDirection = Vector2.zero;

    void OnValidate() {
        rb ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<CharacterAnimator>();
        state ??= GetComponent<CharacterState>();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        if (!canMove) return;
        var hasInputted = false;
        CharacterStateType stateType = state.Type;
        CharacterStateDirection stateDirection = state.Direction;
        forceDirection = Vector2.zero;
        if (Keyboard.current.aKey.isPressed) {
            forceDirection.x -= 1;
            stateType = CharacterStateType.Run;
            stateDirection = CharacterStateDirection.Left;
            hasInputted = true;
        }
        if (Keyboard.current.dKey.isPressed) {
            forceDirection.x += 1;
            stateType = CharacterStateType.Run;
            stateDirection = CharacterStateDirection.Right;
            hasInputted = true;
        }
        if (Keyboard.current.wKey.isPressed) {
            forceDirection.y += 1;
            stateType = CharacterStateType.Run;
            stateDirection = CharacterStateDirection.Back;
            hasInputted = true;
        }
        if (Keyboard.current.sKey.isPressed) {
            forceDirection.y -= 1;
            stateType = CharacterStateType.Run;
            stateDirection = CharacterStateDirection.Front;
            hasInputted = true;
        }
        if (Keyboard.current.eKey.wasPressedThisFrame
            || Keyboard.current.spaceKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame) {
            stateType = CharacterStateType.Interact;
            hasInputted = true;
            interact.Interact();
            _ = TemporarilyDisableMovement();
        }
        if (!hasInputted) stateType = CharacterStateType.Idle;
        state.Set(stateType, stateDirection);
        forceDirection = forceDirection.normalized;
    }

    void FixedUpdate() {
        if (!canMove) return;
        rb.AddForce(forceDirection * moveSpeed);
    }

    async Task TemporarilyDisableMovement() {
        canMove = false;
        await Awaitable.WaitForSecondsAsync(disableTime);
        canMove = true;
    }
}

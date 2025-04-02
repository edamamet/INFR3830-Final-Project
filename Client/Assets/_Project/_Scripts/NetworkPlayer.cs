using System;
using UnityEngine;
class NetworkPlayer : MonoBehaviour {
    [SerializeField] CharacterState state;
    public Vector2 Position => transform.position;
    public int ID;

    Vector2 lastPosition;

    void Update() {
        if (!NetworkConfiguration.Instance) return;
        Vector2 position = GameManager.GameInfo.Positions[ID];
        transform.position = position;

        Vector2 direction = position - lastPosition;

        lastPosition = position;

        if (ID == NetworkConfiguration.Instance.ID) return;
        CharacterStateDirection stateDirection = 0;
        if (direction.x != 0) {
            stateDirection = direction.x > 0 ? CharacterStateDirection.Right : CharacterStateDirection.Left;
        } else if (direction.y != 0) {
            stateDirection = direction.y > 0 ? CharacterStateDirection.Back : CharacterStateDirection.Front;
        }
        state.Set(
            direction.magnitude < 0.1f ? CharacterStateType.Idle : CharacterStateType.Run,
            stateDirection
        );
    }
}

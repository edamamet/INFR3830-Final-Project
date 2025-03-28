using System;
using FMODUnity;
using UnityEngine;
public class CharacterFootsteps : MonoBehaviour {
    [SerializeField] ParticleSystem footsteps;
    [SerializeField] EventReference footstepsSound;
    [SerializeField] float distanceBetweenFootsteps = 1;
    [SerializeField] CharacterState state;
    
    float distanceCovered = 0;
    Vector3 lastPosition = Vector3.zero;

    void OnEnable() {
        state.OnStateChanged += HandleStateChange;
    }
    
    void OnDisable() {
        state.OnStateChanged -= HandleStateChange;
    }
    void HandleStateChange(CharacterStateType type, CharacterStateDirection _) {
        if (type == CharacterStateType.Idle) distanceCovered = 0;
    }

    void Update() {
        if (state.Type != CharacterStateType.Run) return;
        distanceCovered += (lastPosition - transform.position).magnitude;
        lastPosition = transform.position;
        if (!(distanceCovered >= distanceBetweenFootsteps)) return;
        PlayFootsteps();
        distanceCovered = 0;
    }

    public void PlayFootsteps() {
        footsteps.Play();
        RuntimeManager.PlayOneShotAttached(footstepsSound, gameObject);
    }
}
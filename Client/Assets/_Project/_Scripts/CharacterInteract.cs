using System;
using System.Threading.Tasks;
using FMODUnity;
using UnityEngine;
public class CharacterInteract : MonoBehaviour {
    [SerializeField] CharacterState state;
    [SerializeField] Vector2 offset;
    [SerializeField] float positionalOffset;
    [SerializeField] float hitDelay, soundDelay;
    [SerializeField] EventReference interactSound;
    [SerializeField] ParticleSystem interactEffect;
    
    public event Action<Vector2> OnHit = delegate { };

    public void Interact() {
        float x = offset.x, y = offset.y;
        Vector2 offsetVector = state.Direction switch {
            CharacterStateDirection.Front => Vector2.down * y,
            CharacterStateDirection.Back => Vector2.up * y,
            CharacterStateDirection.Left => Vector2.left * x,
            CharacterStateDirection.Right => Vector2.right * x,
            _ => Vector2.zero,
        };
        offsetVector += Vector2.up * positionalOffset;
        transform.localPosition = offsetVector;
        _ = PlayHit();
        _ = PlaySound();
    }

    async Task PlayHit() {
        await Task.Delay(TimeSpan.FromSeconds(hitDelay));
        interactEffect.Play();
        OnHit(transform.position);
    }
    async Task PlaySound() {
        await Task.Delay(TimeSpan.FromSeconds(soundDelay));
        RuntimeManager.PlayOneShotAttached(interactSound, gameObject);
    }
}

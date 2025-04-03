using UnityEngine;
public class PlayerInteract : MonoBehaviour {
    [SerializeField] CharacterInteract interact;
    [SerializeField] ContactFilter2D contactFilter;
    [SerializeField] float interactRadius = 1;

    Collider2D[] results = new Collider2D[10];

    Vector2 lastPosition;
    void OnValidate() {
        interact ??= GetComponent<CharacterInteract>();
    }

    void OnEnable() {
        interact.OnHit += OnHit;
    }

    void OnDisable() {
        interact.OnHit -= OnHit;
    }
    void OnHit(Vector2 position) {
        int hits = Physics2D.OverlapCircle(position, interactRadius, contactFilter, results);
        if (hits == 0) return;
        lastPosition = position;
        for (var i = 0; i < hits; i++) {
            Collider2D hit = results[i];
            hit.GetComponent<Chest>().Query();
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastPosition, interactRadius);
    }
}
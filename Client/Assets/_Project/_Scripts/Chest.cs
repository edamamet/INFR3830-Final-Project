using FMODUnity;
using UnityEngine;
public class Chest : MonoBehaviour {
    static int OPEN = Animator.StringToHash("Open");
    [SerializeField] Animator animator;
    [SerializeField] float deathTime;
    [SerializeField] EventReference openSound;
    public void Open() {
        Debug.Log("Chest opened!");
        animator.CrossFade(OPEN, 0);
        RuntimeManager.PlayOneShotAttached(openSound, gameObject);
        Destroy(gameObject, deathTime);
    }
}

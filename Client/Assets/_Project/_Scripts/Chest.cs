using System;
using FMODUnity;
using UnityEngine;
public class Chest : MonoBehaviour {
    static int OPEN = Animator.StringToHash("Open");
    bool opened = false;
    [SerializeField] Animator animator;
    [SerializeField] float deathTime;
    [SerializeField] EventReference openSound;
    [SerializeField] Milk milkPrefab;
    public Guid ID { get; private set; }

    public void Initialize(ChestManager chestManager) {
        ID = Guid.NewGuid();
        chestManager.RegisterChest(this);
        opened = false;
    }

    public async void Open() {
        opened = true;
        Debug.Log("Chest opened!");
        animator.CrossFade(OPEN, 0);
        RuntimeManager.PlayOneShotAttached(openSound, gameObject);
        await Awaitable.WaitForSecondsAsync(0.2f);
        Instantiate(milkPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);
        Destroy(gameObject, deathTime);
    }

    public void Query() {
        Debug.Log($"Querying chest {ID}");
        if (opened) return;
        Open();
    }
}

using System;
using DG.Tweening;
using UnityEngine;
public class Milk : MonoBehaviour {
    [SerializeField] SpriteRenderer spriteRenderer;
    void Start() {
        StartTween();
    }

    async void StartTween() {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(transform.DOLocalMoveY(1, 1).SetEase(Ease.OutBack));
        sequence.Join(spriteRenderer.DOFade(0, 1));
        await sequence.Play().AsyncWaitForCompletion();
    }
}
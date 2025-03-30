using System;
using UnityEngine;
using DG.Tweening;
using FMODUnity;
namespace _Project._Scripts {
    public class UIButton : MonoBehaviour {
        RectTransform rect;
        int id;
        [SerializeField] EventReference hoverSound, clickSound;
        void Awake() {
            rect = GetComponent<RectTransform>();
            id = GetInstanceID();
        }
        public void OnClick() {
            DOTween.Kill(id);
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * 0.1f, 0.3f).SetId(id);
            RuntimeManager.PlayOneShot(clickSound);
        }
        public void OnHover() {
            DOTween.Kill(id);
            rect.DOScale(1.05f, 0.7f).SetId(id).SetEase(Ease.OutElastic);
            RuntimeManager.PlayOneShot(hoverSound);
        }
        public void OnUnhover() {
            DOTween.Kill(id);
            rect.DOScale(1f, 0.5f).SetId(id).SetEase(Ease.OutElastic);
        }
    }
}

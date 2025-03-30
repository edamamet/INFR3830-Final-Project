using DG.Tweening;
using UnityEngine;
namespace _Project._Scripts {
    public class HomeMenu : MonoBehaviour, IMenu {
        [SerializeField] CanvasGroup canvas;
        RectTransform rect;
        object id;

        void Awake() {
            rect = GetComponent<RectTransform>();
            id = GetInstanceID();
        }

        public void Show() {
            canvas.Enable();
            rect.anchoredPosition = Vector2.left * 500;
            canvas.alpha = 0;
            DOTween.Kill(id);
            rect.DOAnchorPosX(0, 1f).SetEase(Ease.OutCirc).SetId(id);
            canvas.DOFade(1, 1f).SetId(id);
        }
        public void Hide() {
            DOTween.Kill(id);
            canvas.Disable();
            rect.DOAnchorPosX(-500, 1f).SetEase(Ease.OutCirc).SetId(id);
            canvas.DOFade(0, 1f).SetId(id);
        }
        public void ForceShow() => canvas.Show();
        public void ForceHide() => canvas.Hide();
    }
}

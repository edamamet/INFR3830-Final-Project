using UnityEngine;
public static class CanvasGroupExtensions {
    public static void Show(this CanvasGroup canvasGroup) {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    
    public static void Hide(this CanvasGroup canvasGroup) {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    
    public static void Enable(this CanvasGroup canvasGroup) {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    
    public static void Disable(this CanvasGroup canvasGroup) {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class BGScroll : MonoBehaviour {
    [SerializeField] RawImage bg;
    [SerializeField] Vector2 scroll;
    
    void Update() {
        bg.uvRect = new(bg.uvRect.position + scroll * Time.deltaTime, bg.uvRect.size);
    }
}
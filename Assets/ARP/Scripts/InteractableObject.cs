// InteractableObject.cs

using UnityEngine;

public class InteractableObject : MonoBehaviour {
    public void Interact() {
        Debug.Log("👉 설치된 프리팹과 상호작용");
        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Play");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScroller : MonoBehaviour {

    public float scrollSpeed;

    void Update () {
        float y = Mathf.Repeat(Time.time * scrollSpeed, 1);
        Vector2 offset = new Vector2(y, y);
        GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
    }
}

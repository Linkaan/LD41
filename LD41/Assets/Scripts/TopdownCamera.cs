using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopdownCamera : MonoBehaviour {

    public float scrollSpeed;
    public float scrollEdge;

    public float panSpeed;
    public float zoomSpeed;
    public Vector2 zoomRange;

    private float curZoom = 0;
    private float zoomRotation = 1;

    private Vector3 initPos;
    private Vector3 initRotation;

    void Start () {
        initPos = transform.position;
        initRotation = transform.eulerAngles;
    }

    void Update () {
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

        if (Input.GetKey("mouse 2")) {
            transform.Translate(Vector3.right * Time.deltaTime * panSpeed * (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f), Space.World);
            transform.Translate(Vector3.forward * Time.deltaTime * panSpeed * (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), Space.World);
        } else {
            bool scrollRight = Input.mousePosition.x >= Screen.width * (1.0f - scrollEdge);
            bool scrollLeft = Input.mousePosition.x <= Screen.width * scrollEdge;
            if (horInput > Mathf.Epsilon || scrollRight) {
                transform.Translate(Vector3.right * Mathf.Abs(scrollRight ? 1.0f : horInput) * Time.deltaTime * scrollSpeed, Space.World);
            } else if (horInput < -Mathf.Epsilon || scrollLeft) {
                transform.Translate(Vector3.right * Mathf.Abs(scrollLeft ? 1.0f : horInput) * Time.deltaTime * -scrollSpeed, Space.World);
            }

            bool scrollUp = Input.mousePosition.y >= Screen.height * (1.0f - scrollEdge);
            bool scrollDown = Input.mousePosition.y <= Screen.height * scrollEdge;
            if (verInput > Mathf.Epsilon || scrollUp) {
                transform.Translate(Vector3.forward * Mathf.Abs(scrollUp ? 1.0f : verInput) * Time.deltaTime * scrollSpeed, Space.World);
            } else if (verInput < -Mathf.Epsilon || scrollDown) {
                transform.Translate(Vector3.forward * Mathf.Abs(scrollDown ? 1.0f : verInput) * Time.deltaTime * -scrollSpeed, Space.World);
            }
        }

        curZoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSpeed;
        curZoom = Mathf.Clamp(curZoom, zoomRange.x, zoomRange.y);

        Vector3 pos = transform.position;
        pos.y -= (transform.position.y - (initPos.y + curZoom)) * 0.1f;
        transform.position = pos;

        Vector3 rot = transform.eulerAngles;
        rot.x -= (transform.eulerAngles.x - (initRotation.x + curZoom * zoomRotation)) * 0.1f;
        transform.eulerAngles = rot;
    }
}

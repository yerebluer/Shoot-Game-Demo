using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showPosition : MonoBehaviour
{
    public Vector3 position;
    public Vector3 localPosition;
    public Vector3 anchoredPosition3D;
    public Vector2 anchoredPosition;
    public Vector3 sub;
    RectTransform rect;
    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    private void Update()
    {
        position = transform.position;
        anchoredPosition = rect.anchoredPosition;
        localPosition = transform.localPosition;
        anchoredPosition3D = rect.anchoredPosition3D;
        sub = anchoredPosition3D - localPosition;
    }
}

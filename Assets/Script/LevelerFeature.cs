using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelerFeature : MonoBehaviour
{
    [Range(1.0f, 10.0f)]
    [SerializeField] private float levelerTolerance = 5.0f;

    [SerializeField] private TextMeshPro levelerReadingText;
    [SerializeField] private Renderer levelerOuterRenderer;

    private Color levelerDefaultColor;

    // Event function
    void Awake()
    {
        levelerDefaultColor = levelerOuterRenderer.material.color;
    }

    // Event function
    //void Update()
    //{
    //    Vector3 objectUp = transform.up;
    //    Vector3 worldUp = Vector3.up;

    //    int angle = Mathf.RoundToInt(Vector3.Angle(objectUp, worldUp));

    //    Vector3 crossProduct = Vector3.Cross(worldUp, objectUp);
    //    if (crossProduct.z > 0)
    //        angle = -angle;

    //    levelerReadingText.text = $"{angle:F0}°";

    //    levelerOuterRenderer.material.color =
    //        Mathf.Abs(angle) <= levelerTolerance ? Color.green : levelerDefaultColor;
    //}


    void Update()
    {
        // Get current rotation
        Vector3 rotation = transform.eulerAngles;

        // Lock X axis rotation
        transform.eulerAngles = new Vector3(0f, rotation.y, rotation.z);

        // Get Z rotation
        float zAngle = transform.eulerAngles.z;

        // Convert from 0–360 to -180–180
        if (zAngle > 180f)
            zAngle -= 360f;

        int roundedAngle = Mathf.RoundToInt(zAngle);

        // Display value
        levelerReadingText.text = $"{roundedAngle:F0}°";

        // Color feedback
        levelerOuterRenderer.material.color =
            Mathf.Abs(roundedAngle) <= levelerTolerance ? Color.green : levelerDefaultColor;
    }

}

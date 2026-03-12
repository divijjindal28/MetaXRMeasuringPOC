using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LearnXR.Core.Utilities;
using TMPro;
using UnityEngine;
using System.Text;
using System.IO;

public class MeasuringTapeFeature : MonoBehaviour
{
    [Range(.005f, .05f)]
    [SerializeField]
    private float tapeWidth = .01f;

    [SerializeField] private OVRInput.Button tapeActionButton;

    [SerializeField] private Material tapeMaterial;

    [SerializeField] private GameObject measurementInfoPrefab;
    [SerializeField] private Vector3 measurementInfoControllerOffset = new Vector3(0, 0.045f, 0);
    [SerializeField] private GameObject leftControllerTapeArea;
    [SerializeField] private GameObject rightControllerTapeArea;
    [SerializeField] private string measurementFormat = "<mark=#0000005A padding=\"20,20,10,10\"><color=white>{0}</color></mark>";
    [SerializeField] private float measurementInfoLength = 0.01f;
    [SerializeField] private TextMeshPro statusText;

    private List<MeasuringTape> savedTapelines = new();
    private TextMeshPro lastMeasurementInfo;
    private LineRenderer lastTapeLineRenderer;
    private OVRInput.Controller? currentContoller;

    private OVRCameraRig cameraRig;


    private void Awake()
    {
        cameraRig = FindObjectOfType<OVRCameraRig>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleControllerAction(OVRInput.Controller.LTouch, leftControllerTapeArea.transform);
        HandleControllerAction(OVRInput.Controller.RTouch, rightControllerTapeArea.transform);
    }

    

    private void HandleControllerAction(OVRInput.Controller controller, Transform TapeArea) {
        if(currentContoller != controller && currentContoller != null) return;

        if (OVRInput.GetDown(tapeActionButton, controller)) {
            currentContoller = controller;
            HandleDownAction(TapeArea);
        }
        if (OVRInput.Get(tapeActionButton, controller))
        {
            HandleHoldAction(TapeArea);
        }
        if (OVRInput.GetUp(tapeActionButton, controller))
        {
            currentContoller = controller;
            HandleUpAction(TapeArea);
        }
    }

    private void HandleDownAction(Transform TapeArea) {
        CreateNewTapeLine(TapeArea.position);
        AttachAndDetachMeasurementInfo(TapeArea, true);
    }

    private void HandleHoldAction(Transform TapeArea)
    {
        lastTapeLineRenderer.SetPosition(1, TapeArea.position);
        CalculateMeasurements();
        AttachAndDetachMeasurementInfo(TapeArea,true);
    }

    private void HandleUpAction(Transform TapeArea)
    {
        AttachAndDetachMeasurementInfo(TapeArea, false);
        var tape = savedTapelines[savedTapelines.Count - 1];

        LabelSelectionManager.Instance.ShowPanel(
            (label) => AssignLabel(tape, label)
        );
    }

    private void AssignLabel(MeasuringTape tape, string label)
    {
        tape.Label = label;

        var lineRenderer = tape.TapeLine.GetComponent<LineRenderer>();

        var distance = Vector3.Distance(
            lineRenderer.GetPosition(0),
            lineRenderer.GetPosition(1));

        var inches = MeasuringTape.MetersToInches(distance);
        var centimeters = MeasuringTape.MetersTocentemeters(distance);

        tape.TapeInfo.text =
            $"{label}\n{inches:F2}\" ({centimeters:F2}cm)";
    }

    private void CreateNewTapeLine(Vector3 initialPosition) {
        var newTapeLine = new GameObject($"TapeLine_{savedTapelines.Count}", typeof(LineRenderer));

        lastTapeLineRenderer = newTapeLine.GetComponent<LineRenderer>();
        lastTapeLineRenderer.positionCount = 2;
        lastTapeLineRenderer.startWidth = tapeWidth;
        lastTapeLineRenderer.endWidth = tapeWidth;

        lastTapeLineRenderer.material = tapeMaterial;
        lastTapeLineRenderer.SetPosition(0, initialPosition);
        lastMeasurementInfo = Instantiate(measurementInfoPrefab, Vector3.zero, Quaternion.identity).GetComponent<TextMeshPro>();
        lastMeasurementInfo.GetComponent<BillboardAlignment>().AttachTo(cameraRig.centerEyeAnchor);
        lastMeasurementInfo.gameObject.SetActive(false);
        savedTapelines.Add(new MeasuringTape {
            TapeLine = newTapeLine,
            TapeInfo = lastMeasurementInfo
        });
         
    }

    private void AttachAndDetachMeasurementInfo(Transform tapeArea, bool attachToController = true) {
        if (attachToController)
        {
            lastMeasurementInfo.gameObject.SetActive(true);
            lastMeasurementInfo.transform.SetParent(tapeArea.transform.parent);
            lastMeasurementInfo.transform.localPosition = measurementInfoControllerOffset;
        }
        else {
            lastMeasurementInfo.transform.SetParent(lastTapeLineRenderer.transform);
            
            var lineDirection = (lastTapeLineRenderer.GetPosition(0) - lastTapeLineRenderer.GetPosition(1));
            Vector3 lineCrossProduct  = Vector3.Cross(lineDirection, Vector3.up);
            Vector3 lineMidPoint = (lastTapeLineRenderer.GetPosition(0) + lastTapeLineRenderer.GetPosition(1)) / 2;
            lastMeasurementInfo.transform.position = lineMidPoint + (2 * lineCrossProduct.normalized * measurementInfoLength);

        }
    }

    private void CalculateMeasurements2() {
        var distance = Vector3.Distance(lastTapeLineRenderer.GetPosition(0), lastTapeLineRenderer.GetPosition(1));
        var inches = MeasuringTape.MetersToInches(distance);
        var centimeters = MeasuringTape.MetersTocentemeters(distance);
        var lastLineMeasuringTape = savedTapelines[savedTapelines.Count-1];
        lastLineMeasuringTape.TapeInfo.text = string.Format(measurementFormat, $"{inches:F2} <i>{centimeters:F2}cm</i>");
    }

    private void CalculateMeasurements()
    {
        Vector3 start = lastTapeLineRenderer.GetPosition(0);
        Vector3 end = lastTapeLineRenderer.GetPosition(1);

        float distance = Vector3.Distance(start, end);

        var inches = MeasuringTape.MetersToInches(distance);
        var centimeters = MeasuringTape.MetersTocentemeters(distance);

        // Direction of the line
        Vector3 direction = (end - start).normalized;

        // Angle with world axes
        float xAngle = Vector3.Angle(direction, Vector3.right);
        float yAngle = Vector3.Angle(direction, Vector3.up);
        float zAngle = Vector3.Angle(direction, Vector3.forward);

        var lastLineMeasuringTape = savedTapelines[savedTapelines.Count - 1];

        lastLineMeasuringTape.TapeInfo.text =
            $"{inches:F2}\" <i>{centimeters:F2}cm</i>\n" +
            $"X: {xAngle:F1}°  Y: {yAngle:F1}°  Z: {zAngle:F1}°";
    }

    private void OnDestroy()
    {
        foreach (var tapeLine in savedTapelines) {
            Destroy(tapeLine.TapeLine);
        }
        savedTapelines.Clear();
    }

    public void ExportMeasurementsToCSV()
    {
        if (savedTapelines.Count == 0)
        {
            Debug.Log("ExportMeasurementsToCSV : No measurements to export.");
            return;
        }

        StringBuilder csv = new StringBuilder();

        // Header
        csv.AppendLine("Label,DistanceMeters,DistanceInches,DistanceCentimeters");

        foreach (var tape in savedTapelines)
        {
            var lineRenderer = tape.TapeLine.GetComponent<LineRenderer>();

            float distanceMeters = Vector3.Distance(
                lineRenderer.GetPosition(0),
                lineRenderer.GetPosition(1)
            );

            double inches = MeasuringTape.MetersToInches(distanceMeters);
            double centimeters = MeasuringTape.MetersTocentemeters(distanceMeters);

            string label = string.IsNullOrEmpty(tape.Label) ? "Unnamed" : tape.Label;

            csv.AppendLine($"{label},{distanceMeters:F3},{inches:F2},{centimeters:F2}");
        }

        string fileName = "XR_Measurements_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, csv.ToString());

        Debug.Log($"ExportMeasurementsToCSV : CSV exported successfully to: {path}");
        StartCoroutine(ShowStatusMessage($"Measurements exported"));
    }

    IEnumerator ShowStatusMessage(string message, float duration = 2f)
    {
        if (statusText == null) yield break;
        statusText.text = message;
        statusText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        statusText.gameObject.SetActive(false);
    }
}

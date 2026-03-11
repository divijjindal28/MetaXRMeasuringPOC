using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LearnXR.Core.Utilities;
using TMPro;
using UnityEngine;

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
            lastMeasurementInfo.transform.position = lineMidPoint + (lineCrossProduct.normalized * measurementInfoLength);

        }
    }

    private void CalculateMeasurements() {
        var distance = Vector3.Distance(lastTapeLineRenderer.GetPosition(0), lastTapeLineRenderer.GetPosition(1));
        var inches = MeasuringTape.MetersToInches(distance);
        var centimeters = MeasuringTape.MetersTocentemeters(distance);
        var lastLineMeasuringTape = savedTapelines[savedTapelines.Count-1];
        lastLineMeasuringTape.TapeInfo.text = string.Format(measurementFormat, $"{inches:F2} <i>{centimeters:F2}cm</i>");
    }

    private void OnDestroy()
    {
        foreach (var tapeLine in savedTapelines) {
            Destroy(tapeLine.TapeLine);
        }
        savedTapelines.Clear();
    }
}

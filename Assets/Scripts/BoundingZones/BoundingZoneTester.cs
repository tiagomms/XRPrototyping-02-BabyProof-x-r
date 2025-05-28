using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tests the BoundingZoneChecker functionality by moving a cube to different positions
/// and logging the results.
/// </summary>
public class BoundingZoneTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float testInterval = 10f;
    [SerializeField] private Vector3 cubeSize = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private BoundingZoneManager boundingZoneManager;

    private GameObject testCube;
    private float nextTestTime;

    private bool isTesting = false;

    public void ToggleTest()
    {
        if (!isTesting)
        {
            StartTest();
        }
        else
        {
            StopTest();
        }
    }

    private void StartTest()
    {
        // Create the test cube
        testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.transform.localScale = cubeSize;
        testCube.name = "BoundingZoneTestCube";

        if (boundingZoneManager.AllZones.Count == 0)
        {
            XRDebugLogViewer.LogError("No BoundingZoneCheckers found in the scene!");
            enabled = false;
            return;
        }

        nextTestTime = Time.time + testInterval;
        isTesting = true;
    }

    private void StopTest()
    {
        DestroyTest();
        isTesting = false;
    }


    private void Update()
    {
        if (!isTesting) return;

        if (Time.time >= nextTestTime)
        {
            TestNextPosition();
            nextTestTime = Time.time + testInterval;
        }
    }

    private void TestNextPosition()
    {
        // Select a random zone checker
        BoundingZoneChecker selectedZone = boundingZoneManager.AllZones[Random.Range(0, boundingZoneManager.AllZones.Count)];

        // Randomly choose one of three positions
        int positionType = Random.Range(0, 3);
        Vector3 testPosition;

        switch (positionType)
        {
            case 0: // Inside internal area (anchor position)
                testPosition = selectedZone.transform.position;
                break;
            case 1: // In the zone (between internal and external bounds)
                // Get a random point between internal and external bounds
                Vector3 randomOffset = selectedZone.InternalBounds.extents + Vector3.one * 0.1f;
                randomOffset.y = 0f;
                testPosition = selectedZone.transform.position + randomOffset;
                break;
            case 2: // Outside external bounds
                // Get a point far from the zone
                Vector3 farOffset = selectedZone.ExternalBounds.extents + Vector3.one * 0.25f;
                farOffset.y = 0f;
                testPosition = selectedZone.transform.position + farOffset;
                break;
            default:
                testPosition = selectedZone.transform.position;
                break;
        }

        // Move the test cube
        testCube.transform.position = testPosition;

        // Check if the point is in the zone
        bool isInZone = selectedZone.IsPointInZone(testPosition);

        // Log the result
        string positionTypeStr = positionType switch
        {
            0 => "Inside internal area",
            1 => "In the zone",
            2 => "Outside external bounds",
            _ => "Unknown"
        };

        XRDebugLogViewer.Log($"Testing {selectedZone.gameObject.name}:\n" +
                           $"Position Type: {positionTypeStr}\n" +
                           $"Position: {testPosition}\n" +
                           $"Is in zone: {isInZone}\n" +
                           $"External Bounds: {selectedZone.ExternalBounds}\n" + 
                           $"Internal Bounds: {selectedZone.InternalBounds}\n"
                           );
    }

    private void OnDestroy()
    {
        DestroyTest();
    }

    private void DestroyTest()
    {
        if (testCube != null)
        {
            Destroy(testCube);
        }
    }
} 
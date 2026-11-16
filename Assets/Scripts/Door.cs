using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private int totalSwitches = 4;
    [SerializeField] private float openHeight = 6.5f;
    [SerializeField] private float openSpeed = 3f;

    private HashSet<int> activatedSwitches = new HashSet<int>();

    private Vector3 closedPosition;
    private bool opening = false;
    private int nextSwitchId = 1;

    private void Start()
    {
        closedPosition = transform.position;
    }

    public bool ActivateSwitch(int switchId)
    {
        if (opening)
            return false;

        if (switchId != nextSwitchId)
        {
            Debug.Log("Wrong order! Activate Switch " +
                      nextSwitchId + " next.");
            return false;
        }

        if (!activatedSwitches.Add(switchId))
            return false;

        Debug.Log("Switch " + switchId + " activated! (" +
                  activatedSwitches.Count + "/" +
                  totalSwitches + ")");

        nextSwitchId++;

        if (activatedSwitches.Count >= totalSwitches)
        {
            Debug.Log("All switches activated! Door opening!");
            opening = true;
        }

        return true;
    }

    private void Update()
    {
        if (!opening)
            return;

        Vector3 targetPosition =
            closedPosition + Vector3.up * openHeight;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            openSpeed * Time.deltaTime
        );

        if (transform.position == targetPosition)
            opening = false;
    }
}


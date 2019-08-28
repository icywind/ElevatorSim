using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.Tween;

public enum LiftState
{
    Up,
    Still,
    Down
}

public class Elevator : MonoBehaviour
{
    [SerializeField] Transform lowAnchor = null;
    [SerializeField] Transform hiAnchor = null;

    [SerializeField] float speed = 1.0f;

    [SerializeField]
    List<float> floorPositionY = new List<float>();
    [SerializeField]
    int testFloor = 4;

    HashSet<int> UpStops = new HashSet<int>();
    HashSet<int> DownStops = new HashSet<int>();

    int CurrentFloor = 0;
    int highest = 0;
    int lowest = 0;
    LiftState liftState = LiftState.Still;

    private void Awake()
    {
        SetFloors(lowAnchor.position.y, hiAnchor.position.y, 5);
    }

    public void AddRequest(int floor)
    {
        if (floor > CurrentFloor)
        {
            UpStops.Add(floor);
            if (floor > highest)
            {
                highest = floor;
            }
        }
        else
        {
            DownStops.Add(floor);
            if (floor < lowest)
            {
                lowest = floor;
            }
        }
    }

    public void SetFloors(float buildingBase, float stopheight, int numFloors)
    {
        float floorHeight = (stopheight - buildingBase)/ numFloors;
        for (int f = 0; f<numFloors; f++)
        {
            float y = buildingBase + floorHeight * f;
            floorPositionY.Add(y);
        }
    }

    void OnStop()
    {
        UpStops.Remove(CurrentFloor);
        DownStops.Remove(CurrentFloor);
        if (liftState == LiftState.Up && UpStops.Count > 0)
        {
            // move it up again
        }
        else if (liftState == LiftState.Down && DownStops.Count > 0)
        {
            // move it down again
        }
    }

    public void OnClick()
    {
        Debug.Log("OnClick of Elevator");
        Move(testFloor);
    }

    private void Update()
    {

    }

    float getDuration(Vector3 current, Vector3 target)
    {
        float dist = Vector3.Distance(current, target);
        return dist / speed;
    }

    void Move(int targetFloor)
    {
        if (targetFloor == CurrentFloor || targetFloor < 0 || targetFloor > floorPositionY.Count-1)
        {
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 targetFloorPos = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
        float dist = Vector3.Distance(currentPos, targetFloorPos);
        // intermidiate position for the elevator to slow down before stop
        Vector3 nextPos = currentPos + .7f * dist *  (targetFloor>CurrentFloor ? Vector3.up : Vector3.down);

        Debug.LogWarning(string.Format("CurrentPos = {0} next = {1}  target={2}", currentPos, nextPos, targetFloorPos));
        System.Action<ITween<Vector3>> startMovement = (t) =>
        {
            gameObject.transform.position = t.CurrentValue;
        };
        float duration1 = getDuration(currentPos, nextPos);
        float duration2 = getDuration(nextPos, targetFloorPos);
        gameObject.Tween("MoveElevator", currentPos, nextPos, duration1, TweenScaleFunctions.CubicEaseIn, startMovement)
            .ContinueWith( new Vector3Tween().Setup(nextPos, targetFloorPos, duration2, 
            TweenScaleFunctions.CubicEaseOut, startMovement, 
            (t)=> {
                CurrentFloor = targetFloor;
            }));
    }
}

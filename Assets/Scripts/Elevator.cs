using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

namespace SimElevator
{
    public enum LiftState
    {
        Still,
        Up,
        Down,
        PauseUp,
        PauseDown
    }

    public class Elevator : MonoBehaviour
    {
        [SerializeField] GameObject PanelPrefab = null;
        [SerializeField] float speed = 1.0f;
        [SerializeField] float liftPauseTime = 3.0f;
        [SerializeField] Button elvButton = null;
        [SerializeField]
        int testFloor = 4;

        const float DISTANCE_ERROR = 0.05f;

        List<float> floorPositionY = new List<float>();
        [SerializeField]
        List<int> UpStops = new List<int>();
        //HashSet<int> UpStops = new HashSet<int>();
        [SerializeField]
        List<int> DownStops = new List<int>();

        int TopFloor
        {
            get
            {
                return floorPositionY.Count - 1;
            }
        }

        int elvId = -1;
        //HashSet<int> DownStops = new HashSet<int>();

        // floor is requested inside the elevator
        [SerializeField]
        HashSet<int> InsideRequest = new HashSet<int>();

        int CurrentFloor = 0;

        int highest = 0;
        int lowest = 0;
        double pausingTime = 0;

        Action<int, LiftState> stopNotify = null;
        Vector3 TargetPosition { get; set; }

        public LiftState LiftState; //{ get; private set; }

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
            if (LiftState == LiftState.Still)
            {
                OnStart();
            }
        }

        /// <summary>
        /// Cancels the stop request.  Only request from the building button panel
        /// can be cancelled.
        /// </summary>
        /// <param name="floor">Floor.</param>
        public void CancelRequest(int floor)
        {
            if (!InsideRequest.Contains(floor))
            {
                UpStops.Remove(floor);
                DownStops.Remove(floor);
            }
        }

        /// <summary>
        /// Setup the Elevator with specified numOfFloors, floorHeight and stopCallback.
        /// The starting up of Elevator is position at floor 0 in the prefab.
        /// </summary>
        /// <param name="elvId">Elevator ID</param>
        /// <param name="numOfFloors">Number of floors.</param>
        /// <param name="floorHeight">Floor height.</param>
        /// <param name="stopCallback">Stop callback.</param>
        public void Setup(int elvId, int numOfFloors, float floorHeight, Action<int, LiftState> stopCallback)
        {
            this.elvId = elvId;
            Debug.LogWarning("Screen size h= " + Screen.height + " w:" + Screen.width);
            for (int i = 0; i < numOfFloors; i++)
            {
                float posY = transform.position.y + i * floorHeight/(5.5f*Screen.height/640);
                floorPositionY.Add(posY);
            }
            stopNotify = stopCallback;
            UpdateElevatorVisual(true);
        }


        private void Update()
        {
            switch  (LiftState) 
            {
                case LiftState.Up:
                case LiftState.Down:
                    {
                        float distance = Vector3.Distance(TargetPosition, transform.position);

                        // will only move while the distance is bigger than 1.0 units
                        if (distance > DISTANCE_ERROR)
                        {
                            Vector3 dir = TargetPosition - transform.position;
                            dir.Normalize();                                    // normalization is obligatory
                            transform.position += dir * speed * Time.deltaTime; // using deltaTime and speed is obligatory
                        }
                        int floor = CheckFloor();
                        if (-1 != floor)
                        {
                            CurrentFloor = floor;
                            if ((UpStops.Contains(floor) && LiftState == LiftState.Up) || 
                                (DownStops.Contains(floor) && LiftState == LiftState.Down) ||
                                distance <= DISTANCE_ERROR 
                                )
                            {
                                OnStop();
                            }
                        }
                    }
                    break;
                case LiftState.Still:
                    break;
                case LiftState.PauseUp:
                case LiftState.PauseDown:
                    if (pausingTime >= liftPauseTime)
                    {
                        // move back to other state
                        ResumeLifting();
                        pausingTime = 0;
                    } else
                    {
                        pausingTime += Time.deltaTime;
                    }
                    break;
            }
        }

        int CheckFloor()
        {
            float curY = transform.position.y;
            int floor = -1;
            if (curY < floorPositionY[0])
            {
                floor = 0;
            }
            else
            {
                for (int h = 1; h < floorPositionY.Count; h++)
                {
                    if ( Math.Abs (floorPositionY[h]-curY ) < DISTANCE_ERROR )
                    {
                        floor = h;
                        break;
                    }
                }
            }

            return floor;
        }

        void UpdateElevatorVisual(bool doorOpen)
        {
            if(doorOpen)
            {
                elvButton.image.color = Color.green;
            } else
            {
                elvButton.image.color = Color.white;
            }
        }

        /// <summary>
        /// Start moving the elevator, entry only on STILL state
        /// </summary>
        void OnStart()
        { 
            if (LiftState == LiftState.Still)
            {
                if (CurrentFloor == 0)
                {
                    PrepareMoveUp();
                } 
                else if (CurrentFloor == TopFloor)
                {
                    PrepareMoveDown();
                }  
                else
                {
                    if (UpStops.Count > 0 && DownStops.Count == 0) 
                    {
                        PrepareMoveUp();
                    } 
                    else if (DownStops.Count > 0 && UpStops.Count == 0)
                    {
                        PrepareMoveDown();
                    }
                    else if (DownStops.Count > 0 && UpStops.Count > 0)
                    {
                        //determine up or down based on the closest requested floor
                        int[] upDist = UpStops.Select(x => Math.Abs(x - CurrentFloor)).ToArray();
                        int[] downDist = DownStops.Select(x => Math.Abs(x - CurrentFloor)).ToArray();
                        if (upDist.Min() <= downDist.Min())
                        {
                            PrepareMoveUp();
                        } 
                        else
                        {
                            PrepareMoveDown();
                        }
                    }
                } 
            }
        }


        void OnStop()
        {
            Debug.LogWarning("OnStop, elv:" + elvId);
            UpStops.Remove(CurrentFloor);
            DownStops.Remove(CurrentFloor);
            InsideRequest.Remove(CurrentFloor);

            if (UpStops.Count == 0 && DownStops.Count == 0)
            {
                LiftState = LiftState.Still;
                UpdateElevatorVisual(true);
            }
            else
            {
                // change the state, Update will pause movement
                if (LiftState == LiftState.Up)
                {
                    LiftState = LiftState.PauseUp;
                    UpdateElevatorVisual(true);

                } 
                else if (LiftState == LiftState.Down)
                {
                    LiftState = LiftState.PauseDown;
                    UpdateElevatorVisual(true);
                }
            }

            stopNotify?.Invoke(CurrentFloor, LiftState);
        }

        void ResumeLifting()
        {
            if (LiftState==LiftState.PauseUp)
            {
                if (UpStops.Count > 0)
                {
                    // move it up again
                    // LiftState = LiftState.Up;
                    PrepareMoveUp();
                }
                else
                {
                    //LiftState = LiftState.Down;
                    PrepareMoveDown();
                    // move it down
                }
            } 
            else if (LiftState == LiftState.PauseDown)
            {
                if (DownStops.Count > 0)
                {
                    // move it down again
                    // LiftState = LiftState.Down;
                    PrepareMoveDown();
                }
                else
                {
                    PrepareMoveUp();
                    //LiftState = LiftState.Up;
                    // move it up
                }
            }
           
        }

        public void OnClick()
        {
            Debug.Log("OnClick of Elevator");
             // Move2(testFloor);
            if (LiftState == LiftState.PauseDown || 
                LiftState == LiftState.PauseUp ||
                LiftState == LiftState.Still)
            {
                OpenPanel();
            }
        }

        void OpenPanel()
        {
            GameObject go = GameObjectUtil.InstantiateAndAnchor(PanelPrefab,
                transform.parent.parent.parent,
                new Vector3(0, 0, 0)
                );
            ElevatorPanel panel = go.GetComponent<ElevatorPanel>();
            if (panel != null)
            {
                panel.Setup(elvId, TopFloor + 1, AddRequest, LiftState, null);
            }
        }

        float getDuration(Vector3 current, Vector3 target)
        {
            float dist = Vector3.Distance(current, target);
            return dist / speed;
        }

        void PrepareMoveUp()
        {
            if (UpStops.Count > 0)
            {
                Vector3 currentPos = transform.position;
                int targetFloor = UpStops[0];
                TargetPosition = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
                LiftState = LiftState.Up;
                UpdateElevatorVisual(false);
            }
        }

        void PrepareMoveDown()
        {
            if (DownStops.Count > 0)
            {
                Vector3 currentPos = transform.position;
                int targetFloor = DownStops[0];
                TargetPosition = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
                LiftState = LiftState.Down;
                UpdateElevatorVisual(false);
            }
        }

        void Move2(int targetFloor)
        {
            if (targetFloor == CurrentFloor || targetFloor < 0 || targetFloor > floorPositionY.Count - 1)
            {
                return;
            }

            if (targetFloor > CurrentFloor)
            {
                UpStops.Add(targetFloor);
            }
            else
            {
                DownStops.Add(targetFloor);
            }

            InsideRequest.Add(targetFloor);
            if (LiftState == LiftState.Still)
            {

                if (targetFloor > CurrentFloor)
                {
                    PrepareMoveUp();
                } else
                {
                    PrepareMoveDown();
                }
            } 

        }

        void Move(int targetFloor)
        {
            if (targetFloor == CurrentFloor || targetFloor < 0 || targetFloor > floorPositionY.Count - 1)
            {
                return;
            }

            Vector3 currentPos = transform.position;
            Vector3 targetFloorPos = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
            float dist = Vector3.Distance(currentPos, targetFloorPos);
            // intermidiate position for the elevator to slow down before stop
            Vector3 nextPos = currentPos + .7f * dist * (targetFloor > CurrentFloor ? Vector3.up : Vector3.down);

            Debug.LogWarning(string.Format("CurrentPos = {0} next = {1}  target={2}", currentPos, nextPos, targetFloorPos));
            System.Action<ITween<Vector3>> startMovement = (t) =>
            {
                gameObject.transform.position = t.CurrentValue;
            };
            float duration1 = getDuration(currentPos, nextPos);
            float duration2 = getDuration(nextPos, targetFloorPos);
            gameObject.Tween("MoveElevator", currentPos, nextPos, duration1, TweenScaleFunctions.CubicEaseIn, startMovement)
                .ContinueWith(new Vector3Tween().Setup(nextPos, targetFloorPos, duration2,
                TweenScaleFunctions.CubicEaseOut, startMovement,
                (t) =>
                {
                    CurrentFloor = targetFloor;
                }));
        }
    }

}
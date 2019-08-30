using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public delegate void StateUpdateDelegate(int floor, LiftState state);

    public class Elevator : MonoBehaviour
    {
        #region Declarations
        [SerializeField] GameObject PanelPrefab = null;

        [SerializeField] Button elvButton = null;
             
        const float DISTANCE_ERROR = 0.05f;
        readonly List<float> floorPositionY = new List<float>();

        [SerializeField]
        HashSet<int> UpStops = new HashSet<int>();

        [SerializeField]
        HashSet<int> DownStops = new HashSet<int>();

        int TopFloor
        {
            get
            {
                return floorPositionY.Count - 1;
            }
        }

        int elvId = -1;
        float speed = 1.0f;
        float liftPauseTime = 3.0f;
        // floor is requested inside the elevator
        [SerializeField]
        HashSet<int> InsideRequest = new HashSet<int>();

        int _currentFloor = 0;
        public int CurrentFloor
        {
            get { return _currentFloor;  }
            private set
            {
                _currentFloor = value;
                handleStateUpdate?.Invoke(_currentFloor, LiftState);
            }
        }

        double pausingTime = 0;

        Vector3 TargetPosition { get; set; }

        public LiftState LiftState; //{ get; private set; }

        event StateUpdateDelegate handleStateUpdate;
        event StateUpdateDelegate stopNotify;

        ElevatorPanel elevatorPanel;

        #endregion

        public void AddRequest(int floor)
        {
            if (floor > CurrentFloor)
            {
                UpStops.Add(floor);
            }
            else
            {
                DownStops.Add(floor);
            }
            if (LiftState == LiftState.Still)
            {
                OnStart();
            }
            else UpdateCourse(floor);
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
        /// <param name="speed">moving speed.</param>
        /// <param name="pausetime">wait time before moving again.</param>
        /// <param name="stopCallback">Stop callback.</param>
        public void Setup(int elvId, int numOfFloors, float floorHeight, float speed, float pausetime, StateUpdateDelegate stopCallback)
        {
            this.elvId = elvId;
            this.speed = speed;
            this.liftPauseTime = pausetime;

            for (int i = 0; i < numOfFloors; i++)
            {
                float posY = transform.position.y + i * floorHeight/(5.5f*Screen.height/640);
                floorPositionY.Add(posY);
            }
            stopNotify += stopCallback;
            UpdateElevatorVisual(true);
        }

        private void Update()
        {
            monitorMovement();
        }

        /// <summary>
        /// Monitors the movement.
        /// </summary>
        private void monitorMovement()
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
                            if (CurrentFloor != floor)
                            {
                                Debug.Log("Floor ------> " + floor);
                                CurrentFloor = floor;
                            }
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

        /// <summary>
        ///   Determine what floor the elevator is at.
        /// </summary>
        /// <returns>The floor.</returns>
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
                for (int h = 0; h < floorPositionY.Count; h++)
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

        /// <summary>
        /// Updates the elevator visual.  Different look during move and at stop.
        /// </summary>
        /// <param name="doorOpen">If set to <c>true</c> door open.</param>
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

        /// <summary>
        ///    Elevator arrives at a stop. update the list and state.
        /// </summary>
        void OnStop()
        {
            Debug.Log("OnStop, elv:" + elvId);
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
            handleStateUpdate?.Invoke(CurrentFloor, LiftState);
        }

        /// <summary>
        /// Resumes the lifting.
        /// </summary>
        void ResumeLifting()
        {
            if (LiftState==LiftState.PauseUp)
            {
                if (UpStops.Count > 0)
                {
                    PrepareMoveUp();
                }
                else
                {
                    PrepareMoveDown();
                }
            } 
            else if (LiftState == LiftState.PauseDown)
            {
                if (DownStops.Count > 0)
                {
                    PrepareMoveDown();
                }
                else
                {
                    PrepareMoveUp();
                }
            }
           
        }

        /// <summary>
        ///   respond to the click event
        /// </summary>
        public void OnClick()
        {

            if (LiftState == LiftState.PauseDown || 
                LiftState == LiftState.PauseUp ||
                LiftState == LiftState.Still)
            {
                OpenPanel();
            }
        }

        /// <summary>
        /// Opens the button panel inside elevator
        /// </summary>
        void OpenPanel()
        {
            if (elevatorPanel == null)
            {
                GameObject go = GameObjectUtil.InstantiateAndAnchor(PanelPrefab,
                    transform.parent.parent.parent,
                    new Vector3(0, 0, 0)
                    );
                elevatorPanel = go.GetComponent<ElevatorPanel>();
                if (elevatorPanel != null)
                {
                    elevatorPanel.Setup(elvId, TopFloor + 1, LiftState, null, AddRequest, ClosePanel);
                }
                handleStateUpdate += elevatorPanel.UpdateState;

            } else
            {
                elevatorPanel.gameObject.SetActive(true);
            }
        }

        void ClosePanel()
        {
            if (elevatorPanel != null)
            {
                elevatorPanel.gameObject.SetActive(false);
            }
        }


        /// <summary>
        ///   Handle a new request during the move
        /// </summary>
        /// <param name="floor">Floor.</param>
        void UpdateCourse(int floor)
        {
            if (floor > CurrentFloor && LiftState == LiftState.Up) {
                PrepareMoveUp();
            } 
            else if (floor < CurrentFloor && LiftState == LiftState.Down)
            {
                PrepareMoveDown();
            }
        }

        /// <summary>
        /// Prepares the move up.
        /// </summary>
        void PrepareMoveUp()
        {
            if (UpStops.Count > 0)
            {
                Vector3 currentPos = transform.position;
                int targetFloor = UpStops.Min();
                Debug.Log("PrepareMoveUp target:" + targetFloor);
                TargetPosition = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
                LiftState = LiftState.Up;
                UpdateElevatorVisual(false);
            }
        }

        /// <summary>
        /// Prepares the move down.
        /// </summary>
        void PrepareMoveDown()
        {
            if (DownStops.Count > 0)
            {
                Vector3 currentPos = transform.position;
                int targetFloor = DownStops.Max();
                Debug.Log("PrepareMoveDown target:" + targetFloor);
                TargetPosition = new Vector3(currentPos.x, floorPositionY[targetFloor], currentPos.z);
                LiftState = LiftState.Down;
                UpdateElevatorVisual(false);
            }
        }

    }

}
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SimElevator
{
    public class GameController : MonoBehaviour
    {
        #region Declarations
        [SerializeField] int numOfFloors = 5;

        [SerializeField] int numOfElevators = 1;

        [SerializeField] float speed = 1.0f;
        [SerializeField] float liftPauseTime = 3.0f;

        [SerializeField] GameObject ShaftPrefab = null;
        [SerializeField] GameObject ButtonsPrefab = null;
        [SerializeField] GameObject Building = null;

        const float BUILDING_WIDTH = 700f;
        const float FLOOR_HEIGHT = 100f;

        List<Elevator> elevators = new List<Elevator>();

        List<ButtonPanel> buttonPanels = new List<ButtonPanel>();

        #endregion


        void Awake()
        {
            SetupElevators();
            SetupButtonPanels();
        }


        /// <summary>
        /// Setups the elevators.
        /// </summary>
        void SetupElevators()
        {
 
            for (int i = 0; i < numOfElevators; i++)
            {
                float x = (i+1)*BUILDING_WIDTH / (numOfElevators + 1) - BUILDING_WIDTH/2;
                GameObject go = GameObjectUtil.InstantiateAndAnchor(ShaftPrefab, this.Building.transform, 
                    new Vector3(x, 0, 0));

                Elevator elevator = go.GetComponentInChildren<Elevator>();

                elevator.Setup(i, numOfFloors, FLOOR_HEIGHT, speed, liftPauseTime, HandleElevatorArrival);
                go.name = "Shaft(Elevator) " + i;
                elevators.Add(elevator);
            }

        }

        /// <summary>
        /// Setups the button panels.
        /// </summary>
        void SetupButtonPanels()
        {
            List<float> floorPositionY = new List<float>();
            for (int f = 0; f < numOfFloors; f++)
            {
                float y = 75 + FLOOR_HEIGHT * f ;
                GameObject go = GameObjectUtil.InstantiateAndAnchor(ButtonsPrefab, Building.transform,
                    new Vector3( - BUILDING_WIDTH/2 - 50, y, 0));
                go.name = "ButtonPanel " + f;
                ButtonPanel buttonPanel = go.GetComponentInChildren<ButtonPanel>();
                if (buttonPanel != null)
                {
                    buttonPanel.RegisterActionHandlers(f, HandleUpCall, HandleDownCall);
                }
                if (f==0)
                {
                    buttonPanel.SetAsBottom();
                } else if (f == numOfFloors - 1)
                {
                    buttonPanel.SetAsTop();
                }
                buttonPanels.Add(buttonPanel);
            }
        }

        /// <summary>
        ///   Check if there is already an elevator stopping at this floor
        /// </summary>
        /// <returns><c>true</c>, if elevator is sthere, <c>false</c> otherwise.</returns>
        /// <param name="floor">Floor.</param>
        bool HasElevator(int floor)
        {
            return elevators.Any(x => floor == x.CurrentFloor);
        }

        /// <summary>
        /// Handles calling to go up (UP button pressed on a certain floor)
        /// </summary>
        /// <param name="floor">Floor.</param>
        void HandleUpCall(int floor)
        {
            Debug.Log("floor " + floor + " is calling for UP");
            if (HasElevator(floor))
            {
                Debug.Log("There is a elavtor waiting already...");

                buttonPanels[floor].ResetUpButton();
                return;
            }

            foreach(Elevator elevator in elevators)
            {
                elevator.AddRequest(floor);
            }
        }

        /// <summary>
        /// Handles calling to go down (Down button pressed on a certain floor)
        /// </summary>
        /// <param name="floor">Floor.</param>
        void HandleDownCall(int floor)
        {
            Debug.Log("floor " + floor + " is calling for Down");
            if (HasElevator(floor))
            {
                Debug.Log("There is a elavtor waiting already...");
                buttonPanels[floor].ResetDownButton();
                return;
            }

            foreach (Elevator elevator in elevators)
            {
                elevator.AddRequest(floor);
            }
        }

        /// <summary>
        /// Handles the elevator arrival.  Cancel the highlight on the requesting 
        /// floor button.
        /// </summary>
        /// <param name="floor">Floor.</param>
        /// <param name="state">State.</param>
        void HandleElevatorArrival(int floor, LiftState state)
        {
            //Debug.Log("floor " + floor + " got an elevator, state = " + state );

            switch (state)
            {
                case LiftState.PauseDown:
                    if (floor == 0)
                    {
                        buttonPanels[floor].ResetUpButton();
                    }
                    else
                    {
                        buttonPanels[floor].ResetDownButton();
                    }
                    break;
                case LiftState.PauseUp:
                    if (floor == numOfFloors - 1)
                    {
                        buttonPanels[floor].ResetDownButton();
                    }
                    else
                    {
                        buttonPanels[floor].ResetUpButton();
                    }
                    break;
                case LiftState.Still:
                    buttonPanels[floor].ResetUpButton();
                    buttonPanels[floor].ResetDownButton();
                    break;
            }

            // Cancel request
            foreach(Elevator elevator in elevators)
            {
                elevator.CancelRequest(floor);
            }

        }
    }

}
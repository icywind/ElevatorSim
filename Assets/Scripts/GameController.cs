using System.Collections.Generic;
using UnityEngine;

namespace SimElevator
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        int numOfFloors = 5;

        [SerializeField]
        int numOfElevators = 1;

        List<Elevator> elevators = new List<Elevator>();

        [SerializeField]
        List<ButtonPanel> buttonPanels = null;

        [SerializeField] GameObject ShaftPrefab = null;
        [SerializeField] GameObject ButtonsPrefab = null;
        [SerializeField] GameObject Building = null;

        const float BUILDING_WIDTH = 700f;
        const float FLOOR_HEIGHT = 100f;


        void Awake()
        {
            SetupElevators();
            SetupButtonPanels();
        }


        void SetupElevators()
        {
 
            for (int i = 0; i < numOfElevators; i++)
            {
                float x = (i+1)*BUILDING_WIDTH / (numOfElevators + 1) - BUILDING_WIDTH/2;
                GameObject go = GameObjectUtil.InstantiateAndAnchor(ShaftPrefab, this.Building.transform, 
                    new Vector3(x, 0, 0));

                Elevator elevator = go.GetComponentInChildren<Elevator>();

                elevator.Setup(i, numOfFloors, FLOOR_HEIGHT, HandleElevatorArrival);
                go.name = "Shaft(Elevator) " + i;
                elevators.Add(elevator);
            }

        }


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

        void HandleUpCall(int floor)
        {
            Debug.Log("floor " + floor + " is calling for UP");
            foreach(Elevator elevator in elevators)
            {
                elevator.AddRequest(floor);
            }
        }
        void HandleDownCall(int floor)
        {
            Debug.Log("floor " + floor + " is calling for Down");
            foreach (Elevator elevator in elevators)
            {
                elevator.AddRequest(floor);
            }
        }

        void HandleElevatorArrival(int floor, LiftState state)
        {
            Debug.Log("floor " + floor + " got an elevator, state = " + state );



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
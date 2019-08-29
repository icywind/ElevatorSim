using System.Diagnostics;
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
        List<ButtonPanel> buttonPanels;

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

                elevator.SetFloors(numOfFloors, FLOOR_HEIGHT);
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
            }
        }

    }

}
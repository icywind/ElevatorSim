﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SimElevator {
    public class ElevatorPanel : MonoBehaviour
    {
        [SerializeField]
        GameObject ButtonPrefab = null;

        [SerializeField]
        Image UpArrow = null;

        [SerializeField]
        Image DownArrow = null;

        [SerializeField]
        Text ElevatorTitle = null;
        [SerializeField]
        Text FloorTitle = null;

        [SerializeField]
        GameObject ButtonContainer = null;

        [SerializeField]
        List<Button> floorButtons = new List<Button>();

        HashSet<int> SelectedFloors = new HashSet<int>();

        const float CONTAINER_HEIGHT = 600;

        Action<int> OnFloorSelect;
        LiftState liftState;
        int totalFloors = 0;
        int currentFloor = 0;

        private void Start()
        {
            // TEST CODE
            /*
            Setup(2, 5, (f) => {
                Debug.Log("Floor selected: " + f);
            }, 
            LiftState.Up, new List<int> { 1, 4 });
            */

            Time.timeScale = 0;           
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Time.timeScale = 1;
                Destroy(gameObject);
            }
        }

        public void Setup(int elvId, int numFloors, Action<int> floorUpdate, LiftState state, IEnumerable<int>selectedFloors)
        {
            liftState = state;
            OnFloorSelect = floorUpdate;
            totalFloors = numFloors;
            if (selectedFloors != null)
            {
                SelectedFloors = new HashSet<int>(selectedFloors);
            }
            InitButtons();
            UpdateState(state, 0);
            ElevatorTitle.text = string.Format("ELEVATOR {0}", elvId);
        }
    
        public void UpdateState(LiftState state, int floor)
        {
            FloorTitle.text = floor.ToString();
            currentFloor = floor;

            switch (state)
            {
                case LiftState.PauseDown:
                    HighlightButtonText(floor, false);
                    break;

                case LiftState.Down:
                    EnableDownArrow(true);
                    EnableUpArrow(false);
                    break;
                
                case LiftState.PauseUp:
                    HighlightButtonText(floor, false);
                    break;

                case LiftState.Up:
                    EnableUpArrow(true);
                    EnableDownArrow(false);
                    break;

                default:
                    EnableUpArrow(false);
                    EnableDownArrow(false);
                    break;
            }

        }

        void EnableUpArrow(bool enable)
        {
            UpArrow.color = enable ? Color.white : Color.black;
        }

        void EnableDownArrow(bool enable)
        {
            DownArrow.color = enable ? Color.white : Color.black;
        }

        static Color HIGHLIGHTED_RED = new Color(231/255, 60/255, 50/255);
        static Color BASE_DARK_RED = new Color(111/255, 19/255, 19/255);

        void HighlightButtonText(int floor, bool highlighting)
        {
            Button button = floorButtons[floor];
            Text btnText = button.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = highlighting ? Color.red : Color.black;
            }
        }

        void InitButtons()
        {
            for(int f = 0; f < totalFloors; f++)
            {
                float posY = (f + 1) * CONTAINER_HEIGHT / (totalFloors + 1) - CONTAINER_HEIGHT / 2;
                Vector3 btnPosition = new Vector3(0, posY, 0);
                GameObject go = GameObjectUtil.InstantiateAndAnchor(ButtonPrefab, ButtonContainer.transform,
                    btnPosition);
                Button button = go.GetComponent<Button>();
                Text btnTxt = button.GetComponentInChildren<Text>();
                if (btnTxt != null)
                {
                    btnTxt.text = f.ToString();
                    btnTxt.color = SelectedFloors.Contains(f) ? Color.red : Color.black;
                }
                int floor = f;
                button.onClick.AddListener(() => {
                    OnButtonPressed(floor);
                ; });
                floorButtons.Add(button);
            }
        }

        void OnButtonPressed(int floor)
        {

            if (floor != currentFloor && !SelectedFloors.Contains(floor))
            {
                HighlightButtonText(floor, true);
                SelectedFloors.Add(floor);
                if (OnFloorSelect != null)
                {
                    OnFloorSelect(floor);
                }
            }

        }
    }
}
using System;
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

        int totalFloors = 0;
        int currentFloor = 0;
        Action OnClose;
        Action<int> OnFloorSelect;

        private void OnEnable()
        {
            Time.timeScale = 0;           
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Time.timeScale = 1;
                OnClose?.Invoke();
            }
        }

        public void Setup(int elvId, int numFloors, LiftState state, IEnumerable<int>selectedFloors, Action<int> floorUpdate, Action onClose)
        {
            OnFloorSelect = floorUpdate;
            OnClose = onClose;
            totalFloors = numFloors;
            if (selectedFloors != null)
            {
                SelectedFloors = new HashSet<int>(selectedFloors);
            }
            InitButtons();
            UpdateState(0, state);
            ElevatorTitle.text = string.Format("ELEVATOR {0}", elvId);
        }
    
        public void UpdateState(int floor, LiftState state)
        {
            FloorTitle.text = floor.ToString();
            currentFloor = floor;

            // Debug.Log("Update state floor:" + floor + " state: " + state);

            switch (state)
            {
                case LiftState.PauseDown:
                case LiftState.PauseUp:
                    HandleStop(floor);
                    break;

                case LiftState.Down:
                    EnableDownArrow(true);
                    EnableUpArrow(false);
                    break;

                case LiftState.Up:
                    EnableUpArrow(true);
                    EnableDownArrow(false);
                    break;

                default:
                    EnableUpArrow(false);
                    EnableDownArrow(false);
                    HandleStop(floor);
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

        void HandleStop(int floor)
        {
            SelectedFloors.Remove(floor);
            HighlightButtonText(floor, false);
        }

        void OnButtonPressed(int floor)
        {

            if (floor != currentFloor && !SelectedFloors.Contains(floor))
            {
                HighlightButtonText(floor, true);
                SelectedFloors.Add(floor);
                OnFloorSelect?.Invoke(floor);
            }

        }
    }
}
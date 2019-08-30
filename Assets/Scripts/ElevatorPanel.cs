using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SimElevator {
    /// <summary>
    /// Elevator panel class provides access to the internal
    /// control panel to selection of floors at an elevator.
    /// </summary>
    public class ElevatorPanel : MonoBehaviour
    {
        #region Declartions

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

        #endregion

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

        /// <summary>
        /// Setup the specified elvId, numFloors, state, selectedFloors, floorUpdate and onClose.
        /// </summary>
        /// <param name="elvId">Elv identifier.</param>
        /// <param name="numFloors">Number floors.</param>
        /// <param name="state">State.</param>
        /// <param name="selectedFloors">Selected floors.</param>
        /// <param name="floorUpdate">Floor update.</param>
        /// <param name="onClose">On close.</param>
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
    
        /// <summary>
        /// Updates the state in response to the Lift event (changed floor and/or   
        ///   updated state)
        /// </summary>
        /// <param name="floor">Floor.</param>
        /// <param name="state">State.</param>
        public void UpdateState(int floor, LiftState state)
        {
            FloorTitle.text = floor.ToString();
            currentFloor = floor;

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

        /// <summary>
        /// Enables tje up arrow for direction
        /// </summary>
        /// <param name="enable">If set to <c>true</c> enable.</param>
        void EnableUpArrow(bool enable)
        {
            UpArrow.color = enable ? Color.white : Color.black;
        }

        /// <summary>
        /// Enables down arrow for direction.
        /// </summary>
        /// <param name="enable">If set to <c>true</c> enable.</param>
        void EnableDownArrow(bool enable)
        {
            DownArrow.color = enable ? Color.white : Color.black;
        }

        /// <summary>
        /// Highlights the button text.
        /// </summary>
        /// <param name="floor">Floor.</param>
        /// <param name="highlighting">If set to <c>true</c> highlighting.</param>
        void HighlightButtonText(int floor, bool highlighting)
        {
            Button button = floorButtons[floor];
            Text btnText = button.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = highlighting ? Color.red : Color.black;
            }
        }

        /// <summary>
        /// Instantiate the buttons.
        /// </summary>
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

        /// <summary>
        /// Handles the stop event
        /// </summary>
        /// <param name="floor">Floor.</param>
        void HandleStop(int floor)
        {
            SelectedFloors.Remove(floor);
            HighlightButtonText(floor, false);
        }

        /// <summary>
        /// Responds to a button pressed.
        /// </summary>
        /// <param name="floor">Floor.</param>
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
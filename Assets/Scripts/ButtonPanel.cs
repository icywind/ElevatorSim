using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimElevator
{
    public class ButtonPanel : MonoBehaviour
    {
        [SerializeField] Button upButton = null;
        [SerializeField] Button downButton = null;
        [SerializeField] Text floorText = null;

        private int floorID { get; set; }
        private Action<int> handleUpAction = null;
        private Action<int> handleDownAction = null;

        private void Awake()
        {
            upButton.onClick.AddListener(OnUpButton);
            downButton.onClick.AddListener(OnDownButton);
        }

        private void OnDestroy()
        {
            handleUpAction = null;
            handleDownAction = null;
        }


        private void OnUpButton()
        {
            upButton.image.color = Color.yellow;
            handleUpAction?.Invoke(floorID);
        }

        private void OnDownButton()
        {
            downButton.image.color = Color.yellow;
            handleDownAction?.Invoke(floorID);
        }

        public void RegisterActionHandlers(int floorid, Action<int> upAction, Action<int> downAction)
        {
            handleUpAction = upAction;
            handleDownAction = downAction;
            floorID = floorid;
            floorText.text = floorid.ToString();
        }

        public void ResetUpButton()
        {
            upButton.image.color = Color.white;
        }
        public void ResetDownButton()
        {
            downButton.image.color = Color.white;
        }

        public void SetAsTop()
        {
            // top floor doesn't show the UP button
            upButton.interactable  = false;
        }

        public void SetAsBottom()
        {
            // bottom floor doesn't show the DOWN button
            downButton.interactable  = false;
        }
    }
}
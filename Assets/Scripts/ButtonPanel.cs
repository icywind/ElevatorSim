using UnityEngine;
using UnityEngine.UI;

public class ButtonPanel : MonoBehaviour
{
    [SerializeField] Button upButton;
    [SerializeField] Button downButton;

    private void Awake()
    {
        upButton.onClick.AddListener(OnUpButton);
        downButton.onClick.AddListener(OnDownButton);
    }

    private void OnUpButton()
    {
        upButton.image.color = Color.yellow;
    }

    private void OnDownButton()
    {
        downButton.image.color = Color.yellow;
    }

    public void ResetUpButton()
    {
        upButton.image.color = Color.white;
    }
    public void ResetDownButton()
    {
        downButton.image.color = Color.white;
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GazeHoverColorTest : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        uiCanvas.gameObject.SetActive(true);
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        uiCanvas.gameObject.SetActive(false);
    }
}
using UI;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyAddUI : MonoBehaviour
{
    public AstralBodyPlacementUI astralBodyPlacementUI;
    public Button button;

    public void Switch2Placement()
    {
        button.gameObject.SetActive(false);
        astralBodyPlacementUI.SetPlacing();
        astralBodyPlacementUI.gameObject.SetActive(true);
    }

    public void Switch2Normal()
    {
        button.gameObject.SetActive(true);
        astralBodyPlacementUI.gameObject.SetActive(false);
    }
}
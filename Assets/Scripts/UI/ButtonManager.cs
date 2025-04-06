using UnityEngine;
using System.Collections.Generic;
using static GameManager;

public class ButtonManager : MonoBehaviour
{
    private List<ButtonBehaviour> buttons = new List<ButtonBehaviour>();
    public ButtonManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterButton(ButtonBehaviour button)
    {
        if (!buttons.Contains(button))
        {
            buttons.Add(button);
        }
    }

    public void ActivateButton(ButtonBehaviour activeButton)
    {
        //Debug.Log(activeButton.gameObject.name);
        if (activeButton.gameObject.name == "CamButton")
        {
            GameManager.Instance.ChangeInputMode(InputMode.Camera);
            GridManager.Instance.DeactivateRings();
        }
        else if (activeButton.gameObject.name == "MoveButton")
        {
            GameManager.Instance.ChangeInputMode(InputMode.Movement);
            GridManager.Instance.VisualizeRange(BaseUnitManager.Instance.SelectedPlayer.OccupiedTile, BaseUnitManager.Instance.SelectedPlayer.Movement);
        }
        else if (activeButton.gameObject.name == "AttackButton")
        {
            GameManager.Instance.ChangeInputMode(InputMode.Attack);
            GridManager.Instance.VisualizeAttackRange(BaseUnitManager.Instance.SelectedPlayer.OccupiedTile);
        }

        foreach (var button in buttons)
        {
            if (button != activeButton)
            {
                button.SetPressedState(false);
            }
        }
    }
}

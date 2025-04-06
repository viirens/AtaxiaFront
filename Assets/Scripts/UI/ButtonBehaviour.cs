using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ButtonManager manager;
    public Image buttonBackground; // Reference to the button's background image

    public Color defaultColor = Color.white; // Default color
    public Color pressedColor = Color.red; // Color when pressed

    public bool isPressed = false; // State tracking
    public string mapPoint;

    public static event Action<GameManager.GameState> endTurn;

    public AlignedCam cameraMovement;

    void Start()
    {
        if (buttonBackground)
        {
            manager.RegisterButton(this);
            buttonBackground.color = isPressed ? pressedColor : defaultColor; // Set initial color based on state
        }
    }

    public void OnEndTurnButtonPress()
    {
        MenuManager.Instance.ToggleEndTurnButton(false);
        //BaseUnitManager.Instance.ResetTurnBasedValues();
        //endTurn?.Invoke(GameManager.GameState.EnemyTurn);
        BaseUnitManager.Instance.ResolvePlayerTurn();
    }

    public void OnNextPlayerButtonPress()
    {
        BaseUnitManager.Instance.IteratePlayer();
        GameManager.Instance.ChangeState(GameManager.GameState.PlayerTurn);
    }

    public void OnDeselectPlayerButtonPress()
    {
        BaseUnitManager.Instance.ClearSelectedPlayer();
    }

    public void MoveCameraUp()
    {
        cameraMovement.MoveCamera("up");
    }

    public void MoveCameraDown()
    {
        cameraMovement.MoveCamera("down");
    }

    public void MoveCameraRight()
    {
        cameraMovement.MoveCamera("right");
    }

    public void MoveCameraLeft()
    {
        cameraMovement.MoveCamera("left");
    }

    //public void ToggleCamera()
    //{
    //    GameManager.Instance.CameraEnabled = true;
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GridManager.Instance) GridManager.Instance.ClearPath();
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    //public void ToggleButtonColor()
    //{
    //    isPressed = !isPressed; // Toggle state
    //    buttonBackground.color = isPressed ? pressedColor : defaultColor;
    //}

    public void ToggleButtonColor()
    {
        isPressed = !isPressed;
        if (isPressed)
        {
            manager.ActivateButton(this); // Inform the manager that this button was pressed
        }
        buttonBackground.color = isPressed ? pressedColor : defaultColor;
    }

    // Add this method
    public void SetPressedState(bool pressed)
    {
        isPressed = pressed;
        buttonBackground.color = isPressed ? pressedColor : defaultColor;
    }

    public void SetActiveState()
    {
        isPressed = true;
        buttonBackground.color = pressedColor;
        manager.ActivateButton(this); // Inform the manager that this button was pressed
    }

    public void SelectMapEntry()
    {
        Debug.Log(mapPoint);
        PlayerPrefs.SetString("SelectedEdge", mapPoint);
        PlayerPrefs.Save();
        StartCoroutine(MenuManager.Instance.FadeCanvasOut(1.0f, MainMenu.Instance.PlayGame));
    }
}
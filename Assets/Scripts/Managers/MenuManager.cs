using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MenuManager : MonoBehaviour, IObserver
{
    public static MenuManager Instance;

    [SerializeField] private TextMeshProUGUI _selectedWeaponObject, _selectedPlayerObject, _selectedEnemyObject, _tileObject, _tileUnitObject, _gameStateObject;
    [SerializeField] public GameObject openInventoryButton;
    [SerializeField] public GameObject PlayerButtonContainer;
    [SerializeField] public Button PlayerButtonPrefab;
    public GameObject[] _nextTurnButtons;
    public CanvasGroup canvasGroup;

    private BaseUnit _selectedUnit;
    private BaseEnemy _selectedEnemyUnit;

    void Awake()
    {
        Instance = this;
        // subscribe to state changed event
        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
        _nextTurnButtons = GameObject.FindGameObjectsWithTag("PlayerTurnMenu");
    }

    private void OnDestroy()
    {
        // unsubscribe
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameManager.GameState obj)
    {
        _gameStateObject.text = obj.ToString();
        //may be a better way to do this
        if (obj == GameManager.GameState.PlayerTurn) this.ToggleEndTurnButton(true);
        else if (obj == GameManager.GameState.EnemyTurn) this.ToggleEndTurnButton(false);
    }

    public void ShowTileInfo(Tile tile)
    {
        if (tile == null) return;

        _tileUnitObject.text = tile?.OccupyingUnit?.UnitName;
    }

    public void ShowSelectedUnit(BaseUnit unit)
    {
        TextMeshProUGUI textObject;
        if (unit)
        {
            textObject = unit.Faction == Faction.player ? _selectedPlayerObject : _selectedEnemyObject;
            textObject.text = "Name: " + unit.UnitName + "\n" + "Mvmt: " + unit.Movement + "\n" + "Health: " + unit.Health;
            if (unit.Faction == Faction.player)
            {
                BasePlayer player = (BasePlayer)unit;
                textObject.text += "\nActions: " + player.Actions;
            }
        }
        else
        {
            textObject = _selectedPlayerObject;
            textObject.text = "";
        }

    }

    public void ToggleEndTurnButton(bool display)
    {
        // Loop through each GameObject and set its active state
        foreach (GameObject button in _nextTurnButtons)
        {
            button.SetActive(display);
        }
    }

    public void SelectUnit(BaseUnit unit)
    {
        if (_selectedUnit != null)
        {
            // If another unit was selected before, stop observing it
            _selectedUnit.RemoveObserver(this);
        }

        _selectedUnit = unit;

        if (_selectedUnit != null)
        {
            // Start observing the new selected unit
            _selectedUnit.RegisterObserver(this);

            // Immediately update the display with the current data
            UpdateObserver(_selectedUnit.Health, _selectedUnit.WeaponAmmo);
        }
    }

    public void SelectEnemyUnit(BaseEnemy unit)
    {
        if (_selectedEnemyUnit != null)
        {
            // If another unit was selected before, stop observing it
            _selectedEnemyUnit.RemoveObserver(this);
        }

        _selectedEnemyUnit = unit;

        if (_selectedEnemyUnit != null)
        {
            // Start observing the new selected unit
            _selectedEnemyUnit.RegisterObserver(this);

            // Immediately update the display with the current data
            UpdateEnemyObserver(_selectedEnemyUnit.WeaponAmmo);
        }
    }

    public void UpdateObserver(int health, int ammo)
    {
        TextMeshProUGUI playerTextObject = _selectedPlayerObject;
        if (_selectedUnit != null)
        {
            // there should be selected enemy unit and selected player unit

            playerTextObject.text = "Name: " + _selectedUnit.UnitName + "\n" + "Mvmt: " + _selectedUnit.Movement + "\n" + "Health: " + _selectedUnit.Health + "\n" + "Location: " + _selectedUnit.OccupiedTile.Location;
            UpdateWeaponAmmo(ammo);

            if (_selectedUnit is BasePlayer player)
            {
                playerTextObject.text += "\nActions: " + player.Actions;
            }
        }
        else
        {
            playerTextObject.text = "";
        }
    }

    public void UpdateEnemyObserver(int ammo)
    {
        if (_selectedUnit != null)
        {
            // there should be selected enemy unit and selected player unit
            TextMeshProUGUI enemyTextObject = _selectedEnemyObject;
            enemyTextObject.text = "Name: " + _selectedEnemyUnit.UnitName + "\n" + "Mvmt: " + _selectedEnemyUnit.Movement + "\n" + "Health: " + _selectedEnemyUnit.Health + "\n" + "Crits: " + _selectedEnemyUnit.Crits;
        }
    }

    public void UpdateWeaponAmmo(int ammo)
    {
        if (_selectedUnit != null && _selectedUnit.EquippedWeapon != null)
        {
            _selectedWeaponObject.text = "Weapon: " + _selectedUnit.EquippedWeapon.itemName + "\n" + "Damage: " + _selectedUnit.EquippedWeapon.Damage + "\n" + "Ammo: " + _selectedUnit.EquippedWeapon.Ammo;
        }
    }

    public void CreatePlayerSelectButton(BasePlayer player)
    {
        // Assuming you have a PlayerButtonPrefab and a parent UI element to hold these buttons
        var playerButton = Instantiate(PlayerButtonPrefab, PlayerButtonContainer.transform);
        playerButton.GetComponentInChildren<TextMeshProUGUI>().text = player.UnitName; // Set the button's text to the player's name or any identifier


        // Set the player's sprite
        Image playerSpriteImage = playerButton.transform.Find("Image").GetComponent<Image>();
        // Get the sprite from the player's UnitPrefab SpriteRenderer
        SpriteRenderer unitSpriteRenderer = player.UnitPrefab.GetComponent<SpriteRenderer>();
        if (unitSpriteRenderer != null)
        {
            playerSpriteImage.sprite = unitSpriteRenderer.sprite;
        }

        // Add a click listener to the button
        playerButton.GetComponent<Button>().onClick.AddListener(() => BaseUnitManager.Instance.SetSelectedPlayer(player));
    }

    public IEnumerator FadeCanvasIn(float duration)
    {
        yield return new WaitForSeconds(.75f);
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f; // Ensure the canvas is fully visible at the end
    }

    public IEnumerator FadeCanvasOut(float duration, Action onFinish)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("FadeCanvasOut was called but canvasGroup is null.");
            yield break;
        }

        Debug.Log("Starting FadeCanvasOut coroutine.");

        yield return new WaitForSeconds(.75f);

        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime if the game might be paused
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            Debug.Log($"Fading out... Alpha is {canvasGroup.alpha}"); // Check the alpha value over time
            yield return null;
        }

        canvasGroup.alpha = 0f; // Ensure the canvas is fully transparent at the end
        Debug.Log("Fade out completed.");

        // Check if the callback is not null, then call it
        onFinish?.Invoke();
    }
}

public class HealthDisplay : MonoBehaviour, IObserver
{
    [SerializeField]
    private Text healthText;
    private BaseUnit unitToObserve;

    private void Start()
    {
        // You need some way to determine which unit this HealthDisplay should observe.
        // This is just one example, you might need a different method.
        unitToObserve = GetUnitToObserve();
        if (unitToObserve != null)
        {
            unitToObserve.RegisterObserver(this);
        }
    }

    public void UpdateObserver(int health, int ammo)
    {
        healthText.text = $"Health: {health}";
    }

    private void OnDestroy()
    {
        if (unitToObserve != null)
        {
            unitToObserve.RemoveObserver(this);
        }
    }

    private BaseUnit GetUnitToObserve()
    {
        throw new NotImplementedException();
    }

}


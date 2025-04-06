using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

abstract public class BaseUnit : MonoBehaviour, ISubject
{
    public string UnitName;
    public Faction Faction;
    public GameObject UnitPrefab;
    public Tile OccupiedTile;
    internal Vector3 coordinate;
    public bool IsReadyForDestruction { get; private set; } = false;

    public Inventory UnitInventory = new Inventory();

    protected int _crits;
    public int Crits
    {
        get { return _crits; }
        set
        {
            if (_crits != value)
            {
                _crits = value;
                NotifyObservers();
            }
        }
    }

    public int Health
    {
        get { return _health; }
        set
        {
            if (_health != value)
            {
                _health = value;
                NotifyObservers();
            }
        }
    }

    // This provides a way to access the equipped weapon directly
    public Weapon EquippedWeapon
    {
        get { return UnitInventory.EquippedWeapon; }
    }

    public int WeaponAmmo
    {
        get
        {
            if (EquippedWeapon != null)
            {
                return EquippedWeapon.Ammo;
            }
            return 0; // default value when there's no equipped weapon
        }
        set
        {
            if (EquippedWeapon != null && EquippedWeapon.Ammo != value)
            {
                EquippedWeapon.Ammo = value;
                NotifyObservers();
            }
        }
    }

    protected int _health;
    abstract public int InitMovement { get; set; }
    abstract public int Movement { get; set; }
    abstract public int Range { get; set; }

    public bool inMovement { get; set; }
    public Weapon Weapon { get; private set; }

    public BaseUnit()
    {
        Health = 100; // arbitrary value
    }

    public void Init(Weapon weapon = null)
    {
        Weapon genWeapon = weapon ?? Resources.Load<Weapon>("Items/Weapons/9mm");
        UnitInventory.AddItem(genWeapon);
        UnitInventory.EquipWeapon(genWeapon);
    }

    public void TempAttack(BaseUnit targetUnit, int distance)
    {
        if (targetUnit.IsReadyForDestruction) return;
        //if (WeaponAmmo > 0)
        //{
        Debug.Log(EquippedWeapon);
        int damage = EquippedWeapon.RollForHit(distance);
        //int critHits = 0;

        if (damage > 0)
        {
            int critHits = EquippedWeapon.NumberOfCritHits(distance);
            if (critHits == 0) DamagePopupManager.Instance.CreateDamagePopup(damage, targetUnit.transform.position, false); // For miss
            else
            {
                targetUnit.ReceiveCriticalDamage(critHits);
                StartCoroutine(ShowMultipleCritPopups(critHits, targetUnit.transform.position, targetUnit));
            }
            Debug.Log("critHits: " + critHits);
        }
        else if (damage == 0)
        {
            Debug.Log("attack missed");
            DamagePopupManager.Instance.CreateDamagePopup(0, targetUnit.transform.position, false); // For miss
        }

        // Deduct ammo from the attacker's weapon
        WeaponAmmo -= 1;

        targetUnit.ReceiveDamage(damage);
        //if (critHits > 0)
        //{
        //    targetUnit.ReceiveCriticalDamage(critHits);
        //    DamagePopupManager.Instance.CreateDamagePopup(0, targetUnit.transform.position, true); // Critical hit popup
        //    //StartCoroutine(ShowMultipleCritPopups(critHits, targetUnit.transform.position));
        //}
        //}
        //else
        //{
        //    Debug.Log("Out of ammo, please reload.");
        //}
    }

    private IEnumerator ShowMultipleCritPopups(int critHits, Vector3 position, BaseUnit targetUnit)
    {
        for (int i = 0; i < critHits; i++)
        {
            DamagePopupManager.Instance.CreateDamagePopup(0, position, true); // Critical hit popup
            yield return new WaitForSeconds(0.35f); // Adjust delay as needed
        }

        if (targetUnit.IsReadyForDestruction)
        {
            StartCoroutine(targetUnit.DeathAnimation());
        }
    }

    public void ReceiveDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Debug.Log("Enemy is dead.");
        }
        else
        {
            Debug.Log($"Enemy receives {damage} damage. Health is now: {Health}");
        }
    }

    public void ReceiveCriticalDamage(int critsHits)
    {
        Crits -= critsHits;

        if (Crits <= 0)
        {
            Debug.Log("Enemy is dead. Marked for destruction.");
            IsReadyForDestruction = true;
            //BaseUnitManager.Instance.DestroyUnit(this, Faction.enemy);
            //Destroy(this.gameObject);
        }
        else
        {
            Debug.Log($"Enemy receives {critsHits} crits. {critsHits} crits remaining.");
        }
    }

    protected List<IObserver> observers = new List<IObserver>();

    public void RegisterObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    public void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.UpdateObserver(Health, WeaponAmmo);
        }
    }

    public IEnumerator DeathAnimation()
    {
        float duration = 2.0f;
        float flipDurationFraction = 0.2f; // Fraction of the duration to complete the flip

        // Initial and final rotation angles
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(180f, 0f, 0f); // Flip upside down

        // Initial and final positions
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(0f, 0f, -1f); // Rise along the z-axis

        // Get the sprite renderer to handle fading
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); // Fade to transparent

        // Bobbing effect variables
        float bobHeight = 0.4f; // Maximum height of bobbing
        float flipEndTime = duration * flipDurationFraction;

        // Phase 1: Flipping and Bobbing
        for (float t = 0; t < flipEndTime; t += Time.deltaTime)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t / flipEndTime);

            // Bobbing effect (sinusoidal oscillation)
            float bobbing = Mathf.Sin((t / flipEndTime) * Mathf.PI) * bobHeight;
            transform.position = new Vector3(startPosition.x, startPosition.y, startPosition.z - bobbing); // Bobbing in negative z-direction

            yield return null;
        }

        // Phase 2: Rising and Fading
        for (float t = 0; t < duration - flipEndTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t / (duration - flipEndTime));
            spriteRenderer.color = Color.Lerp(startColor, endColor, t / (duration - flipEndTime));
            yield return null;
        }

        BaseUnitManager.Instance.DestroyUnit(this, this.Faction); // Destroy the unit after animation
    }

    //private IEnumerable<ScriptableObject> GetWeapons<T>(Faction faction) where T : BaseUnit
    //{
    //    List<ScriptableObject> _unitScriptableObjects = Resources.LoadAll<ScriptableObject>("Items/Weapons").ToList();
    //    return _unitScriptableObjects.Where(u => u.Faction == faction);
    //}
}

public interface IObserver
{
    void UpdateObserver(int health, int ammo);
}

public interface IObserver2
{
    void UpdateObserver(BasePlayer player);
}

public interface ISubject
{
    void RegisterObserver(IObserver observer);
    void RemoveObserver(IObserver observer);
    void NotifyObservers();
}

public interface ISubject2
{
    void RegisterObserver(IObserver2 observer);
    void RemoveObserver(IObserver2 observer);
    void NotifyObservers();
}
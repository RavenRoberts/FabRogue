using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    private Controls controls;

    [SerializeField] private bool moveKeyDown;
    [SerializeField] private bool targetMode; //read only
    [SerializeField] private bool isSingleTarget; //read only
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Ability activeAbility;

    private void Awake() => controls = new Controls();

    private void OnEnable()
    {
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.SetCallbacks(null);
        controls.Player.Disable();
    }

    void Controls.IPlayerActions.OnMovement(InputAction.CallbackContext context)
    {
        if (context.started && GetComponent<Actor>().IsAlive)
        {
            if (targetMode && !moveKeyDown)
            {
                moveKeyDown = true;
                Move();
            }
            else if (!targetMode)
            {
                moveKeyDown = true;
            }
        }
        else if (context.canceled)
        {
            moveKeyDown = false;
        }
    }

    void Controls.IPlayerActions.OnExit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (targetMode)
            {
                ToggleTargetMode();
                return;
            }
            if (!UIManager.instance.IsEscapeMenuOpen && !UIManager.instance.IsMenuOpen)
            {
                UIManager.instance.ToggleEscapeMenu();
            }
            else if (UIManager.instance.IsMenuOpen)
            {
                UIManager.instance.ToggleMenu();
            }
        }
    }

    public void OnView(InputAction.CallbackContext context)
    {
        if (context.performed)
            if (!UIManager.instance.IsMenuOpen || UIManager.instance.IsMessageHistoryOpen)
                UIManager.instance.ToggleMessageHistory();
    }

    public void OnPickup (InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CanAct())
            {
                Action.PickupAction(GetComponent<Actor>());
            }
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CanAct() || UIManager.instance.IsInventoryOpen)
            {
                if (GetComponent<Inventory>().Items.Count > 0)
                {
                    UIManager.instance.ToggleInventory(GetComponent<Actor>());
                }
                else
                {
                    UIManager.instance.AddMessage("You have no items", "#808080");
                }
            }
        }
    }

    public void OnWait(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CanAct())
            {
                Action.WaitAction();
            }
        }
    }

    public void OnAbilityMain(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CanAct())
            {
                Actor actor = GetComponent<Actor>();

                Action.UseAbilityAction(actor, 0);
            }
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CanAct() || UIManager.instance.IsInventoryOpen)
            {
                if (GetComponent<Inventory>().Items.Count > 0)
                {
                    UIManager.instance.ToggleDropMenu(GetComponent<Actor>());
                }
                else
                {
                    UIManager.instance.AddMessage("You have no items", "#808080");
                }
            }
        }
    }
    /*
    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Actor actor = GetComponent<Actor>();
            Inventory inv = actor.Inventory;
            Ability activeAbility = this.activeAbility;

            if (targetMode)
            {
                if (isSingleTarget)
                {
                    Actor target = SingleTargetChecks(targetObject.transform.position);

                    if (target != null)
                    {
                        if (inv.SelectedConsumable != null)
                        {
                            Action.CastAction(actor, target, inv.SelectedConsumable);
                        }
                        else if (activeAbility != null)
                        {
                            Action.CastAbilityAction(actor, target, activeAbility);
                        }
                        
                    }
                }
                else
                {
                    List<Actor> targets = AreaTargetChecks(targetObject.transform.position);

                    if (targets != null)
                    {
                        if (inv.SelectedConsumable != null)
                        {
                            Action.CastAction(GetComponent<Actor>(), targets, GetComponent<Inventory>().SelectedConsumable);
                        }
                        else if (activeAbility != null)
                        {
                            bool anySuccess = false;

                            foreach (Actor t in targets)
                            {
                                if (activeAbility.Cast(actor, t.transform.position))
                                {
                                    anySuccess = true;
                                }
                            }
                            if (anySuccess)
                            {
                                GameManager.instance.EndTurn();
                                ToggleTargetMode(false);
                                this.activeAbility = null;
                            }
                        }
                    }
                }
            }
            else if (CanAct())
            {
                Action.TakeStairsAction(GetComponent<Actor>());
            }
        }
    }*/

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Actor actor = (GetComponent<Actor>());
            Inventory inv = actor.Inventory;
            Ability activeAbility = this.activeAbility;

            if (targetMode)
            {
                if (targetObject == null)
                {
                    UIManager.instance.AddMessage("No target selected.", "#808080");
                    return;
                }

                Vector3 targetPos = targetObject.transform.position;

                /*if (inv.SelectedConsumable != null)
                {
                    Action.CastAction(actor, targetPos, inv.SelectedConsumable)
                }*///I have broken items :)
                if (activeAbility != null)
                {
                    Action.CastAbilityAction(actor, targetPos, activeAbility);
                }
            }
            else if (CanAct())
            {
                Action.TakeStairsAction(GetComponent<Actor>());
            }
        }
    }

    public void OnInfo(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(CanAct() || UIManager.instance.IsCharacterInformationMenuOpen)
            {
                UIManager.instance.ToggleCharacterInformationMenu(GetComponent<Actor>());
            }
        }
    }

    public void ToggleTargetMode(bool isArea = false, int radius = 1)
    {
        targetMode = !targetMode;

        if (targetMode)
        {
            if (targetObject.transform.position != transform.position)
            {
                targetObject.transform.position = transform.position;
            }

            if (isArea)
            {
                isSingleTarget = false;
                targetObject.transform.GetChild(0).localScale = Vector3.one * (radius + 1); //+1 to accound for the center
                targetObject.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                isSingleTarget = true;
            }

            targetObject.SetActive(true);
        }
        else
        {
            if (targetObject.transform.GetChild(0).gameObject.activeSelf)
            {
                targetObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            targetObject.SetActive(false);
            GetComponent<Inventory>().SelectedConsumable = null;
        }
    }

    public void ToggleTargetMode(Ability ability)
    {
        targetMode = !targetMode;

        if (!targetMode)
        {
            targetObject.transform.GetChild(0).gameObject.SetActive(false);
            targetObject.SetActive(false);
            activeAbility = null;
            GetComponent<Inventory>().SelectedConsumable = null;
            return;
        }

        activeAbility = ability;

        if (targetObject.transform.position != transform.position)
            targetObject.transform.position = transform.position;

        isSingleTarget = ability.Radius <= 1;
        targetObject.transform.GetChild(0).localScale = Vector3.one * (ability.Radius + 1);
        targetObject.transform.GetChild(0).gameObject.SetActive(!isSingleTarget);

        targetObject.SetActive(true);        
    }

    private void FixedUpdate()
    {
        if (!UIManager.instance.IsMenuOpen && !targetMode)
        {
            if (GameManager.instance.IsPlayerTurn && moveKeyDown && GetComponent<Actor>().IsAlive)
                Move();
        }
    }

    private void Move()
    {
        Vector2 direction = controls.Player.Movement.ReadValue<Vector2>();
        if (Keyboard.current.numpad7Key.isPressed) direction = new Vector2(-1, 1);
        else if (Keyboard.current.numpad9Key.isPressed) direction = new Vector2(1, 1);
        else if (Keyboard.current.numpad1Key.isPressed) direction = new Vector2(-1, -1);
        else if (Keyboard.current.numpad3Key.isPressed) direction = new Vector2(1, -1);
        Vector2 roundedDirection = new Vector2(Mathf.Round(direction.x), Mathf.Round(direction.y));
        Vector3 futurePosition;

        if (targetMode && activeAbility != null)
        {
            futurePosition = targetObject.transform.position + (Vector3)roundedDirection;

            Vector3 offset = futurePosition - transform.position;

            int dx = Mathf.RoundToInt(offset.x);
            int dy = Mathf.RoundToInt(offset.y);
            int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

            if (distance > activeAbility.Range)
            {
                float scale = (float)activeAbility.Range / distance;
                dx = Mathf.RoundToInt(dx * scale);
                dy = Mathf.RoundToInt(dy * scale);

            }
            futurePosition = transform.position + new Vector3(dx, dy, 0);

            Vector3Int targetGridPosition = MapManager.instance.FloorMap.WorldToCell(futurePosition);

            if (MapManager.instance.IsValidPosition(futurePosition) &&
                GetComponent<Actor>().FieldOfView.Contains(targetGridPosition))
            {
                targetObject.transform.position = futurePosition;
            }
        }
        else
        {
            futurePosition = transform.position + (Vector3)roundedDirection;
            moveKeyDown = Action.BumpAction(GetComponent<Actor>(), roundedDirection); //if we bump into an entity, movekeyheld is set to false

        }
    }

    private bool CanAct()
    {
        if (targetMode || UIManager.instance.IsMenuOpen || !GetComponent<Actor>().IsAlive)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private Actor SingleTargetChecks(Vector3 targetPosition)
    {
        Actor target = GameManager.instance.GetActorAtLocation(targetPosition);

        if (target == null)
        {
            UIManager.instance.AddMessage("You must select an enemy to target.", "#ffffff");
            return null;
        }

        if (target == GetComponent<Actor>())
        {
            UIManager.instance.AddMessage("You can't target yourself.", "#ffffff");
            return null;
        }

        return target;
    }

    private List<Actor> AreaTargetChecks(Vector3 targetPosition)
    {
        //take away 1 to account for center
        int radius = (int)targetObject.transform.GetChild(0).localScale.x - 1;

        Bounds targetBounds = new Bounds(targetPosition, Vector3.one * radius * 2);
        List<Actor> targets = new List<Actor>();

        foreach (Actor target in GameManager.instance.Actors)
        {
            if (targetBounds.Contains(target.transform.position))
            {
                targets.Add(target);
            }
        }

        if (targets.Count == 0)
        {
            UIManager.instance.AddMessage("There are no targets in the radius.", "#ffffff");
            return null;
        }

        return targets;
    }

}

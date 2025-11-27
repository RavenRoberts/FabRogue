using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

static public class Action
{
    static public void PickupAction(Actor actor)
    {
        for (int i = 0; i < GameManager.instance.Entities.Count; i++)
        {
            if (GameManager.instance.Entities[i].GetComponent<Actor>() || actor.transform.position != GameManager.instance.Entities[i].transform.position)
            {
                continue;
            }

            if (actor.Inventory.Items.Count >= actor.Inventory.Capacity)
            {
                UIManager.instance.AddMessage($"Your inventory is full.", "#808080");
                return;
            }

            Item item = GameManager.instance.Entities[i].GetComponent<Item>();
            actor.Inventory.Add(item);
            UIManager.instance.AddMessage($"You picked up the {item.name}.", "#ffffff");

            GameManager.instance.EndTurn();
        }
    }

    static public void TakeStairsAction(Actor actor)
    {
        Vector3Int pos = MapManager.instance.FloorMap.WorldToCell(actor.transform.position);


        TileBase tile =
            MapManager.instance.FloorMap.GetTile(pos) ??
            MapManager.instance.InteractableMap.GetTile(pos) ??
            MapManager.instance.ObstacleMap.GetTile(pos);

        if (tile == null)
        {
            UIManager.instance.AddMessage("There are no stairs here.", "#0da2ff");
            return;
        }

        string tileName = tile.name;
        
        if (tileName != MapManager.instance.UpStairsTile.name && tileName != MapManager.instance.DownStairsTile.name)
        {
            UIManager.instance.AddMessage("There are no stairs here.", "#0da2ff");
            return;         
        }

        if (SaveManager.instance.CurrentFloor == 1 && tileName == MapManager.instance.UpStairsTile.name)
        {
            UIManager.instance.AddMessage("A mysterious force prevents you from going back (its my tongue <3)", "#0da2ff"); //cute as this is probably just don't render stairs on first floor if possible
            return;
        }

        SaveManager.instance.SaveGame();
        SaveManager.instance.CurrentFloor += tileName == MapManager.instance.UpStairsTile.name ? -1 : 1;

        if (SaveManager.instance.Save.Scenes.Exists(x => x.FloorNumber == SaveManager.instance.CurrentFloor))
        {
            SaveManager.instance.LoadScene(false);
        }
        else
        {
            GameManager.instance.Reset(false);
            MapManager.instance.GenerateDungeon();
        }

        UIManager.instance.AddMessage("You take the stairs.", "#0da2ff");
        UIManager.instance.SetDungeonFloorText(SaveManager.instance.CurrentFloor);
    }

    static public void DropAction(Actor actor, Item item)
    {
        if (item.Equippable != null && actor.Equipment.GetEquippedItem(item.Equippable.EquipmentSlot) == item.Equippable)
        {
            actor.Equipment.ToggleEquip(item.Equippable);
        }

        actor.Inventory.Drop(item);

        UIManager.instance.ToggleDropMenu();
        GameManager.instance.EndTurn();
    }

    static public void UseAction(Actor consumer, Item item)
    {        
        bool itemUsed = false;

        if (item.Consumable is not null)
        {
            itemUsed = item.GetComponent<Consumable>().Activate(consumer);
        } 

        UIManager.instance.ToggleInventory();

        if (itemUsed)
        {
            return;
        }


        GameManager.instance.EndTurn();
    }

    static public bool BumpAction(Actor actor, Vector2 direction)
    {
        if (actor.Size.x > 1 || actor.Size.y > 1)
        {
            for (int i = 0; i < actor.OccupiedTiles.Length; i++)
            {
                Actor target = GameManager.instance.GetActorAtLocation(actor.OccupiedTiles[i] + (Vector3)direction);
                if (target != null && target != actor)
                {
                    MeleeAction(actor, target);
                    return false;
                }
            }
            MovementAction(actor, direction);
            return true;
        }
        else
        {
            Actor target = GameManager.instance.GetActorAtLocation(actor.transform.position + (Vector3)direction);
            
            if (target)
            {
                MeleeAction(actor, target);
                return false;
            }
            else
            {
                MovementAction(actor, direction);
                return true;
            }
        }       
    }

    static public void MeleeAction(Actor actor, Actor target)
    {
        DamageType type = DamageType.Physical;
        int baseDamage = actor.Power(type);

        Effects.ApplyDamage(target, baseDamage, StatType.Health, type);

        string attackDesc = $"{actor.name} attacks {target.name}";
        string colorHex = actor.GetComponent<Player>() ? "#ffffff" : "#d1a3a4";

        int flat = target.GetFlatDefense(type);
        float percent = target.GetPercentDefense(type);
        int afterFlat = Mathf.Max(0, baseDamage - flat);
        int damage = Mathf.RoundToInt(afterFlat * (1f - percent / 100f));

        if (damage > 0)
        {
            UIManager.instance.AddMessage($"{attackDesc} for {damage} hit points.", colorHex);
        }
        else
        {
            UIManager.instance.AddMessage($"{attackDesc} but does no damage", colorHex);
        }
        GameManager.instance.EndTurn();
    }

    static public void MovementAction(Actor actor, Vector2 direction)
    {
        actor.Move(direction);
        actor.UpdateFieldOfView();
        GameManager.instance.EndTurn();
    }

    static public void WaitAction()
    {
        GameManager.instance.EndTurn();
    }
    static public void CastAbilityAction(Actor caster, Vector3 targetPosition, Ability ability)
    {
        bool success = ability.Cast(caster, targetPosition);

        if (success)
        {
            caster.GetComponent<Player>().ToggleTargetMode(false);            
            GameManager.instance.EndTurn();
        }
    }

    
    static public void CastAction(Actor consumer, Actor target, Consumable consumable)
    {
        bool castSuccess = consumable.Cast(consumer, target);

        if (castSuccess)
        {
            GameManager.instance.EndTurn();
        }
    }

    static public void CastAction(Actor consumer, List<Actor> targets, Consumable consumable)
    {
        bool castSuccess = consumable.Cast(consumer, targets);

        if (castSuccess)
        {
            GameManager.instance.EndTurn();
        }
    }

    static public void EquipAction(Actor actor, Item item)
    {
        if (item.Equippable == null)
        {
            UIManager.instance.AddMessage($"The {item.name} cannot be equipped.", "#808080");
            return;
        }

        actor.Equipment.ToggleEquip(item.Equippable);

        UIManager.instance.ToggleInventory();
        GameManager.instance.EndTurn();
    }
}

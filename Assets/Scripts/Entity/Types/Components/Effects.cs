using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
    private static HashSet<Actor> deadActors = new HashSet<Actor>();

    public static void RestoreHealth(Actor target, int amount)
    {
        if (target.Hp == target.MaxHp)
        {
            amount = 0;
            return;
        };

        int newHPValue = target.Hp + amount;
        if (newHPValue > target.MaxHp)
        {
            newHPValue = target.MaxHp;
        }

        int amountRecovered = newHPValue - target.Hp;
        target.Hp = newHPValue;
    }

    public static void Restorestamina(Actor target, int amount)
    {
        if (target.Hp == target.MaxHp)
        {
            amount = 0;
            return;
        }
        ;

        int newHPValue = target.Hp + amount;
        if (newHPValue > target.MaxHp)
        {
            newHPValue = target.MaxHp;
        }

        int amountRecovered = newHPValue - target.Hp;
        target.Hp = newHPValue;
    }

    public static int ApplyDamage(Actor target, int amount, StatType stat, DamageType type)
    {
        int flat = target.GetFlatDefense(type);
        float percent = target.GetPercentDefense(type);

        int afterFlat = Mathf.Max(0, amount - flat);
        int finalDamage = Mathf.RoundToInt(afterFlat * (1f - percent / 100f));

        switch (stat)
        {
            case StatType.Health:
                target.Hp -= finalDamage;
                break;
            case StatType.Stamina:
                target.Stamina -= finalDamage;
                break;
        }
        return finalDamage;
    }



    public static void Swallow(Actor caster, Entity target)
    {
        DigestiveTract gut = caster.GetComponent<DigestiveTract>();
        GameManager.instance.RemoveFromPlay(target);
        gut.AddToStomach(target);
    }

    public static void Die(Actor actor, DeathCause cause)
    {
        if (deadActors.Contains(actor)) return;

        actor.IsAlive = false;
        deadActors.Add(actor);

        string subject = actor.GetComponent<Player>() ? "You" : actor.name;
        string causeText;

        if (actor.GetComponent<Player>())
        {
            causeText = cause switch
            {
                DeathCause.Combat => "perish from your injuries.",
                DeathCause.Digestion => "are digested alive.",
                _ => "die."
            };
            UIManager.instance.AddMessage($"{subject} {causeText}", "#ff0000");
        }
        else
        {
            causeText = cause switch
            {
                DeathCause.Combat => "falls down, dead.",
                DeathCause.Digestion => "lets out one last gurgle as they're digested alive.",
                _ => "dies."
            };
            GameManager.instance.Actors[0].GetComponent<Level>().AddExperience(actor.GetComponent<Level>().XpGiven); // give xp to player
            UIManager.instance.AddMessage($"{subject} {causeText}", "#ffa500");
        }

        bool inGut = false;
        foreach (var belly in GameObject.FindObjectsOfType<DigestiveTract>())
        {
            if (belly.DigestiveTractContents.Contains(actor))
            {
                inGut = true;
                break;
            }
        }

        if (!inGut)
        {
            HandleRemains(actor, RemainsType.Corpse);
        }

    }

    public static void HandleRemains(Actor actor, RemainsType type)
    {
        if (!deadActors.Contains(actor)) return;

        if (actor.AI != null) MonoBehaviour.Destroy(actor.AI);
        if (!actor.GetComponent<Player>()) GameManager.instance.RemoveActor(actor);

        actor.BlocksMovement = false;

        SpriteRenderer sprite = actor.SpriteRenderer;
        Color color;

        switch (type)
        {
            case RemainsType.Corpse:
                sprite.sprite = GameManager.instance.DeadSprite;
                sprite.color = new Color(191, 0, 0, 1);
                sprite.sortingOrder = 0;
                actor.name = $"Remains of {actor.name}";
                break;
            case RemainsType.Scat:
                sprite.sprite = GameManager.instance.DeadSprite;
                ColorUtility.TryParseHtmlString("#A34C12", out color);
                sprite.color = color;
                sprite.sortingOrder = 0;
                actor.name = $"Pile of {actor.name}";
                break;
        }
    }

    /*
    public static void Die(Actor actor)
    {
        if (actor.IsAlive)
        {
            if (actor.GetComponent<Player>())
            {
                UIManager.instance.AddMessage($"You died", "#ff0000");
            }
            else
            {
                GameManager.instance.Actors[0].GetComponent<Level>().AddExperience(actor.GetComponent<Level>().XpGiven); // give xp to player
                UIManager.instance.AddMessage($"{actor.name} is dead", "#ffa500");
            }
            actor.IsAlive = false;
        }

        SpriteRenderer spriteRenderer = actor.SpriteRenderer;
        spriteRenderer.sprite = GameManager.instance.DeadSprite;
        spriteRenderer.color = new Color(191, 0, 0, 1);
        spriteRenderer.sortingOrder = 0;

        actor.name = $"Remains of {actor.name}";
        actor.BlocksMovement = false;
        if (!actor.GetComponent<Player>())
        {
            GameManager.instance.RemoveActor(actor);
        }
    }*/

}


public enum StatType
{
    Health,
    Stamina,
    Integrity
}

public enum DamageType
{
    Physical,
    Acid,

    Corpus,
    Astral
}

public enum DeathCause
{
    Combat,
    Digestion
}

public enum RemainsType
{
    Corpse,
    Scat
}
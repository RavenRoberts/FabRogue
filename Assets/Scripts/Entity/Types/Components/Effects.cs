using UnityEngine;

public class Effects : MonoBehaviour
{
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

    public static void ApplyDamage(Actor target, int amount, StatType stat, DamageType type)
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
    }



    public static void Swallow(Actor caster, Entity target)
    {
        DigestiveTract gut = caster.GetComponent<DigestiveTract>();
        GameManager.instance.RemoveFromPlay(target);
        gut.AddToStomach(target);
    }


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
    }

}


public enum StatType
{
    Health,
    Stamina
}

public enum DamageType
{
    Physical,
    Acid,

    Corpus,
    Astral
}
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class RoyalMark : MonoBehaviour
{
    public HealthManager AttachedEnemy { get; set; }

    void Start()
    {
        On.HealthManager.Die += HealthManager_Die;
        if (GetComponent<SpriteRenderer>() is not SpriteRenderer renderer)
            renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 1;
        renderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.KingsBrand");
        CorrectPosition();
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self == AttachedEnemy)
        {
            if (CombatController.Enemies.Count == 0)
            {
                int rolled = RngProvider.GetStageRandom(1, 100);
                if (rolled <= 2)
                    TreasureManager.SpawnShiny(Enums.TreasureType.RareOrb, self.transform.position);
                else if (rolled <= 10)
                    TreasureManager.SpawnShiny(Enums.TreasureType.NormalOrb, self.transform.position);
                else if (rolled <= 35)
                    TreasureManager.SpawnShiny(Enums.TreasureType.PrismaticOrb, self.transform.position);
                else
                    HeroController.instance.AddGeo(100);
                HeroController.instance.AddGeo(100);
                
            }
            else
            {
                AttachedEnemy = CombatController.Enemies[Random.Range(0, CombatController.Enemies.Count)];
                CorrectPosition();
            }
        }
        else
            Destroy(gameObject);
        orig(self, attackDirection, attackType, ignoreEvasion);
    }

    private void CorrectPosition()
    {
        transform.SetParent(AttachedEnemy.transform);
        if (AttachedEnemy.GetComponent<BoxCollider2D>() is BoxCollider2D boxCollider)
            transform.localPosition = new(0f, boxCollider.size.y + 1f, -0.1f);
        else
            transform.localPosition = new(0f, 2f, -0.1f);
        transform.localRotation = Quaternion.identity;
    }
}

using UnityEngine;

namespace DungeonShooter
{
    public static class EntityStatsHelper
    {
        public static int CalculatePercentDamage(int casterAttack, int targetDefense, int percentInt, float damageRandomRatioFrom = 0.8f)
        {
            float percentFloat = (float)percentInt / 100;
            var percentDamage = (int)(casterAttack * percentFloat);
            var finalDamage = (int)(Mathf.Max(0, percentDamage - targetDefense) * Random.Range(damageRandomRatioFrom, 1.0f));
            return finalDamage;
        }
    }
}
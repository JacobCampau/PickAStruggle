using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    // Base stats
    float health = 100;
    float speed;
    float stamina_max = 100;
    float stamina_drain;
    float jump_force;
    float damage = 10;
    float handling_speed;
    float melee_range;

    // Boost stats
    float boost_health = 1;
    float boost_speed = 1;
    float boost_stamina_drain = 1;
    float boost_damage = 1;
    float boost_melee_range = 1;
    float boost_handling_speed = 1;

    // Totals
    float total_health;
    float total_speed;
    float total_stamina_drain;
    float total_damage;
    float total_melee_range;
    float total_handling_speed;

    // Resistance stats
    float slash_resist = 0;
    float bludgeon_resist = 0;

    // Emotion stats
    float emotion_max = 100;
    float emotion_buildup;
    
    public:
        /*================== Gets ==================*/
        // Total gets //CHANGE TO TOTALS
        float getHealth(){return health * boost_health;}
        float getSpeed(){return speed * boost_speed;}
        float getStaminaMax(){return stamina_max;}
        float getStaminaDrain(){return stamina_drain * boost_stamina_drain;}
        flaot getJumpForce(){return jump_force;}
        float getDamage(){return damage * boost_damage;}
        float getMeleeRange(){return melee_range * boost_melee_range;}
        float getHandlingSpeed(){return handling_speed * boost_handling_speed;}
        
        // Base gets
        float getHealth(){return health * boost_health;}
        float getSpeed(){return speed * boost_speed;}
        float getStaminaMax(){return stamina_max;}
        float getStaminaDrain(){return stamina_drain * boost_stamina_drain;}
        flaot getJumpForce(){return jump_force;}
        float getDamage(){return damage * boost_damage;}
        float getMeleeRange(){return melee_range * boost_melee_range;}
        float getHandlingSpeed(){return handling_speed * boost_handling_speed;}

        // Boost gets
        float getBoostHealth(){return boost_health;}
        float getBoostSpeed(){return boost_speed;}
        float getBoostStaminaDrain(){return boost_stamina_drain;}
        float getBoostDamage(){return boost_damage;}
        float getBoostMeleeRange(){return boost_melee_range;}
        float getBoostHandlingSpeed(){return boost_handling_speed;}

        // Resistance gets
        float getSlashResist(){return slash_resist;}
        float getBludgeonResist(){return bludgeon_resist;}

        // Emotion Gets
        float getEmotionMax(){return emotion_max;}
        float getEmotionBuildup(){return emotion_buidup;}

        /*================== Sets ==================*/
        // SET TOTALS
        // Base sets
        
}

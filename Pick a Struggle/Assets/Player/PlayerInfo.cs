using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    // Base stats
    float health = 100;
    float curr_health;
    float speed;
    float stamina_max = 100;
    float curr_stamina;
    float stamina_drain;
    float jump_force;
    float damage = 10;
    float handling_speed;
    float melee_range;

    // Emotion stats
    float emotion_max = 100;
    float curr_emotion;
    float emotion_buildup;

    // Boost stats
    float boost_health = 0;
    float boost_speed = 0;
    float boost_stamina_drain = 0;
    float boost_damage = 0;
    float boost_melee_range = 0;
    float boost_handling_speed = 0;
    float boost_emotion_buildup = 0;

    // Totals -> only affected via code
    float total_health;
    float total_speed;
    float total_stamina_drain;
    float total_damage;
    float total_melee_range;
    float total_handling_speed;
    float total_emotion_buildup;

    // Resistance stats
    float slash_resist = 0;
    float bludgeon_resist = 0;
    
    public:
        /*============== Initial Call ==============*/
        PlayerInfo(){}
        
        /*================== Gets ==================*/
        // Boosted gets
        float getHealth(){return total_health;}
        float getSpeed(){return total_speed;}
        float getStaminaDrain(){return total_stamina_drain;}
        float getDamage(){return total_damage;}
        float getMeleeRange(){return total_melee_range;}
        float getHandlingSpeed(){return total_handling_speed;}
        
        // Base gets (Not including the boosted stats)
        float getCurrentHealth(){return curr_health;}
        float getStaminaMax(){return stamina_max;}
        float getCurrentStamina(){return curr_stamina;}
        flaot getJumpForce(){return jump_force;}

        // Emotion gets
        float getEmotionMax(){return emotion_max;}
        float getCurrentEmotion(){return curr_emotion;}
        float getEmotionBuildup(){return total_emotion_buidup;}

        // Boost gets
        float getBoostHealth(){return boost_health;}
        float getBoostSpeed(){return boost_speed;}
        float getBoostStaminaDrain(){return boost_stamina_drain;}
        float getBoostDamage(){return boost_damage;}
        float getBoostMeleeRange(){return boost_melee_range;}
        float getBoostHandlingSpeed(){return boost_handling_speed;}
        float getBoostEmotionBuildup(){return boost_emotion_buildup;}

        // Resistance gets
        float getSlashResist(){return slash_resist;}
        float getBludgeonResist(){return bludgeon_resist;}

        /*================== Sets ==================*/
        // Total sets
        void setHealth(){total_health = health + boost_health;}
        void setSpeed(){total_speed = speed * boost_speed;}
        void setStaminaDrain(){total_stamina_drain = stamina_drain * boost_stamina_drain;}
        void setDamage(){total_damage = damage * boost_damage;}
        void setMeleeRange(){total_melee_range = melee_range * boost_melee_range;}
        void setHandlingSpeed(){total_handling_speed = handling_speed * boost_handling_speed;}
        
        // Emotional sets
        void setEmotionalBuildUp(){total_emotion_buildup = emotion_buildup + boost_emotion_buildup;}

        // Resistance sets
        void setSlashResist(float new_resist){slash_resist = new_resist;}
        void setBludgeonResist(float new_resist){bludgeon_resist = new_resist;}

        /*============ Custom Functions ============*/
        // Boost adding
        void addBoostHealth(float boost){
            boost_health += boost;
            setHealth();
        }

        void boostSpeed(float boost){boost_speed += boost;}
        
        void boostStaminaDrain(float boost){boost_stamina_drain += boost;}
        
        void boostDamage(float boost){boost_damage += boost;}
        
        void boostMeleeRange(float boost){boost_melee_range += boost;}
        
        void boostHandlingSpeed(float boost){boost_handling_speed += boost;}
        
        void boostEmotionBuildup(float boost){boot_emotion_buildup += boost;}

        // Other
        void dealDamage(float attack_dmg){
            if(curr_health > 0){curr_health -= attack_dmg;}
            if(curr_health < 0){curr_health = 0;}
        }

        void applyStaminaDrain(float time_slice){
            setStaminaDrain(); //ensure the drain is set properlly
            float affect = time_slice * total_stamina_drain; // get the effect
            curr_stamina -= affect; // apply the drain
        }

        void applyEmotionBuildup(float time_slice){
            setEmotionBuildup(); //ensure the buildup is set properlly
            float affect = time_slice * total_emotion_buildup; // get the effect
            curr_emotion += affect; // apply the buildup
        }
}
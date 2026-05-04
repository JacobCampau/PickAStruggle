using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    /*========== Info For All Players ==========*/
    // Base Stats
    float health;
    float curr_health;
    float speed;
    float stamina_max;
    float curr_stamina;
    float stamina_drain;
    float jump_force;
    float damage;
    float handling_speed;
    float melee_range;

    // Emotion Stats
    float emotion_max;
    float curr_emotion;
    float emotion_buildup;

    // Boost Stats
    float boost_health;
    float boost_speed;
    float boost_stamina_drain;
    float boost_damage;
    float boost_melee_range;
    float boost_handling_speed;
    float boost_emotion_buildup;

    // Totals -> only affected via code
    float total_health;
    float total_speed;
    float total_stamina_drain;
    float total_damage;
    float total_melee_range;
    float total_handling_speed;
    float total_emotion_buildup;

    // Resistance Stats
    float slash_resist;
    float bludgeon_resist;

    // Online Player Information
    int player_id;
    string player_name;
    Color player_color;

    // Player State Trackers
    bool is_grounded;
    bool is_stunned;
    bool is_dead;
    
    public:
        /*============== Initial Call ==============*/
        PlayerInfo(){
            // Set Basics
            health = 100;
            curr_health = getHealth();
            speed = 10;
            stamina_max = 100;
            curr_stamina = getStaminaMax();
            stamina_drain = 1;
            jump_force = 8;
            damage = 10;
            handling_speed = 1;
            melee_range = 1;
        
            // Set Emotions
            emotion_max = 100;
            curr_emotion = 0;
            emotion_buildup = 1;
        
            // Set Boosts
            boost_health = 0;
            boost_speed = 0;
            boost_stamina_drain = 0;
            boost_damage = 0;
            boost_melee_range = 0;
            boost_handling_speed = 0;
            boost_emotion_buildup = 0;
        
            // Set Totals
            setHealth();
            setSpeed();
            setStaminaDrain();
            setDamage();
            setMeleeRange();
            setHandlingSpeed();

            // Set Emotion
            setEmotionalBuildUp();

            // Set Resistances
            setSlashResist(0);
            setBludgeonResist(0);

            // Set Base Info
            int player_id = 0;
            string player_name = "NONE";
            Color player_color = Color.white;

            // Active State Info
            bool is_grounded = false;
            bool is_stunned = false;
            bool is_dead = false;
        }
        
        /*================== Gets ==================*/
        // Boosted Gets
        float getHealth(){
            setHealth();
            return total_health;
        }
        
        float getSpeed(){
            setSpeed();
            return total_speed;
        }
        
        float getStaminaDrain(){
            setStaminaDrain();
            return total_stamina_drain;
        }
        
        float getDamage(){
            setDamage();
            return total_damage;
        }
        
        float getMeleeRange(){
            setMeleeRange();
            return total_melee_range;
        }
        
        float getHandlingSpeed(){
            setHandlingSpeed();
            return total_handling_speed;
        }
        
        // Base Gets
        float getCurrentHealth(){return curr_health;}
        float getStaminaMax(){return stamina_max;}
        float getCurrentStamina(){return curr_stamina;}
        flaot getJumpForce(){return jump_force;}

        // Emotion Gets
        float getEmotionMax(){return emotion_max;}
        float getCurrentEmotion(){return curr_emotion;}
        float getEmotionBuildup(){return total_emotion_buidup;}

        // Boost Gets
        float getBoostHealth(){return boost_health;}
        float getBoostSpeed(){return boost_speed;}
        float getBoostStaminaDrain(){return boost_stamina_drain;}
        float getBoostDamage(){return boost_damage;}
        float getBoostMeleeRange(){return boost_melee_range;}
        float getBoostHandlingSpeed(){return boost_handling_speed;}
        float getBoostEmotionBuildup(){return boost_emotion_buildup;}

        // Resistance Gets
        float getSlashResist(){return slash_resist;}
        float getBludgeonResist(){return bludgeon_resist;}

        // Player Info Gets
        int getPlayerId(){return player_id;}
        string getPlayerName(){return player_name;}
        Color getPlayerColor(){return player_color;}

        // Player States Gets
        bool getIsGrounded(){return is_gounded;}
        bool getIsStunned(){return is_stunned;}
        bool getIsDead(){return is_dead;}

        /*================== Sets ==================*/
        // Total Sets
        void setHealth(){total_health = health + boost_health;}
        void setSpeed(){total_speed = speed * boost_speed;}
        void setStaminaDrain(){total_stamina_drain = stamina_drain * boost_stamina_drain;}
        void setDamage(){total_damage = damage * boost_damage;}
        void setMeleeRange(){total_melee_range = melee_range * boost_melee_range;}
        void setHandlingSpeed(){total_handling_speed = handling_speed * boost_handling_speed;}
        
        // Emotional Sets
        void setEmotionalBuildUp(){total_emotion_buildup = emotion_buildup + boost_emotion_buildup;}

        // Resistance Sets
        void setSlashResist(float new_resist){slash_resist = new_resist;}
        void setBludgeonResist(float new_resist){bludgeon_resist = new_resist;}

        // Player Info Sets
        void setPlayerId(int new_id){player_id = new_id;}
        void setPlayerName(string new_name){player_name = new_name;}
        void setPlayerColor(Color new_color){player_color = new_color;}

        // Player States Sets
        void setIsGrounded(bool new_truth){is_gounded = new_truth;}
        void setIsStunned(bool new_truth){is_stunned = new_truth;}
        void setIsDead(bool new_truth){is_dead = new_truth;}

        /*============ Custom Functions ============*/
        // Boost Adding
        void addBoostHealth(float boost){
            boost_health += boost;
            setHealth();
        }

        void addBoostSpeed(float boost){
            boost_speed += boost;
            setSpeed();
        }
        
        void addBoostStaminaDrain(float boost){
            boost_stamina_drain += boost;
            setStaminaDrain();
        }
        
        void addBoostDamage(float boost){
            boost_damage += boost;
            setDamage();
        }
        
        void addBoostMeleeRange(float boost){
            boost_melee_range += boost;
            setMeleeRange();
        }
        
        void addBoostHandlingSpeed(float boost){
            boost_handling_speed += boost;
            setHandlingSpeed();
        }
        
        void addBoostEmotionBuildup(float boost){
            boot_emotion_buildup += boost;
            setEmotionBuildup();
        }

        // More Complex Player Info Functions
        void dealDamage(float attack_dmg){
            if(curr_health > 0)
                curr_health -= attack_dmg;
            if(curr_health < 0)
                setIsDead(true);
                curr_health = 0;
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

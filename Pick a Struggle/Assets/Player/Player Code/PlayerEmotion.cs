using UnityEngine;

public class PlayerEmotion : MonoBehaviour
{
    public PlayerStats stats;

    // Emotion stats
    private float emotionMax;
    private float currEmotion;
    private float emotionBuildup;
    
    private float boostEmotionBuildup;
    
    private float totalEmotionBuildup;

    private void Start(){
        // Stat sets
        emotionMax = stats.emotionMax;
        currEmotion = 0;
        emotionBuildup = stats.emotionBuildup;
        
        // Initial set
        SetTotalEmotionBuildup();

        // Other set-up calls
    }

    private void Update(){
        currEmotion += totalEmotionBuildup * Time.deltaTime;
        if(currEmotion >= emotionMax){
            // Call the virtual emotion affect
            currEmotion = 0;
            EmotionAffect();
        }
    }

    // Virtual emotion function
    public virtual void EmotionAffect(){
        Debug.Log("EMOTION AFFECT NOT ASSIGNED");
    }

    // Setters
    void SetTotalEmotionBuildup(){ totalEmotionBuildup = emotionBuildup + boostEmotionBuildup; }
}

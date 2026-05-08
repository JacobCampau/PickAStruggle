using UnityEngine;

public class PlayerEmotion : MonoBehaviour
{
    // Emotion stats
    private float emotionMax;
    private float currEmotion;
    private float emotionBuildup;
    
    private float boostEmotionBuildup;
    
    private float totalEmotionBuildup;

    private void Start(){
        // Initial set
        SetTotalEmotionBuildup();

        // Other set-up calls
    }

    private void Update(){
    
    }

    // Setters
    void SetTotalEmotionBuildup(){ totalEmotionBuildup = emotionBuildup + boostEmotionBuildup; }
}

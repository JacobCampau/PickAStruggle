using UnityEngine;

public class AnimatorToController : MonoBehaviour
{
    PlayerAnimator _animator;

    private void Awake()
    {
        _animator = transform.GetComponentInParent<PlayerAnimator>();
    }

    public void ActivateIK()
    {
        _animator.SetIk(1);
    }

    public void DeActivateIK()
    {
        _animator.SetIk(0);
    }
}

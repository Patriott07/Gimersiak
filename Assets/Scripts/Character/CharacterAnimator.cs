using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    Animator animator;
    public static CharacterAnimator Instance;
    enum stateCharacter
    {
        idle, hit
    }
    void Start()
    {
        Instance = this;
        animator = gameObject.GetComponent<Animator>();
    }

    public void SetAnimationToIdle()
    {
        animator.Play(stateCharacter.idle.ToString(), 0, 0f);
    }

    public void SetAnimationToHit()
    {
        animator.Play(stateCharacter.hit.ToString(), 0, 0f);
    }


}

using UnityEngine;

public class EndAttack : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Attacking", false);
        animator.SetInteger("AttackType", -1);
    }
}

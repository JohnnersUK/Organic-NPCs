using UnityEngine;

public class FadeTime : StateMachineBehaviour
{
    public float fadeTime;
    public PlayerController player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        animator.SetFloat("combatTimer", fadeTime);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float tempTime = animator.GetFloat("combatTimer") - Time.deltaTime;
        animator.SetFloat("combatTimer", tempTime);

        if (tempTime <= 0 && animator.GetBool("inCombat"))
        {
            player.inCombat = false;
        }
    }
}

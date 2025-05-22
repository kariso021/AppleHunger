using Unity.Netcode;
using UnityEngine;

public class AppleAnimatorController : NetworkBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [ClientRpc]
    public void PlaySpawnClientRpc()
    {
        animator.SetTrigger("Spawn");
    }

    [ClientRpc]
    public void PlayDespawnClientRpc()
    {
        animator.SetTrigger("Despawn");
    }
}

using UnityEngine;

public class ForcarArmaNaMao : MonoBehaviour
{
    public Animator animator;
    public Transform arma;          // o GameObject Armas

    void LateUpdate()
    {
        if (animator == null || arma == null) return;

        // Pega o osso real da mão direita que o Animator está usando
        Transform mao = animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (mao == null) return;

        arma.position = mao.position;
        arma.rotation = mao.rotation;
    }
}
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    private Transform target;

    [Header("Correção de rotação (ajuste se necessário)")]
    public Vector3 rotationOffset = new Vector3(90, 0, 0);

    void Start()
    {
        ProcurarMissao();
    }

    void Update()
    {
        // Se não tem missão ou a missão foi destruída, procura outra
        if (target == null)
        {
            ProcurarMissao();

            // Se ainda não encontrou nenhuma, a seta some
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Direção só no plano horizontal
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
        }
    }

    void ProcurarMissao()
    {
        GameObject missao = GameObject.FindGameObjectWithTag("missao");

        if (missao != null)
        {
            target = missao.transform;
        }
        else
        {
            target = null;
        }
    }
}
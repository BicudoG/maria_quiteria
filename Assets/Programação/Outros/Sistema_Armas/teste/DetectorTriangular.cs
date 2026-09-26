using UnityEngine;

public class DetectorTriangular : MonoBehaviour
{
    public float alcance = 5f;       // Distância máxima do triângulo
    public float anguloVisao = 45f;  // Ângulo total de abertura da ponta

    void Update()
    {
        // 1. Detecta todos os colisores dentro do raio de alcance
        Collider[] alvosNoRaio = Physics.OverlapSphere(transform.position, alcance);

        foreach (var colisor in alvosNoRaio)
        {
            // Ignora o próprio objeto
            if (colisor.gameObject == gameObject) continue;

            // 2. Calcula a direção em direção ao alvo
            Vector3 direcaoParaAlvo = (colisor.transform.position - transform.position).normalized;

            // 3. Verifica se a direção está dentro do ângulo frontal do objeto
            float angulo = Vector3.Angle(transform.forward, direcaoParaAlvo);

            if (angulo < anguloVisao / 2f)
            {
                // O objeto está dentro do triângulo!
                Debug.Log($"Detectado: {colisor.name}");
            }
        }
    }

    // Desenha o triângulo no editor para visualização
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcance);

        Vector3 limiteEsquerdo = Quaternion.AngleAxis(-anguloVisao / 2f, transform.up) * transform.forward;
        Vector3 limiteDireito = Quaternion.AngleAxis(anguloVisao / 2f, transform.up) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, limiteEsquerdo * alcance);
        Gizmos.DrawRay(transform.position, limiteDireito * alcance);
    }

}

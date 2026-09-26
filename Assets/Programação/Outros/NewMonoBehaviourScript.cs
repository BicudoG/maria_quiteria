using UnityEngine;

public class cu : MonoBehaviour
{
    public enum TipoArma { Mosquete, Blunderbuss, Handmortar }

    [Header("Configuração Atual")]
    [SerializeField] private TipoArma armaEquipada = TipoArma.Mosquete;

    [Header("Parâmetros Gerais")]
    [SerializeField] private Transform pontoDeOrigem; // Substitui o 'fonte'
    [SerializeField] private LayerMask mascaraAlvos;

    [Header("Configurações: Blunderbuss")]
    [SerializeField] private float alcanceBlunderbuss = 5f;
    [SerializeField] private float anguloVisaoBlunderbuss = 45f;
    [SerializeField] private float alturaMaximaBlunderbuss = 2f;
    [SerializeField] private int danoBlunderbuss = 35;

    [Header("Configurações: Mosquete")]
    [SerializeField] private float alcanceMosquete = 15f;
    [SerializeField] private int danoMosquete = 20;

    [Header("Configurações: Handmortar")]
    [SerializeField] private GameObject prefabBomba;
    [SerializeField] private float forcaArremesso = 10f;

    /// <summary>
    /// Método público para ser chamado tanto pelo script de Input do Player quanto pela IA do NPC.
    /// </summary>
    public void Atirar()
    {
        switch (armaEquipada)
        {
            case TipoArma.Mosquete:
                DispararMosquete();
                break;
            case TipoArma.Blunderbuss:
                DispararBlunderbuss();
                break;
            case TipoArma.Handmortar:
                DispararHandmortar();
                break;
        }
    }

    /// <summary>
    /// Permite trocar de arma via código (ex: inventário ou troca de estado da IA)
    /// </summary>
    public void EquiparArma(TipoArma novaArma)
    {
        armaEquipada = novaArma;
    }

    private void DispararMosquete()
    {
        if (pontoDeOrigem == null) return;

        Vector3 origem = pontoDeOrigem.position;
        Vector3 direcao = pontoDeOrigem.forward;

        if (Physics.Raycast(origem, direcao, out RaycastHit hit, alcanceMosquete))
        {
            // Evita causar dano em si mesmo
            if (hit.collider.gameObject == gameObject) return;

            AplicarDano(hit.collider.gameObject, danoMosquete);
        }

        Debug.DrawRay(origem, direcao * alcanceMosquete, Color.red, 1f);
    }

    private void DispararBlunderbuss()
    {
        if (pontoDeOrigem == null) return;

        // Pega todos os alvos potenciais na esfera
        Collider[] alvosNoRaio = mascaraAlvos != 0
            ? Physics.OverlapSphere(pontoDeOrigem.position, alcanceBlunderbuss, mascaraAlvos)
            : Physics.OverlapSphere(pontoDeOrigem.position, alcanceBlunderbuss);

        foreach (var colisor in alvosNoRaio)
        {
            // Ignora o próprio atirador
            if (colisor.gameObject == gameObject || colisor.transform == pontoDeOrigem) continue;

            Vector3 vetorDiferenca = colisor.transform.position - pontoDeOrigem.position;

            // 1. Filtro de Altura (Eixo Y)
            if (Mathf.Abs(vetorDiferenca.y) > alturaMaximaBlunderbuss) continue;

            // 2. Projeção Horizontal para o Cálculo do Triângulo/Cone em 3D
            Vector3 direcaoHorizontal = new Vector3(vetorDiferenca.x, 0f, vetorDiferenca.z).normalized;
            Vector3 frenteHorizontal = new Vector3(pontoDeOrigem.forward.x, 0f, pontoDeOrigem.forward.z).normalized;

            // 3. Checagem do Ângulo
            float angulo = Vector3.Angle(frenteHorizontal, direcaoHorizontal);

            if (angulo <= anguloVisaoBlunderbuss / 2f)
            {
                AplicarDano(colisor.gameObject, danoBlunderbuss);
            }
        }
    }

    private void DispararHandmortar()
    {
        if (pontoDeOrigem == null || prefabBomba == null) return;

        GameObject novaBomba = Instantiate(prefabBomba, pontoDeOrigem.position, pontoDeOrigem.rotation);
        Rigidbody rb = novaBomba.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direcaoForca = pontoDeOrigem.forward + Vector3.up * 0.5f;
            rb.AddForce(direcaoForca * forcaArremesso, ForceMode.Impulse);
        }
    }

    private void AplicarDano(GameObject alvo, int dano)
    {
        Sistema_Vida vida = alvo.GetComponent<Sistema_Vida>();
        if (vida != null)
        {
            vida.Vida -= dano;
            Debug.Log($"{gameObject.name} causou {dano} de dano em {alvo.name}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoDeOrigem == null) return;

        // Desenha a área do Blunderbuss no Inspector quando selecionado
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pontoDeOrigem.position, alcanceBlunderbuss);

        Vector3 limiteEsquerdo = Quaternion.AngleAxis(-anguloVisaoBlunderbuss / 2f, pontoDeOrigem.up) * pontoDeOrigem.forward;
        Vector3 limiteDireito = Quaternion.AngleAxis(anguloVisaoBlunderbuss / 2f, pontoDeOrigem.up) * pontoDeOrigem.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(pontoDeOrigem.position, limiteEsquerdo * alcanceBlunderbuss);
        Gizmos.DrawRay(pontoDeOrigem.position, limiteDireito * alcanceBlunderbuss);
    }
}
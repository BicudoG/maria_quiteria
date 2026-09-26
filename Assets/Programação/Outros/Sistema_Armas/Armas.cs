using UnityEngine;

public class Armas : MonoBehaviour
{
    [SerializeField] bool[] armaAtual;
    [SerializeField] bool npc;
    [SerializeField] float alcance;
    [SerializeField] float anguloVisao = 45f;
    [SerializeField] Transform fonte;
    [SerializeField] Transform[] fonteMosquete;
    [SerializeField] GameObject bomba;
    [SerializeField] Collider[] alvosNoRaio;

    // Variável que as IAs usam para saber se o tiro pode ou não ser efetuado
    [HideInInspector] public bool jaAtacou;
    public float forcaDoArremesso = 10f;

    private void Awake()
    {
        
        if (armaAtual == null || armaAtual.Length < 3)
        {
            armaAtual = new bool[3];
        }

        if (npc)
        {
            
            int indiceAleatorio = Random.Range(0, armaAtual.Length);
            DefinirArmaAtiva(indiceAleatorio);
        }
        else
        {

            if (!armaAtual[0] && !armaAtual[1] && !armaAtual[2])
            {
                DefinirArmaAtiva(0);
            }
        }
    }
    private void DefinirArmaAtiva(int indiceParaAtivar)
    {
        for (int i = 0; i < armaAtual.Length; i++)
        {
            armaAtual[i] = (i == indiceParaAtivar);
        }
    }


    private void Update()
    {
        // LOGICA DO JOGADOR
        if (!npc)
        {
            // Só permite clicar se não tiver atacado ou se o tempo de espera acabou
            if (Input.GetMouseButtonDown(0) && !jaAtacou)
            {
                jaAtacou = true; // Trava o clique do jogador temporariamente
                Atacar();

                // Define o tempo de espera do jogador antes do próximo clique (Ex: 1.5 segundos)
                // Você pode substituir 1.5f por uma variável pública se preferir
                Invoke(nameof(ResetDisparoJogador), 1.5f);
            }
        }
        // LOGICA DO NPC
        else if (npc && !jaAtacou)
        {
            jaAtacou = true; // Impede o loop infinito do NPC no próximo frame
            Atacar();
        }
    }

    // Nova função para limpar a trava do jogador
    private void ResetDisparoJogador()
    {
        jaAtacou = false;
    }

    private void Atacar()
    {
        if (armaAtual == null || armaAtual.Length < 3) return;

        if (armaAtual[0] && !armaAtual[1] && !armaAtual[2]) Mosquete();
        if (!armaAtual[0] && armaAtual[1] && !armaAtual[2]) Blunderbuss();
        if (!armaAtual[0] && !armaAtual[1] && armaAtual[2]) handmortar();
    }

    void Mosquete()
    {
        alcance = 15f;
        if (fonteMosquete == null || fonteMosquete.Length == 0) return;

        int indiceAleatorio = Random.Range(0, fonteMosquete.Length);
        Transform puntoAtual = fonteMosquete[indiceAleatorio];

        RaycastHit hit;
        if (Physics.Raycast(puntoAtual.position, puntoAtual.forward, out hit, alcance))
        {
            Sistema_Vida vidaAlvo = hit.transform.GetComponent<Sistema_Vida>();
            if (vidaAlvo != null)
            {
                if (hit.collider.CompareTag("Inimigo") || hit.collider.CompareTag("NPC_Aliado"))
                {
                    vidaAlvo.Vida -= 20;
                }
                else if (gameObject.CompareTag("Inimigo") && hit.collider.CompareTag("Maria_incompleta"))
                {
                    vidaAlvo.Vida -= 20;
                }
            }
        }
        Debug.DrawRay(puntoAtual.position, puntoAtual.forward * alcance, Color.red, 1f);
    }
    void Blunderbuss()
    {
        alcance = 5f;
        if (fonte == null) return;

        alvosNoRaio = Physics.OverlapSphere(fonte.position, alcance);
        foreach (var colisor in alvosNoRaio)
        {
            if (colisor.gameObject == gameObject || colisor.transform == fonte) continue;

            Vector3 direcaoParaAlvo = (colisor.transform.position - fonte.position).normalized;
            float angulo = Vector3.Angle(fonte.forward, direcaoParaAlvo);

            if (angulo < anguloVisao / 2f)
            {
                Sistema_Vida vidaAlvo = colisor.GetComponent<Sistema_Vida>();
                if (vidaAlvo != null)
                {
                    if (!npc || gameObject.CompareTag("NPC_Aliado"))
                    {
                        if (colisor.CompareTag("Inimigo") || colisor.CompareTag("Maria_incompleta"))
                        {
                            vidaAlvo.Vida -= 20;
                        }
                    }

                    else if (gameObject.CompareTag("Inimigo"))
                    {
                        if (colisor.CompareTag("Player") || colisor.CompareTag("NPC_Aliado"))
                        {
                            vidaAlvo.Vida -= 20;
                        }
                    }
                }
            }
        }
    }


    void handmortar()
    {
        if (fonteMosquete == null || fonteMosquete.Length == 0 || bomba == null) return;

        GameObject novaBomba = Instantiate(bomba, fonteMosquete[0].position, fonteMosquete[0].rotation);
        Rigidbody rb = novaBomba.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direcaoForca = fonteMosquete[0].forward + Vector3.up * 0.5f;
            rb.AddForce(direcaoForca * forcaDoArremesso, ForceMode.Impulse);
        }
    }

    void OnDrawGizmos()
    {
        if (fonte == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fonte.position, alcance);
    }
}

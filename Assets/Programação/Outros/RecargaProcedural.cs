using System.Collections;
using UnityEngine;

public class RecargaProceduralIK : MonoBehaviour
{
    [Header("Componentes do Personagem")]
    public Transform maoEsquerda;        // Osso hand.L
    public Transform mosquete;           // Objeto do mosquete
    public Transform alvoBocaDoCano;     // Ponto vazio na ponta do cano

    [Header("Configurações da Arma na Recarga")]
    public Vector3 posicaoRecargaArma = new Vector3(0.1f, -0.3f, 0.2f);
    public Vector3 rotacaoRecargaArma = new Vector3(45f, 0f, 0f);

    [Header("Debug")]
    public bool mostrarLogs = true;

    private Vector3 posicaoOriginalArma;
    private Quaternion rotacaoOriginalArma;
    private bool estaRecarregando = false;

    // Variáveis usadas no LateUpdate para forçar a mão
    private bool forcarMao = false;
    private Vector3 posicaoMaoDesejada;
    private Quaternion rotacaoMaoDesejada;
    private bool usarLocalMao = false;

    void Start()
    {
        if (mosquete != null)
        {
            posicaoOriginalArma = mosquete.localPosition;
            rotacaoOriginalArma = mosquete.localRotation;
        }
        else
        {
            Debug.LogError("[Recarga] Mosquete não foi atribuído!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !estaRecarregando)
        {
            if (mosquete == null || alvoBocaDoCano == null || maoEsquerda == null)
            {
                Debug.LogError("[Recarga] Alguma referência está faltando no Inspector!");
                return;
            }

            StartCoroutine(RotinaRecargaManual());
        }
    }

    // Roda DEPOIS do Animator → consegue sobrescrever a mão
    void LateUpdate()
    {
        if (forcarMao && maoEsquerda != null)
        {
            if (usarLocalMao)
            {
                maoEsquerda.localPosition = posicaoMaoDesejada;
                maoEsquerda.localRotation = rotacaoMaoDesejada;
            }
            else
            {
                maoEsquerda.position = posicaoMaoDesejada;
                // Mantém a rotação atual ou define se quiser
            }
        }
    }

    IEnumerator RotinaRecargaManual()
    {
        estaRecarregando = true;
        if (mostrarLogs) Debug.Log("[Recarga] Iniciando...");

        float duracao = 0.5f;
        float cronometro;

        // Guarda a posição original da mão
        Vector3 posicaoOriginalMaoLocal = maoEsquerda.localPosition;
        Quaternion rotacaoOriginalMao = maoEsquerda.localRotation;
        Vector3 posicaoOriginalMaoWorld = maoEsquerda.position;

        // ========== PASSO 1: Traz a arma para perto do peito ==========
        cronometro = 0f;
        while (cronometro < duracao)
        {
            cronometro += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, cronometro / duracao);

            mosquete.localPosition = Vector3.Lerp(posicaoOriginalArma, posicaoRecargaArma, t);
            mosquete.localRotation = Quaternion.Lerp(rotacaoOriginalArma, Quaternion.Euler(rotacaoRecargaArma), t);

            yield return null;
        }

        mosquete.localPosition = posicaoRecargaArma;
        mosquete.localRotation = Quaternion.Euler(rotacaoRecargaArma);

        yield return new WaitForSeconds(0.15f);

        // ========== PASSO 2: Move a mão até a boca do cano ==========
        forcarMao = true;
        usarLocalMao = false; // vamos trabalhar em world space

        cronometro = 0f;
        Vector3 inicioMao = maoEsquerda.position;

        while (cronometro < duracao)
        {
            cronometro += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, cronometro / duracao);

            posicaoMaoDesejada = Vector3.Lerp(inicioMao, alvoBocaDoCano.position, t);

            yield return null;
        }

        // Mantém a mão na boca do cano
        posicaoMaoDesejada = alvoBocaDoCano.position;

        // Tempo "empurrando a bala"
        yield return new WaitForSeconds(0.6f);

        // ========== PASSO 3: Retorna tudo ==========
        cronometro = 0f;
        Vector3 posicaoMaoAtual = posicaoMaoDesejada;

        while (cronometro < duracao)
        {
            cronometro += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, cronometro / duracao);

            // Arma volta
            mosquete.localPosition = Vector3.Lerp(posicaoRecargaArma, posicaoOriginalArma, t);
            mosquete.localRotation = Quaternion.Lerp(Quaternion.Euler(rotacaoRecargaArma), rotacaoOriginalArma, t);

            // Mão volta (ainda forçando no LateUpdate)
            posicaoMaoDesejada = Vector3.Lerp(posicaoMaoAtual, posicaoOriginalMaoWorld, t);

            yield return null;
        }

        // Alinhamento final
        mosquete.localPosition = posicaoOriginalArma;
        mosquete.localRotation = rotacaoOriginalArma;

        // Agora força a posição LOCAL original da mão
        usarLocalMao = true;
        posicaoMaoDesejada = posicaoOriginalMaoLocal;
        rotacaoMaoDesejada = rotacaoOriginalMao;

        // Espera um frame para o LateUpdate aplicar
        yield return null;

        // Para de forçar a mão
        forcarMao = false;

        estaRecarregando = false;
        if (mostrarLogs) Debug.Log("[Recarga] Finalizada.");
    }
}
using UnityEngine;
using System.Collections;

public class fade : MonoBehaviour
{
    public Animator anima;
    public ProximaFase pro;
    public GameObject[] images;

    int imageAtual = 0;   // começa na primeira imagem

    public void chamaFeidparafase()
    {
        StartCoroutine(Load());
    }

    public void paginas()
    {
        // Só avança se ainda houver imagem
        if (imageAtual >= images.Length - 1)
        {
            Debug.Log("Não há mais páginas");
            return;
        }

        StartCoroutine(Loadproximo());
    }

    private IEnumerator Load()
    {
        yield return null;
        anima.SetTrigger("fade");
        yield return new WaitForSeconds(1.5f);
        pro.Fase();
    }

    private IEnumerator Loadproximo()
    {
        yield return null;
        anima.SetTrigger("fade");
        yield return new WaitForSeconds(1.5f);

        // Desativa a imagem atual (opcional, mas geralmente útil)
        if (imageAtual >= 0 && imageAtual < images.Length)
            images[imageAtual].SetActive(false);

        imageAtual++;   // agora sim, só incrementa DEPOIS do fade

        // Ativa a próxima
        if (imageAtual < images.Length)
            images[imageAtual].SetActive(true);

        anima.SetTrigger("fade");   // fade de entrada
    }
}
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;

public class AlembicLooper : MonoBehaviour
{
    public AlembicStreamPlayer streamPlayer;

    [Header("Configurações do Loop")]
    public float startTime = 3f;   // Começa no 3
    public float endTime = 10f;    // Volta quando chegar no 10
    public float speed = 1f;

    private float currentTime;

    void Start()
    {
        currentTime = startTime;
        if (streamPlayer != null)
            streamPlayer.UpdateImmediately(currentTime);
    }

    void Update()
    {
        if (streamPlayer == null) return;

        // Avança o tempo
        currentTime += Time.deltaTime * speed;

        // Quando passar do final, volta pro início imediatamente
        if (currentTime >= endTime)
        {
            currentTime = startTime;
        }

        // Força a atualização
        streamPlayer.UpdateImmediately(currentTime);
    }
}
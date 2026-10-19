using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    // O padrão "Singleton" permite que qualquer script do jogo acesse essas variáveis facilmente
    public static GameProgressManager Instance;

    [Header("Coletáveis / Habilidades Destravadas")]
    public bool hasFire;  // Destrava o Soco
    public bool hasWater; // Destrava o Tiro
    public bool hasEarth; // Destrava o Chicote
    public bool hasWind;  // Destrava o Planador

    [Header("Fim de Jogo")]
    public bool hasFinalArtifact; // Coletável do Templo

    private void Awake()
    {
        // Garante que só exista UM GameProgressManager no jogo inteiro
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // O PULO DO GATO: Este objeto não é destruído ao mudar de fase!
        }
        else
        {
            Destroy(gameObject); // Se já existir um, destrói o clone
        }
    }

    // Retorna quantos cristais elementais o jogador já pegou (de 0 a 4)
    public int GetCollectedCount()
    {
        int count = 0;
        if (hasFire) count++;
        if (hasWater) count++;
        if (hasEarth) count++;
        if (hasWind) count++;
        return count;
    }
}
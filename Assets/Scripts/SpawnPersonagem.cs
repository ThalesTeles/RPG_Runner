using UnityEngine;

public class SpawnPersonagem : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsPersonagens;
    [SerializeField] private Transform pontoSpawn;

    void Start()
    {
        int indice = PlayerPrefs.GetInt("PersonagemEscolhido", 0);
        GameObject jogador = Instantiate(prefabsPersonagens[indice], pontoSpawn.position, pontoSpawn.rotation);
        SkinService.Aplicar(jogador); // aplica a skin escolhida no menu (botao PERSONALIZAR)
    }
}
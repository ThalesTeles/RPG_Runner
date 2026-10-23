using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Teste de fumaca do botao PERSONALIZAR: Menu -> escolher skin -> Cena com o jogador usando a skin.
/// </summary>
public class PersonalizarSmokeTest
{
    private const string ChaveSkin = "SkinEscolhida";
    private const string ChavePersonagem = "PersonagemEscolhido";

    private string skinAntes;
    private int personagemAntes;

    [SetUp]
    public void SalvarPrefs()
    {
        skinAntes = PlayerPrefs.GetString(ChaveSkin, "original");
        personagemAntes = PlayerPrefs.GetInt(ChavePersonagem, 0);
        PlayerPrefs.SetString(ChaveSkin, "original");
    }

    [TearDown]
    public void RestaurarPrefs()
    {
        PlayerPrefs.SetString(ChaveSkin, skinAntes);
        PlayerPrefs.SetInt(ChavePersonagem, personagemAntes);
        PlayerPrefs.Save();
    }

    private static string[] TexturasDePersonagem()
    {
        return Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
            .SelectMany(r => r.sharedMaterials)
            .Where(m => m != null && m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null)
            .Select(m => m.GetTexture("_BaseMap").name)
            .Where(n => n.StartsWith("knight_") || n.StartsWith("mage_") || n.StartsWith("barbarian_"))
            .Distinct()
            .ToArray();
    }

    private static void Clicar(string nome)
    {
        GameObject go = GameObject.Find(nome);
        Assert.IsNotNull(go, nome + " nao encontrado");
        go.GetComponent<Button>().onClick.Invoke();
    }

    [UnityTest]
    public IEnumerator Menu_EscolherSkin_AplicaNoMenuENaCena()
    {
        yield return SceneManager.LoadSceneAsync("Menu");
        yield return null; // Start do SeletorSkinUI

        Assert.IsNotNull(GameObject.Find("SeletorSkinUI"), "SeletorSkinUI nao foi criado no Menu");
        Assert.IsNotNull(GameObject.Find("BotaoPersonalizar"), "botao PERSONALIZAR nao existe");
        Assert.IsNull(GameObject.Find("PainelSkins"), "painel deveria comecar fechado");

        Clicar("BotaoPersonalizar");
        Assert.IsNotNull(GameObject.Find("PainelSkins"), "painel nao abriu");

        Clicar("Cartao_abobora");
        Assert.AreEqual("abobora", PlayerPrefs.GetString(ChaveSkin));

        string[] noMenu = TexturasDePersonagem();
        Assert.IsNotEmpty(noMenu, "nenhum personagem com textura KayKit no Menu");
        Assert.That(noMenu, Has.All.EndsWith("_abobora"), "Menu: " + string.Join(", ", noMenu));

        Clicar("BotaoFechar");
        Assert.IsNull(GameObject.Find("PainelSkins"), "painel nao fechou");

        PlayerPrefs.SetInt(ChavePersonagem, 1); // mago
        yield return SceneManager.LoadSceneAsync("Cena");
        yield return null; // Start do SpawnPersonagem

        string[] naCena = TexturasDePersonagem();
        Assert.IsNotEmpty(naCena, "jogador nao foi instanciado na Cena");
        Assert.That(naCena, Has.All.EndsWith("_abobora"), "Cena: " + string.Join(", ", naCena));
        Assert.IsNull(GameObject.Find("SeletorSkinUI"), "SeletorSkinUI nao deveria existir na Cena");

        // voltar ao Menu (como vindo da tela Morte) recria a UI
        yield return SceneManager.LoadSceneAsync("Morte");
        yield return SceneManager.LoadSceneAsync("Menu");
        yield return null;
        Assert.IsNotNull(GameObject.Find("BotaoPersonalizar"), "UI nao recriada ao voltar ao Menu");
        Assert.AreEqual(1, Object.FindObjectsByType<Canvas>().Count(c => c.name == "CanvasSkins"));
    }
}

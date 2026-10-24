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

    private static Rect RetanguloNaTela(string nome)
    {
        GameObject go = GameObject.Find(nome);
        Assert.IsNotNull(go, nome + " nao encontrado");
        var rt = go.GetComponent<RectTransform>();
        Canvas raiz = rt.GetComponentInParent<Canvas>().rootCanvas;
        Camera cam = raiz.renderMode == RenderMode.ScreenSpaceOverlay ? null : raiz.worldCamera;
        var cantos = new Vector3[4];
        rt.GetWorldCorners(cantos);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, cantos[0]), max = min;
        for (int i = 1; i < 4; i++)
        {
            Vector2 p = RectTransformUtility.WorldToScreenPoint(cam, cantos[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    [UnityTest]
    public IEnumerator Menu_BotaoPersonalizarFicaAbaixoDoLivroSemSobrepor()
    {
        yield return SceneManager.LoadSceneAsync("Menu");
        yield return null;
        yield return null; // CanvasScaler e LateUpdate do SeletorSkinUI

        Rect botao = RetanguloNaTela("BotaoPersonalizar");
        Rect livro = RetanguloNaTela("botaoInstru");
        Rect jogar = RetanguloNaTela("botaoJogar");
        string info = $"tela {Screen.width}x{Screen.height} botao {botao} livro {livro} jogar {jogar}";

        Assert.LessOrEqual(botao.yMax, livro.yMin + 1f, "o botao deveria ficar abaixo do livro. " + info);
        Assert.AreEqual(livro.xMax, botao.xMax, 2f, "o botao deveria alinhar pela direita com o livro. " + info);
        Assert.AreEqual(jogar.height * 0.75f, botao.height, 2f, "altura proporcional ao JOGAR. " + info);
        Assert.GreaterOrEqual(botao.xMin, 0f, "o botao saiu da tela. " + info);
        Assert.GreaterOrEqual(botao.yMin, 0f, "o botao saiu da tela. " + info);
        foreach (string outro in new[] { "botaoJogar", "botaoInstru", "botaoMago", "botaoLutador", "botaoBarbaro" })
            Assert.IsFalse(botao.Overlaps(RetanguloNaTela(outro)), "o botao sobrepoe " + outro + ". " + info);
        Assert.IsNotNull(GameObject.Find("BotaoPersonalizar").GetComponent<UnityEngine.UI.Image>().sprite,
            "o botao deveria usar a arte de Resources/UI/botaoPersonalizar");
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

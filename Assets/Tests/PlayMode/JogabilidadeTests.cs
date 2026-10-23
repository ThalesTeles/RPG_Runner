using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Testes da logica de jogo (PASSOS 1 a 4 do Guia de Estudo): dano, invulnerabilidade,
/// game over e dificuldade progressiva. Na versao da aula eles falham ate o aluno completar os passos.
/// Os scripts do jogo ficam no Assembly-CSharp, entao o acesso e por reflexao.
/// </summary>
public class JogabilidadeTests
{
    private const BindingFlags Tudo = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private int personagemAntes;
    private int vidaExtraAntes;

    [SetUp]
    public void SalvarPrefs()
    {
        personagemAntes = PlayerPrefs.GetInt("PersonagemEscolhido", 0);
        vidaExtraAntes = PlayerPrefs.GetInt("VidaExtra", 0);
        PlayerPrefs.SetInt("PersonagemEscolhido", 1); // mago: sem escudo nem dash
        PlayerPrefs.SetInt("VidaExtra", 0);
    }

    [TearDown]
    public void RestaurarPrefs()
    {
        PlayerPrefs.SetInt("PersonagemEscolhido", personagemAntes);
        PlayerPrefs.SetInt("VidaExtra", vidaExtraAntes);
        PlayerPrefs.Save();
    }

    private static Component Achar(string tipo)
    {
        foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude))
        {
            if (mb.GetType().Name == tipo) return mb;
        }
        Assert.Fail(tipo + " nao encontrado na Cena");
        return null;
    }

    private static object Ler(object alvo, string membro)
    {
        FieldInfo f = alvo.GetType().GetField(membro, Tudo);
        if (f != null) return f.GetValue(alvo);
        return alvo.GetType().GetProperty(membro, Tudo).GetValue(alvo);
    }

    private static void Escrever(object alvo, string campo, object valor)
    {
        alvo.GetType().GetField(campo, Tudo).SetValue(alvo, valor);
    }

    private static void Bater(Component vida, string tag)
    {
        var obstaculo = new GameObject("ObstaculoTeste") { tag = tag };
        Collider col = obstaculo.AddComponent<BoxCollider>();
        col.isTrigger = true;
        vida.GetType().GetMethod("OnTriggerEnter", Tudo).Invoke(vida, new object[] { col });
        Object.Destroy(obstaculo);
    }

    private static IEnumerator CarregarCena()
    {
        yield return SceneManager.LoadSceneAsync("Cena");
        yield return null; // Start do SpawnPersonagem e do VidaPlayer
    }

    [UnityTest]
    public IEnumerator Passos1e2_ObstaculoTiraUmaVidaEDeixaInvulneravel()
    {
        yield return CarregarCena();
        Component vida = Achar("VidaPlayer");
        int max = (int)Ler(vida, "vidasMaximas");

        Bater(vida, "obstacle");
        Assert.AreEqual(max - 1, (int)Ler(vida, "vidasAtuais"), "bater num obstaculo deveria tirar 1 vida");
        Assert.IsTrue((bool)Ler(vida, "invulneravel"), "depois do dano o jogador deveria ficar invulneravel");

        Bater(vida, "enemy");
        Assert.AreEqual(max - 1, (int)Ler(vida, "vidasAtuais"), "invulneravel nao deveria levar dano");

        Escrever(vida, "invulneravel", false);
        Bater(vida, "enemy");
        Assert.AreEqual(max - 2, (int)Ler(vida, "vidasAtuais"), "bater num inimigo deveria tirar 1 vida");

        Escrever(vida, "invulneravel", false);
        Bater(vida, "Untagged");
        Assert.AreEqual(max - 2, (int)Ler(vida, "vidasAtuais"), "objeto sem tag perigosa nao deveria dar dano");
    }

    [UnityTest]
    public IEnumerator Passo3_SemVidasOJogoParaEVaiParaMorte()
    {
        yield return CarregarCena();
        Component vida = Achar("VidaPlayer");
        int max = (int)Ler(vida, "vidasMaximas");

        for (int i = 0; i < max; i++)
        {
            Escrever(vida, "invulneravel", false);
            Bater(vida, "obstacle");
        }

        Assert.IsTrue((bool)Ler(vida, "morreu"), "com 0 vidas o jogador deveria morrer");
        Assert.IsFalse(((Behaviour)vida.GetComponent("MovimentoPlayer")).enabled, "MovimentoPlayer deveria ser desligado");
        Assert.AreEqual(0f, (float)Ler(Achar("GameManager"), "VelocidadeAtual"), "o jogo deveria parar");

        float espera = (float)Ler(vida, "tempoAteMorte") + 1f;
        float fim = Time.realtimeSinceStartup + espera;
        while (SceneManager.GetActiveScene().name != "Morte" && Time.realtimeSinceStartup < fim)
            yield return null;
        Assert.AreEqual("Morte", SceneManager.GetActiveScene().name, "deveria ir para a tela de Game Over");
    }

    [UnityTest]
    public IEnumerator Passo4_DificuldadeAumentaACadaIntervalo()
    {
        yield return CarregarCena();
        Component gm = Achar("GameManager");
        float velocidade = (float)Ler(gm, "VelocidadeAtual");
        float chance = (float)Ler(gm, "ChanceSpawnAtual");

        Escrever(gm, "timerAumento", (float)Ler(gm, "intervaloAumento"));
        yield return null;

        Assert.AreEqual(velocidade + (float)Ler(gm, "incrementoVelocidade"), (float)Ler(gm, "VelocidadeAtual"), 0.001f,
            "a velocidade deveria aumentar a cada intervaloAumento");
        Assert.AreEqual(chance + (float)Ler(gm, "incrementoChance"), (float)Ler(gm, "ChanceSpawnAtual"), 0.001f,
            "a chance de spawn deveria aumentar a cada intervaloAumento");
        Assert.Less((float)Ler(gm, "timerAumento"), 0.5f, "o timer deveria ser zerado");
    }
}

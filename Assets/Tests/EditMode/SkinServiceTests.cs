using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Testes do SkinService. Os scripts do jogo ficam no Assembly-CSharp (sem asmdef),
/// que nao pode ser referenciado por um asmdef de teste, entao o acesso e por reflexao.
/// </summary>
public class SkinServiceTests
{
    private const string ChavePrefs = "SkinEscolhida";
    private const string MaterialMago =
        "Assets/Imports/personagens/KayKit/Characters/KayKit - Adventurers (for Unity)/Materials/mage.mat";

    private static readonly Type Servico = Type.GetType("SkinService, Assembly-CSharp", true);

    private bool tinhaSkinSalva;
    private string skinSalva;

    [SetUp]
    public void SalvarPrefs()
    {
        tinhaSkinSalva = PlayerPrefs.HasKey(ChavePrefs);
        skinSalva = PlayerPrefs.GetString(ChavePrefs);
        PlayerPrefs.DeleteKey(ChavePrefs);
    }

    [TearDown]
    public void RestaurarPrefs()
    {
        if (tinhaSkinSalva) PlayerPrefs.SetString(ChavePrefs, skinSalva);
        else PlayerPrefs.DeleteKey(ChavePrefs);
        PlayerPrefs.Save();
    }

    private static object Chamar(string metodo, params object[] args)
    {
        foreach (MethodInfo m in Servico.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (m.Name == metodo && m.GetParameters().Length == args.Length)
                return m.Invoke(null, args);
        }
        throw new MissingMethodException("SkinService", metodo);
    }

    private static string SkinAtual =>
        (string)Servico.GetProperty("SkinAtual", BindingFlags.Public | BindingFlags.Static).GetValue(null);

    [Test]
    public void ListarSkins_RetornaAsSeisSkinsComOriginalPrimeiro()
    {
        var skins = (List<string>)Chamar("ListarSkins");

        CollectionAssert.AreEquivalent(
            new[] { "original", "abobora", "bruxa", "fantasma", "vampiro", "zumbi" }, skins);
        Assert.AreEqual("original", skins[0]);
    }

    [Test]
    public void SkinAtual_SemEscolha_EOriginal()
    {
        Assert.AreEqual("original", SkinAtual);
    }

    [Test]
    public void Escolher_PersisteEmPlayerPrefs()
    {
        Chamar("Escolher", "vampiro");

        Assert.AreEqual("vampiro", SkinAtual);
        Assert.AreEqual("vampiro", PlayerPrefs.GetString(ChavePrefs));
    }

    [Test]
    public void Aplicar_TrocaTexturaDoMagoSemAlterarOAsset()
    {
        // renderer.materials em EditMode loga aviso de "leak" de material; aqui isso e esperado.
        LogAssert.ignoreFailingMessages = true;

        Material asset = AssetDatabase.LoadAssetAtPath<Material>(MaterialMago);
        Assert.IsNotNull(asset, "material do mago nao encontrado");
        Assert.AreEqual("mage_texture", asset.GetTexture("_BaseMap").name);

        var go = new GameObject("MagoTeste");
        try
        {
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = asset;

            Chamar("Aplicar", new Renderer[] { renderer }, "abobora");

            Assert.AreEqual("mage_abobora", renderer.sharedMaterial.GetTexture("_BaseMap").name);
            Assert.AreEqual("mage_abobora", renderer.sharedMaterial.mainTexture.name);
            Assert.AreNotSame(asset, renderer.sharedMaterial, "deveria usar uma copia do material");
            Assert.AreEqual("mage_texture", asset.GetTexture("_BaseMap").name, "o asset original foi alterado");

            // voltar para "original" restaura a textura padrao
            Chamar("Aplicar", new Renderer[] { renderer }, "original");
            Assert.AreEqual("mage_original", renderer.sharedMaterial.GetTexture("_BaseMap").name);
        }
        finally
        {
            Destruir(go);
        }
    }

    // Destroi o objeto e a copia de material criada pelo Aplicar (nunca o asset).
    private static void Destruir(GameObject go)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null && r.sharedMaterial != null && !AssetDatabase.Contains(r.sharedMaterial))
            UnityEngine.Object.DestroyImmediate(r.sharedMaterial);
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void Aplicar_IgnoraMateriaisSemTexturaDePersonagem()
    {
        var go = new GameObject("Cubo");
        try
        {
            var renderer = go.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial = mat;

            Chamar("Aplicar", new Renderer[] { renderer }, "abobora");

            Assert.AreSame(mat, renderer.sharedMaterial);
            UnityEngine.Object.DestroyImmediate(mat);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}

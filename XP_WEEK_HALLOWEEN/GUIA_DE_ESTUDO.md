# Guia de Estudo – XP Week Outubro (Especial Halloween)
**Projeto base:** RPG Runner (Unity) — Campeão GameJam · **Tecnologia:** Unity · **Responsável:** Thales

> Documento para os professores que vão aplicar a aula. O aluno assume o papel de **Desenvolvedor** (completa a lógica do jogo)
> e de **Diretor de Arte** (escolhe a skin de Halloween do personagem).

---

## 1. Como abrir o projeto

- Branch da aula: **`xp-week-halloween`** (jogo "quebrado" para os alunos).
- Branch do gabarito: **`xp-week-solucao`** (jogo completo, **não** entregar aos alunos).
- Versão do Unity: **6000.5.1f1** (a mesma de `ProjectSettings/ProjectVersion.txt`).
- Abra a cena `Assets/Scenes/Menu.unity` e dê Play. Na primeira abertura o Unity cria os arquivos `.meta` das skins novas
  (pasta `Assets/Resources/HalloweenAssets`); isso é normal.

## 2. O que está faltando no código (e onde)

O jogo compila e roda, mas o jogador **não leva dano, não perde e a dificuldade nunca aumenta**.
Os pontos estão marcados com `// TODO [PASSO N - ...]`.

| Passo | Arquivo / método | O que o jogo faz sem isso |
|---|---|---|
| 1 – Colisão | `VidaPlayer.cs` → `OnTriggerEnter` | Obstáculos e inimigos atravessam o jogador |
| 2 – Dano | `VidaPlayer.cs` → `TomarDano` | A vida nunca diminui |
| 3 – Game Over | `VidaPlayer.cs` → `IniciarMorte` | Nunca chega à tela de Game Over |
| 4 – Dificuldade | `GameManager.cs` → `Update` | Velocidade e quantidade de obstáculos ficam fixas |

## 3. Instruções lógicas para o aluno (4 passos)

1. **Colisão:** se o objeto que encostou tem a tag `obstacle` ou `enemy`, o jogador deve sofrer dano — **exceto** se o escudo (Lutador)
   ou o dash (Bárbaro) estiver ativo, ou se ele estiver invulnerável. Lembre que essas habilidades podem ser `null`.
2. **Dano:** diminua 1 vida, ligue a invulnerabilidade por `tempoInvulnerabilidade` segundos, atualize a barra de vida
   (`AtualizarHUD()`) e, se as vidas acabaram, chame `IniciarMorte()`.
3. **Game Over:** marque `morreu = true`, desligue o `MovimentoPlayer`, pare o mapa (`GerarMapa.instancia.PararGeracao()`)
   e o jogo (`GameManager.instancia.PararJogo()`), toque a animação (`"morreu"`) e, depois de `tempoAteMorte` segundos,
   chame `IrParaMorte()` com `Invoke`.
4. **Dificuldade progressiva:** a cada `intervaloAumento` segundos, aumente `velocidadeBase` e `ChanceSpawnAtual`,
   sem passar de `velocidadeMaxima` e `chanceSpawnMaxima` (use `Mathf.Min`).

### Como testar cada passo em sala
- Passo 1+2: bata num obstáculo → a vida na tela diminui e o personagem fica invulnerável por ~1,5 s.
- Passo 3: perca todas as vidas → animação de morte e, em ~2 s, vai para a cena **Morte**.
- Passo 4: jogue ~30 s → o cenário passa a vir mais rápido e com mais obstáculos.

## 4. Asset Pack de Halloween e botão PERSONALIZAR (parte "Diretor de Arte")

- No **Menu** há um botão **PERSONALIZAR** (lado esquerdo da tela). Ele abre um painel com todas as skins que estão na pasta
  `Assets/Resources/HalloweenAssets`. Clicar numa skin aplica na hora nos personagens do menu; a escolha vale na partida.
- Skins incluídas: **Original, Abóbora, Fantasma, Zumbi, Vampiro e Bruxa** — para Lutador, Bárbaro e Mago.
- **Desafio extra (arte):** o aluno cria uma skin própria colocando um PNG na pasta com o nome `<classe>_<nome>.png`
  (classes: `knight`, `barbarian`, `mage`; ex.: `mage_neon.png`). Dica: editar uma cópia de `mage_original.png`,
  pois a textura é um atlas de cores (cada quadrado de cor pinta uma parte do personagem). A skin nova aparece sozinha no painel.
- O pacote foi gerado por `Tools/gerar_skins_halloween.py` (opcional, só para quem quiser recriar/estender os temas).

## 5. Encerramento

Os alunos jogam a própria versão e adicionam o projeto ao portfólio. Nesta edição **não há Profiacoins**: o prêmio é a posse
de um projeto de excelência para o portfólio.

## 6. Gabarito

O código completo está na branch `xp-week-solucao`. Para ver exatamente o que foi removido:
`git diff xp-week-solucao xp-week-halloween -- Assets/Scripts`.

# Twinkle Twinkle Little Mole/Brilha Brilha Toupeirinha - Game Design Document (GDD)

**Responsável por manter o GDD atualizado:** [Nicolas Kerpen]

---

##  1. Visão Geral e Propósito

* Nome: twinkle twinkle little mole
* Grupo: Nicolas Kerpen, Beatriz Oliveira, Gabriel Gouveia
* Plataforma: PC

*   **O que é o jogo:** collectathon com fases não lineares que controla 2 personagens. Tudo começa quando uma toupeira entra em um templo subterrâneo buscando por comida; lá ela encontra um chapéu de mago falante que a convence a acordar os grandes espíritos. Graças ao chapéu ela ganha a habilidade de afetar as pinturas.

*   **Para quem é (Público-alvo):** +12 até 24 anos
*   **Gênero e Plataforma:** Plataforma 2D para PC, aventura, collectathon

---

##  2. Core Loop
*O que o jogador faz, repetidamente, que torna o jogo divertido.*



*   **Descrição do Loop:** Seleciona a fase -> Ganha a habilidade da fase escolhida -> Utiliza a habilidade para pegar os coletáveis da fase -> utiliza os coletaveis pegos para progredir no jogo -> Volta para a seleção de fases. 
*   **Justificativa:** (Como as mecânicas listadas abaixo servem a esse core loop).
![[WhatsApp Image 2026-10-13 at 16.41.41.jpeg]]
 
*(Nota: Arraste para cá o diagrama ou fluxograma visual do core loop)*

---

##  3. Escopo
1.  **Sistema de Duplo Plano (Foreground/Background):** O jogador controla a Toupeira em dois planos de jogo distintos (Mundo Real e o Mundo Espiritual). O sistema garante isolamento físico total (através da Matrix de Colisão de Layers) para que as interações e físicas de um plano não interfiram acidentalmente no outro. 
2. **Mecânicas Elementais e Coletáveis:** 
	- **Água:** Habilidade com limite de carga (recarregável em fontes) que permite atirar bolhas para destruir alvos à distância e apagar armadilhas das fases de fogo. 
	- **Fogo:** Habilidade com soco poderoso (`PlayerPunch`) capaz de quebrar rochas (`RochaQuebravel`). O que é destruído na pintura (fundo) afeta o mundo real (ex: derrubar um pilar). Reflete armadilhas ativas. 
	- **Terra (Chicote):** Mecânica de gancho/balanço (Grappling). Utilizando âncoras (`WhipAnchor`) espalhadas pelo cenário, a toupeira usa o chicote de terra para se pendurar, ganhar momento e transpor buracos ou alcançar plataformas altas. 
	- **Vento (Planar):** Mecânica de mobilidade aérea. Interage com correntes de ar (`WindGust`) e permite que o jogador diminua a velocidade de queda, planando longas distâncias para cruzar grandes vãos ou evitar armadilhas no chão.
	
3. **Transição Fluida de Fases:** Sistema de portais e conclusão por coletáveis que utilizam carregamento assíncrono de cenas (`SceneManager.LoadSceneAsync`) e proteção de objetos (`DontDestroyOnLoad`). Telas de carregamento são mascaradas por transições suaves de fade (preto ao entrar em fases, branco ao retornar ao Templo). 
4.  **Controles Otimizados:** Uso do Novo Input System suportando leitura apenas de Controle Gamepads e Joysticks.
### O que NÃO vamos fazer nesta etapa (Não Escopo)
*Pelo menos 3 itens definidos para evitar escopo excessivo.

1. Não faremos cutscenes animadas, apenas texto.
2. Não terá inimigos.
3. Não terá fase final e nem sub fases.
4. Não terá Sons e Trilha Sonora.
  
---

##  4. Programação e Engenharia
*Seção enxuta em tópicos para lógicas e sistemas.*
###  Decisões Já Tomadas
*O que já está aprovado e alinhado com o Planejamento de Projeto.*

- **Motor gráfico e Padrão de Arquitetura:** Unity Engine (2D) em C#. 
-  **Máquina de Estado de Progresso Global:** Implementação do Singleton `GameProgressManager`. Ele não é destruído ao mudar de fase e guarda as variáveis booleanas que definem quais habilidades (Fogo, Água, Terra, Vento) o jogador já desbloqueou. 
-  **Arquitetura de Isolamento de Planos:
	- `DefinidorDePlano.cs` É o script mestre de organização. Anexado às pastas principais (`----ObjetosFrente----` e `----ObjetosFundo----`), ele roda no `Awake` e distribui automaticamente o Enum `PlanoDeJogo` (Frente ou Fundo) para todos os portais, rochas, botões e inimigos filhos. Ele também altera a Layer de colisão de tudo dentro da pasta para isolar a física. Sua última atualização incluiu o parâmetro `(true)` para garantir que objetos desativados (invisíveis) também recebam essa configuração.
- **`GameProgressManager.cs`:**
	- O Singleton do jogo. Ele carrega as variáveis booleanas (`hasFire`, `hasWater`, `hasEarth`, `hasWind`, `hasFinalArtifact`) e utiliza `DontDestroyOnLoad` para não ser destruído ao trocar de cena. É ele quem dita o progresso do "Collectathon".
- **`TempleManager.cs`:**
	- Fica ativo apenas na cena do Hub (Templo). Ele lê o `GameProgressManager` no `Start()` e, se o jogador tiver 4 ou mais cristais, desativa o `GameObject` da barreira final, liberando o fim do jogo.
- **`CharacterSwitcher.cs`:** 
	- Gerencia a troca de controle entre a toupeira do Mundo Real e a toupeira das Pinturas. Ele ativa/desativa os componentes `PlayerMovement` e alterna as Câmeras Virtuais (Cinemachine) de cada plano, além de zerar a inércia (`linearVelocity`) do personagem que foi desativado para que ele não continue deslizando sozinho.
- Controle e Habilidades do Jogador: 
	- Os scripts do jogador modularizam as mecânicas. Todos possuem um sistema de "trava de progresso" (`CanUseAbility()`) que libera a habilidade apenas se ela foi destravada no save global, se a opção `isUnlockedPermanently` estiver marcada (para testes), ou se for a fase nativa do elemento.
		1. **`PlayerMovement.cs`:** 
			- A base física da toupeira. Controla velocidade, inércia e pulo. Possui cálculos automáticos no `Awake()` que multiplicam a velocidade, força de pulo e escala de gravidade com base no tamanho do personagem (para que a minitoupeira do fundo não seja rápida ou pesada demais). Inclui um sistema de _Hang Time_ que reduz a gravidade no pico do pulo para saltos mais precisos.
		2. **`PlayerPunch.cs` (Fogo):** 
			- Cria um círculo de colisão invisível (`OverlapCircleAll`) na frente do jogador quando o botão é pressionado. Se acertar uma `RochaQuebravel` do mesmo plano, ela é destruída.
		3. **`PlayerShoot.cs` & `Projectile.cs` (Água):**
			- O `PlayerShoot` controla o limite de munição e instancia o `Projectile`. O `Projectile` herda o plano e a escala de quem atirou (para nascer pequeno no fundo) e voa horizontalmente. Ao colidir, ele checa o plano do objeto acertado e se destrói ao tocar no chão, ativando botões ou quebrando rochas específicas no caminho.
		4. **`PlayerWhip.cs` (Terra):** 
			- Cria um sistema de _Grappling Hook_. Ao apertar o botão, ele busca a `WhipAnchor` mais próxima do mesmo plano. Se encontrar, ele trava o `PlayerMovement` (para a gravidade base não interferir), cria um `DistanceJoint2D` dinâmico atrelado à âncora, desenha uma linha (`LineRenderer`) e permite o balanço (`swingForce`) usando inputs direcionais. Ao soltar o botão, o jogador é arremessado mantendo a inércia.
		5. **`PlayerGlide.cs` (Vento):** 
			- Usado ao segurar o botão de pulo no ar. Limita a velocidade máxima de queda (`glideFallSpeed`). Se o personagem entrar em um `WindGust`, este script ignora a gravidade padrão e suavemente arrasta a velocidade Y do Rigidbody para cima e para os lados, acompanhando a força do vento.

###  Próximos Passos (Em Aberto)
*O que ainda precisa ser discutido, sem inventar decisões.*
1. Como será feito o save do jogo persistente? (Uso de `PlayerPrefs`, `.JSON` gerado por serialização, ou salvamento de estado do Singleton em arquivo local?).
2. Adicionar a "Morte" no jogo, e como ela vai impactar o jogador, talvez o colocando novamente na sala de seleção após perder todas as vidas?
3. Implementar e otimizar os *Tilemaps* definitivos dos cenários em conjunto com a equipe de arte, garantindo que suas colisões conversem perfeitamente com os scripts de `DefinidorDePlano`.
4. Adicionar feedback visual e sonoro para as habilidades do jogador.
5. Consertar a física do Grappling na habilidade da Terra.
6. Adicionar as funções secundárias de cada habilidade.

---

##  5. Arte, UI e Assets


###  Estilo e Conceitos Decididos


**Cenarios:
Para a construção dos cenários do jogo foram escolhidos templos como inspiração para cada território. A escolha levou em conta as características de cada elemento e as necessidades de construção de cada fase. Como principal referência para a flora será utilizada a ilha de Yakushima. Ela possui árvores milenares de grande porte e uma vegetação que varia de subtropical a temperada conforme a altitude. Essa característica ajuda a construir uma lógica natural para a diversidade de ambientes do mundo do jogo.

Um dos principais exemplos é o templo Ta Prohm, que parece estar sendo engolido pela natureza com suas arvores gigantes mescladas nas suas ruinas sem destruilo. Ele serve como referência para o território do elemento terra, sua intenção não é representar apenas a terra mas também sua vitalidade e a força da natureza.

![[moodbooartTerra.png|479]]
moodbooad simples criado pela artista Beatriz


![[TerraPuzlle(Simples0).png|475]]
mockup criado pela artista Beatriz

**Hambientação:
Para a ambientação dos templos será utilizada como referência a animação O Segredo de Kells. 
![[Sin título(1).png]]
moodbooard criado pela artista Beatriz

A intenção é explorar a relação entre arquitetura, pinturas e natureza sem reproduzir diretamente sua ornamentação, sua composição buscará equilíbrio para que os detalhes do cenário não ofusquem as pinturas. A representação da natureza também terá influência da animação, só que em vez de buscar o realismo serão utilizadas formas estilizadas e simbólicas. 

Os cenários poderão mudar de acordo com a emoção da cena indo de Momentos tranquilos onde terão formas mais suaves enquanto momentos de tensão poderão apresentar formas angulares ou distorcidas.

A ornamentação dos templos também estará relacionada aos padrões geométricos ou simbólico de cada Grande Espírito. Essas formas poderão aparecer na arquitetura, nas pinturas e na natureza ao redor de maneira sutil, assim cada território terá uma identidade visual própria.

Com isso a proposta do jogo seria busca conectar os Grandes Espíritos aos ambientes em que estão inseridos. Tendo suas formas e os padrões de cada Grandes Espíritos aplicada no mundo ao seu redor reforçando a identidade visual do jogo.

**Personagens:

A criação dos personagens teve como inspiração a arte dos alebrijes, que transforma animais em criaturas fantásticas por meio de padrões, formas e ornamentações e 

moodbooard de Exemplo 1:
![[board2.png|374]]
Taller Jacobo y María Ángeles

O corpo de cada animal será dividido em partes, preenchidas com formas simples e padrões geométricos de maneira econômica e sem excessos. Para isso serão utilizadas as técnicas de :

Geometric Vector Art - que dará formas geométricas ao corpo dos personagens mas com curvas para não deixa-los com uma estética reta demais.

padronagem geometrizada- onde usara padrões geométricos ou simbólicos variados aplicados em partes específicas do animal, valorizando elementos que representem sua identidade. Assim, cada espírito terá padrões em áreas de destaque diferentes, de acordo com suas características.

moodbooard de Exemplo 2:
![[Sin título 1.png|369]]
Obras de George Tonks e Jo Scott

Então para explicar melhor a construção podemos vela assim:

Animal → Simplificação da silhueta com formas geométricas → Aplicação de padrões que representam sua identidade espiritual → Detalhamento final, com elementos que reforçam suas características ou a aplicação da pedra específica de cada espírito.


Buscando desenvolver uma identidade visual própria os personagens apresentam  semelhanças estéticas com algumas representações artísticas mesoamericanas, principalmente das culturas maia e asteca, embora elas não tenham sido utilizadas como referência direta para sua criação.





**
- [ ] Proporção da tela e paleta de cores.
*(Arraste imagens de referência visual e mockups para cá)*

proporção de tela padrão HD 1920x1080

![[aguaPuzlle(Simples) 1.png]]
Exemplo simplificado da fase de agua (mockup criado pela artista Beatriz)



Habilidade água:
essa habilidade requer sabedoria ao utiliza pois ele possui em limite de carga, pode carregar essa habilidade em fontes de agua.

![[mokcupToperinha carregando00.png|218]]
1° caracteristica
tiro com bolhas serve para destruir coisas a distancia.
![[MokupHabilidadeagua 1.png]]


![[aguaPuzlle2(Simples) 1.png|475]]


2° caracteristica
é possível também utilizar a habilidade de agua para apagar armadilhas das fase do elemento fogo.



![[aguahabilidade1(Simples) 1.png|700]]


![[aguahabilidade2(Simples).png|346]]
Habilidade de Fogo:
1° caracteristica
![[MokcupsFogo1 1.png]]
Refletir golpes de  inimigos  ou armadilhas .

2° caracteristica
![[mokcups2Fogo.png]]
soco poderoso pode quebra pedras assim se destruir algo na pintura pode ajudar a toupeira do mundo real ex : derruba um pilar

Habilidade da terra:
1° caracteristica
![[MockupsTerra 1.png]]


2° caracteristica

![[Terrahabilidade2(Simples).png|360]]
###  Próximos Passos (Em Aberto)

parte de arte:


Montar o tilemap das pedras utilizadas no chão e no topo das estruturas, com três variações de 14 peças cada: pedras entalhadas com símbolos, pedras simples sem entalhes e pedras com detalhes de limo, corrosão ou lava, totalizando 42 peças para o tilemap de cada fase elemental.

(Os tiles de chão e telhado serão compartilhados entre todas as fases. Para finalizar as peças, falta apenas aplicar as cores.)

Animação do player e mini toupeira.

1 coletável simples

- [ ] Design final do inimigo principal.
- [ ] Interface (UI) in-game.

---

##  6. Referências

*link de imagens retiradas para os moodbooard 1:
 https://www.jacoboymariaangeles.com/en/hermanos-de-la-tierra
link de imagens retiradas para os moodbooard  2:
  https://www.behance.net/gallery/115062845/Vector-Animals
 https://www.joscott.co.za/digital-art

---

##  7. Links Úteis e ChangeLog
*   Link para o Miro: [Miro](https://miro.com/app/board/uXjVH0oqiZM=/?share_link_id=276353536082)
*   **Última atualização (Changelog):** [19/10/26] - [Adicionado a seção de programação e engenharia.].



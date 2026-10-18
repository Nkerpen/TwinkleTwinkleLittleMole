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



*   **Descrição do Loop:** (Ex: Explorar o mapa -> Coletar recursos -> Melhorar a base -> Repetir).
*   **Justificativa:** (Como as mecânicas listadas abaixo servem a esse core loop).
![[WhatsApp Image 2026-10-13 at 16.41.41.jpeg]]
 
*(Nota: Arraste para cá o diagrama ou fluxograma visual do core loop)*

---

##  3. Escopo e "Não Escopo"

### O que o jogo VAI ter (Escopo)
- [x] (Mecânica principal 1)
- [x] (Mecânica principal 2)

### O que NÃO vamos fazer nesta etapa (Não Escopo)
*Pelo menos 3 itens definidos para evitar escopo excessivo.*
1. [ ] Não terá multiplayer online.
2. [ ] Não terá sistema de inventário complexo com peso.
3. [ ] Não faremos cutscenes animadas, apenas texto.
4. [ ] não terá inimigos
5. [ ] não terá faze final e nem sub fases
6. [ ] som
  
---

##  4. Programação e Engenharia
*Seção enxuta em tópicos para lógicas e sistemas.*

###  Decisões Já Tomadas
*O que já está aprovado e alinhado com o Planejamento de Projeto.*
- [x] Motor gráfico e padrão de arquitetura.
- [x] Sistema de movimentação base.

###  Próximos Passos (Em Aberto)
*O que ainda precisa ser discutido, sem inventar decisões.*
- [ ] Como será feito o save do jogo? 

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

1° caracteristica
tiro com bolhas serve para destruir coisas a distancia.
![[MokupHabilidadeagua 1.png]]


![[aguaPuzlle2(Simples) 1.png|475]]


2° caracteristica
é possível também utilizar a habilidade de agua para apagar armadilhas das fase do elemento fogo.

![[aguahabilidade1(Simples).png]]

![[aguahabilidade2(Simples).png|462]]



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
*   **Última atualização (Changelog):** [Data] - [O que mudou brevemente. Ex: Adicionado core loop e concept de arte].
*


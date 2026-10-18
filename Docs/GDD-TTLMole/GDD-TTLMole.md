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
*   **Escopo do Projeto:**
	* **Quantidade de Fases:** 4 fases e um hub interativo
	* **NPCs:**
		* **Chapéu:** Um misterioso companheiro que está junto do jogador desde o início, é um chapéu mágico com grandes conhecimentos arcanos e segredos de igual tamanho. Ele é misterioso, zangado e sua personalidade entra em atrito com a inocência da toupeira.
		* ![[Pasted image 20261018085336.png|169]]
		* **Garça Labradorita (Espírito do Ar):** O Espírito do Ar é a Garça, com o humor volátil dos céus, seja tempestuoso ou ensolarado. Ela decora seu ninho com a pedra mágica Labradorita e tende a sofrer constantes transformações que alteram sua intuição e humor. Talvez a mais poderosa dos quatro, está acima de seus irmãos comandando os ares.
		* ![[Pasted image 20261018085034.png|171]]
		* **Polvo Perolado (Espírito da Água):** O Espírito da Água é o Polvo. Possui grande sabedoria, tem a habilidade de se esconder dentro de conchas e pode gerar através de seus anéis azuis pérolas mágicas que simbolizam a transformação da dor em beleza.
		* ![[Pasted image 20261018084816.png|223]]
		* **Serpente Rubi (Espírito do Fogo):** O Espírito do Fogo é a víbora. Com tamanha paixão ardente que queima até em seu olhar, ela possui grande vitalidade, escamas fortificadas e resistentes lapidadas com pedras mágicas rubi que evidenciam sua coragem e poder. 
		  ![[Pasted image 20261018084636.png|218]]
		* **Lince Bismuto (Espírito da Terra):** O Espírito da Terra é o Lince, que possui uma forte conexão com a pedra bismuto, responsável pela abundância da natureza local. Com o tempo, a energia do cristal se espalhou pelo seu corpo, tornando-o a própria ordem e conexão coletiva entre a terra, arvores e rios.
		  ![[Pasted image 20261018084459.png|181]]

---

##  2. Gameplay, Mecânicas e Loop
*O que o jogador faz, repetidamente, que torna o jogo divertido.*


- **Coreloop do jogo:**
	*   **Descrição do Loop:** (Ex: Explorar o mapa -> Coletar recursos -> Melhorar a base -> Repetir).

![[WhatsApp Image 2026-10-13 at 16.41.41.jpeg]]

- **Progressão:** O player tem um caminho claro que é seguido ao decorrer do jogo. Ao iniciar no hub central, ele tem um pequeno tutorial e suas primeiras interações com o personagem chapéu. Ali ele entenderá que sua missão é acordar os quatro espíritos adormecidos sob aquele templo, partindo assim para uma escolha entre três fases: Água, Terra e Fogo. 

	- **As fases:** Cada uma delas possui uma mecânica única que será introduzida e aprofundada ao decorrer. Após utilizar dessa mecânica e atravessar os desafios e obstáculos do ambiente, o player se encontrará com o respectivo espírito e poderá acordar ele. Após isso, ele conclui a fase e volta para o hub central, onde poderá escolher a próxima.
	
	- **A Última fase:** Talvez a mais desafiadora, a fase do Ar é a única que obrigatoriamente é deixada para o final e não pode ser escolhida antes. A missão do player ali é acordar o espírito da garça, o mais poderoso dos outros espíritos. 
	
	- **As Mecânicas:** Após acordar um espírito, a toupeira desbloqueia a mecânica especial usada naquela fase, permitindo que ela a use para atravessar alguns desafios no hub e em outros ambientes. As 4 mecânicas são:
	
		**- Grappling Hook (Fase da Terra)**
		**- Disparos de Projéteis (Fase da Água)**
		**- Ataque carregado (Fase do Fogo)**
		**- Planar (Fase do Ar)**
	
	- **Combate:**

- **Replays e Saves:** O Hub funciona como um checkpoint. Quando entra numa fase, a toupeira possui 3 vidas. Se perder todas elas, ela perde a fase e ou precisa recomeçar do zero ou voltar para o hub. Perder uma fase apenas volta o progresso dela mesma, não reiniciando fases que você já completou. 

- **Easter Eggs:** Pretendemos colocar alguns desafios em determinados levels que só podem ser completados se você já tiver desbloqueado uma habilidade específica de outra fase, com recompensas ainda não definidas.
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
##  5. Histórias e Narrativas


- **História de Fundo:** O mundo vivia em harmonia sendo governado pelos quatro espíritos centrais até que em certo momento, um poderoso feiticeiro os desafiou. Ele ansiava pelo poder dos espíritos, queria ser grande como eles. O feiticeiro estudou uma magia capaz de roubar esse poder para si. Temendo o destino do mundo, os quatro se uniram para derrotar a ameaça. O magia no entanto os enfraqueceu e como última alternativa, usaram suas maiores forças para aprisiona-lo, adormecendo no processo. Eras se passaram e a prisão do mago ficou isolada em uma ilha protegida no meio do mar, até que certo dia, uma pequena criaturinha caiu dos céus e mudou tudo.
- **Personagem Principal:**
	- **Background:**
	- **Personalidade:** Inocente, manipulável, tímida e curiosa.
	- **Aparência:** Uma pequena toupeira com nariz de estrela de coloração azulada. 
##  6. Arte, UI e Assets


*Apresente imagens, mockups e referências visuais onde o texto sozinho não for suficiente.*

###  Estilo e Conceitos Decididos
- [ ] Estilo visual principal.

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

![[aguaPuzlle(Simples).png]]
mockup criado pela artista Beatriz



Habilidade água:![[MokupHabilidadeagua 1.png]]
![[MokupHabilidadeagua2.png|344]]



Habilidade de Fogo:
![[MokcupsFogo1 1.png]]

![[mokcups2Fogo.png]]
soco poderoso pode quebra pedras assim se destruir algo na pintura pode ajudar a toupeira do mundo real ex : derruba um pilar

Habilidade da terra:
![[MockupsTerra 1.png]]

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


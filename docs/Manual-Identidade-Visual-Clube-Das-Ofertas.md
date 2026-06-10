---
titulo: Manual de Identidade Visual - Clube Das Ofertas
criado: 2026-06-10
atualizado: 2026-06-10
fontes:
  - ../Manual Logo Das Rede.pdf
  - ../Clube Das Ofertas fundo redondo.png
---

# Manual de Identidade Visual - Clube Das Ofertas

Este manual recompõe a estrutura de diretrizes de marca do arquivo `Manual Logo Das Rede.pdf` e atualiza a paleta oficial a partir do asset [Clube Das Ofertas fundo redondo.png](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/Clube Das Ofertas fundo redondo.png>), para uso digital no site e em materiais de apoio.

## 1. Objetivo da marca

A marca `Clube Das Ofertas` deve comunicar:

- varejo e movimento;
- oferta em destaque;
- contraste alto e leitura imediata;
- linguagem popular, forte e promocional.

O símbolo principal combina quatro elementos visuais:

- fundo preto para contraste e foco;
- amarelo vibrante como cor dominante de chamada;
- vermelho intenso na etiqueta de oferta;
- branco como cor de leitura e contorno.

## 2. Assinatura principal

A assinatura prioritária para uso digital deve seguir esta hierarquia:

1. fundo preto;
2. lettering e símbolo principal em amarelo;
3. palavra `clube` em branco;
4. etiqueta `ofertas` em vermelho com elementos brancos.

Para aplicações no site, essa assinatura deve ser preservada sem distorção, sem troca de proporção e sem reinterpretação cromática.

## 3. Paleta oficial

As cores abaixo foram extraídas da imagem redonda da marca e devem ser tratadas como padrão:

| Papel | Hex | Uso principal |
|---|---|---|
| Preto institucional | `#000000` | fundo principal, áreas de contraste, rodapé, blocos de destaque |
| Amarelo principal | `#FFED00` | CTAs, destaques, bordas ativas, números e elementos promocionais |
| Vermelho oferta | `#A21815` | ações críticas, selo de oferta, badges de alerta, botões principais promocionais |
| Branco de apoio | `#FFFFFF` | tipografia sobre fundo escuro, linhas, áreas de respiro |

Tons auxiliares recomendados para interface:

| Papel | Hex | Uso |
|---|---|---|
| Preto elevado | `#121212` | superfícies escuras com leve separação do fundo absoluto |
| Vermelho profundo | `#950000` | hover, estados pressionados, detalhes de contraste |
| Amarelo suave | `#FFF6A8` | fundos de aviso leve, realce discreto |
| Cinza quente claro | `#F5F1E8` | fundo neutro alternativo quando o preto integral não for desejado |

## 4. Proporção de uso das cores

Para produtos digitais, a recomendação base é:

- `50%` a `60%` preto ou neutro muito escuro nas áreas de contraste;
- `20%` a `30%` branco ou neutro claro para leitura e respiro;
- `10%` a `20%` amarelo para destaque visual;
- `5%` a `10%` vermelho para oferta, ação e sinalização.

O amarelo deve ser a cor dominante de destaque. O vermelho deve aparecer como acento, não como fundo predominante de toda a interface.

## 5. Tipografia recomendada

Para o site, a tipografia deve reforçar impacto promocional e leitura simples:

- títulos: fonte pesada, arredondada ou grotesca de forte presença;
- subtítulos: sans serif de boa legibilidade;
- textos corridos: sans serif limpa, com contraste alto.

Direção recomendada:

- títulos e números: `Montserrat`, `Poppins`, `Archivo` ou equivalente robusta;
- interface e corpo: `Inter`, `Nunito Sans` ou equivalente de leitura confortável.

## 6. Fundos e contraste

Fundos permitidos:

- preto institucional;
- branco puro;
- neutro claro quente;
- amarelo muito claro em áreas pontuais.

Regras de contraste:

- amarelo principal deve preferir fundos pretos ou muito escuros;
- vermelho oferta deve preferir branco, preto ou neutros claros ao redor;
- textos brancos só devem aparecer sobre preto, vermelho profundo ou áreas muito escuras;
- textos pretos devem ser a regra em superfícies claras.

## 7. Aplicação no site

Direção visual recomendada para o produto:

- cabeçalho: preto com destaques amarelos e acentos vermelhos;
- cards e painéis: branco ou neutro claro, com bordas amarelas suaves;
- botões primários: vermelho oferta com texto branco;
- botões secundários: amarelo principal com texto preto;
- links ativos, filtros e contadores: amarelo principal;
- badges de promoção ou atenção: vermelho oferta;
- áreas de fundo institucional ou seções de destaque: preto.

## 8. Tokens CSS sugeridos

Base inicial para implementação:

```css
:root {
  --color-brand-black: #000000;
  --color-brand-black-soft: #121212;
  --color-brand-yellow: #FFED00;
  --color-brand-yellow-soft: #FFF6A8;
  --color-brand-red: #A21815;
  --color-brand-red-deep: #950000;
  --color-brand-white: #FFFFFF;
  --color-surface: #F5F1E8;
  --color-text-dark: #161616;
  --color-text-light: #FFFFFF;
  --color-border-highlight: #E8C93A;
}
```

## 9. Regras de uso incorreto

Nao fazer:

- aplicar gradientes sobre a marca;
- trocar o amarelo principal por dourado, laranja ou mostarda;
- usar vermelho vivo diferente do tom institucional em elementos centrais da marca;
- aplicar sombra pesada na logo;
- esticar horizontalmente ou verticalmente;
- usar a marca principal sobre fundo amarelo forte sem contorno ou respiro;
- substituir o fundo preto da assinatura principal por cores frias fora da paleta.

## 10. Diretriz para o rodape e logos

Para uso no site:

- preferir arquivos com fundo transparente quando a logo estiver flutuando sobre a interface;
- manter a versao com fundo fechado apenas quando a propria arte pedir contraste interno;
- nao criar caixas brancas artificiais atras da logo;
- preservar margem de respiro ao redor do asset, especialmente no canto inferior direito.

## 11. Diretriz para a próxima etapa do site

Na atualização visual do sistema, seguir esta ordem:

1. trocar variáveis globais de cor;
2. ajustar cabeçalho, botões e links ativos;
3. revisar badges, tabelas e cards;
4. validar contraste de leitura;
5. alinhar gráficos, filtros e estados de hover à nova paleta.

## Resumo executivo

Paleta principal aprovada para o site:

- preto: `#000000`
- amarelo: `#FFED00`
- vermelho: `#A21815`
- branco: `#FFFFFF`

Direção visual:

- promocional;
- contrastada;
- popular;
- limpa;
- forte em amarelo e vermelho, sustentada por preto e branco.

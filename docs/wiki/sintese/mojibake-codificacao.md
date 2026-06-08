---
titulo: mojibake-codificacao
categoria: sintese
criado: 2026-06-08
atualizado: 2026-06-08
fontes: []
links: []
---

# Mojibake de codificação

Resumo da investigação sobre textos como `descriÃ§Ã£o`.

- `origin/main` ainda contém textos corretos com acentuação em `src/ClubeDasOfertas.Web/Program.cs`.
- A comparação `git diff origin/main -- src/ClubeDasOfertas.Web/Program.cs` mostra que vários literais foram regravados de `Descrição` para `DescriÃ§Ã£o`, `Formulário` para `FormulÃ¡rio` e equivalentes.
- O padrão indica mojibake clássico: bytes UTF-8 foram lidos como Windows-1252/Latin-1 e depois salvos novamente em UTF-8.
- O commit-base `71909e7` ainda exibe os textos corretamente; a corrupção já aparece em commits locais posteriores, com destaque para `578737e` em `Program.cs`.
- A wiki também apresentou o mesmo sintoma em arquivos criados mais recentemente, o que reforça que a causa foi o processo de edição/gravação, não uma regra da aplicação.

Implicação prática: para corrigir, é preciso restaurar os literais a partir de uma versão íntegra e evitar regravações sem UTF-8 explícito.

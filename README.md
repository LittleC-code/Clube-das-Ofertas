# Clube Das Ofertas

Sistema web interno para importar itens de campanha, cruzar com catalogo de codigos, aplicar regras de risco, exigir revisao e exportar CSV pronto para o CRM.

Documentacao de fluxo e manutencao:

- [docs/FLUXOGRAMA.md](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/docs/FLUXOGRAMA.md>)

## Stack

- .NET 10 / ASP.NET Core
- PostgreSQL
- Npgsql
- HTML server-side com autenticacao por cookie

## Rodar localmente

1. Instale o SDK .NET 10.
2. Configure as variaveis locais do banco. O repositorio inclui um exemplo em `.env.example`.
   Neste ambiente local, a senha foi salva no arquivo `.env`, que e ignorado pelo git.
3. Suba um PostgreSQL local. Se tiver Docker:

```powershell
docker compose up -d
```

4. Restaure e compile:

```powershell
dotnet build ClubeDasOfertas.slnx
```

5. Iniciar o servico:

```powershell
Get-Content .env | ForEach-Object {
  if ($_ -match '^\s*([^#=]+)=(.*)$') {
    [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
  }
}
dotnet run --project src\ClubeDasOfertas.Web\ClubeDasOfertas.Web.csproj --urls http://0.0.0.0:5088
```

Esse comando deixa a aplicacao ouvindo em `http://localhost:5088`.

6. Pausar o servico:

```powershell
Get-CimInstance Win32_Process -Filter "name = 'dotnet.exe'" | Where-Object { $_.CommandLine -like '*ClubeDasOfertas.Web.csproj*' } | ForEach-Object { Stop-Process -Id $_.ProcessId }
```

Se a aplicacao tiver sido iniciada em segundo plano ou se o `dotnet run` deixar o executavel hospedado ativo, finalize tambem qualquer instancia residual do processo web:

```powershell
Get-Process ClubeDasOfertas.Web -ErrorAction SilentlyContinue | Stop-Process -Force
```

Para encerrar tudo de uma vez com seguranca:

```powershell
Get-CimInstance Win32_Process | Where-Object {
  ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -like '*ClubeDasOfertas.Web.csproj*') -or
  $_.Name -eq 'ClubeDasOfertas.Web.exe'
} | ForEach-Object {
  Stop-Process -Id $_.ProcessId -Force
}
```

Para confirmar que o servico realmente parou:

```powershell
Get-CimInstance Win32_Process | Where-Object {
  ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -like '*ClubeDasOfertas.Web.csproj*') -or
  $_.Name -eq 'ClubeDasOfertas.Web.exe'
}
```

7. Reiniciar o servico:

```powershell
Execute novamente o comando do passo 5.
```

8. Acesse `http://localhost:5088` localmente ou `http://IP-DA-MAQUINA:5088` pela rede.

Se ainda nao existir nenhum usuario no banco, o sistema abre em `/setup` e pede a criacao do primeiro administrador.

## Fluxo operacional

1. Criar o administrador inicial em `/setup` ou informar usuarios bootstrap via configuracao.
2. Entrar como admin e importar o catalogo usando a planilha atual `CHECK LIST - Clube Das Ofertas.xlsm` ou outro arquivo com as colunas `Descricao Tabloide`, `Categoria`, `Descricao Solidus`, `Cod Barras`.
3. Criar uma campanha informando nome e vigencia.
4. Importar o arquivo de itens com layout fixo: `Fonte`, `Vigencia`, `Descricao no Tabloide`, `Quantidade limitada`, `Venda`, `Venda Clube`.
5. Revisar itens bloqueados:
   - sem catalogo/codigo;
   - pesaveis convertidos para kg;
   - fardos/caixas detectados;
   - preco ou quantidade invalida.
6. Exportar CSV quando nao houver pendencias criticas.

## Regras de seguranca e upload

- Todos os formularios `POST` usam token antiforgery.
- Uploads aceitam apenas `CSV`, `TXT`, `XLSX` e `XLSM`.
- O tamanho maximo por arquivo e `10 MB`.
- Arquivos `XLSX/XLSM` precisam ter assinatura ZIP valida.
- Arquivos de texto com bytes nulos sao rejeitados.

## Testes

Os testes locais validam parsers, leitura da planilha `.xlsm` real, regras criticas de conversao e protecoes de upload.

```powershell
dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj
```

## Configuracao

`src/ClubeDasOfertas.Web/appsettings.json` nao carrega mais senha de banco nem credenciais padrao.

### Banco PostgreSQL

A connection string base fica em `ConnectionStrings:PostgreSql`:

```json
"Host=localhost;Port=5432;Database=clube_das_ofertas;Username=clube"
```

Informe a senha por uma destas opcoes:

- `ConnectionStrings__PostgreSqlPassword`
- `POSTGRESQL_PASSWORD`
- `POSTGRES_PASSWORD`

Para este ambiente local, o arquivo `.env` ja foi preenchido e esta fora do versionamento.

### Usuarios bootstrap opcionais

Se quiser iniciar o sistema com usuarios preconfigurados, informe:

- `App__BootstrapAdminEmail`
- `App__BootstrapAdminPassword`
- `App__BootstrapOperatorEmail`
- `App__BootstrapOperatorPassword`

Sem essas chaves, o primeiro acesso vai para `/setup`.

# TechStore

Sistema de gerenciamento de estoque desenvolvido com .NET.

## Sobre o projeto

O TechStore é uma aplicação para gerenciamento de produtos e estoque, composta por API REST, aplicação Web MVC e aplicativo .NET MAUI.

O sistema permite gerenciar produtos, categorias, usuários, entradas e saídas de estoque, histórico de movimentações e produtos com estoque baixo.

## Tecnologias

- C#
- .NET 10
- ASP.NET Core
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- JWT
- .NET MAUI
- HTML/CSS
- Bootstrap
- xUnit

## Estrutura

### TechStore.API

API responsável pela autenticação, regras de negócio e acesso ao banco de dados.

### TechStore.Web

Aplicação Web MVC que consome a TechStore.API.

### TechStore.Mobile

Aplicativo desenvolvido com .NET MAUI que consome a TechStore.API.

### TechStore.Tests

Projeto de testes automatizados das regras de estoque.

## Funcionalidades

- Autenticação com JWT.
- Controle de acesso pelos perfis Admin e User.
- Criação do primeiro administrador por configuração local.
- Cadastro de produtos.
- Cadastro de categorias.
- Gerenciamento de usuários pelo administrador.
- Controle de estoque.
- Entrada e saída de produtos.
- Bloqueio de estoque negativo.
- Rejeição de quantidades inválidas.
- Histórico de movimentações.
- Consulta de produtos com estoque baixo.
- Dashboard.
- Pesquisa de produtos.
- Sessão persistente no Mobile.
- Logout.
- Tratamento de sessão expirada.

## Arquitetura

A aplicação Web e o aplicativo Mobile utilizam a mesma API e o mesmo banco:

- TechStore.Web → TechStore.API → SQLite.
- TechStore.Mobile → TechStore.API → SQLite.

Os clientes consultam e atualizam os dados centralizados na API.

## Banco de dados

O projeto utiliza SQLite com Entity Framework Core e migrations.

Principais entidades:

- Product
- Category
- StockMovement
- User

A API aplica as migrations ao iniciar.

## Pré-requisitos

- SDK do .NET 10.
- Git para clonar e versionar o projeto.
- Para o Mobile Windows: Windows e ferramentas do .NET MAUI.
- Para Android: ferramentas do .NET MAUI, Android SDK e JDK compatíveis.

## Segurança

A autenticação utiliza JWT, com validação de emissor, destinatário, validade e assinatura.

As senhas são armazenadas com hash usando PasswordHasher.

As credenciais e a chave JWT não devem ser incluídas no repositório.

No desenvolvimento, esses valores são configurados usando .NET User Secrets.

## Configuração inicial

Execute os comandos abaixo no terminal PowerShell.

A partir da pasta principal do projeto, entre na API:

```powershell
cd TechStore.API
```

### Chave JWT

Configure uma chave JWT aleatória com pelo menos 32 bytes.

Substitua o marcador abaixo pela sua própria chave. Não utilize o texto de exemplo como chave:

```powershell
dotnet user-secrets set "Jwt:Key" "SUBSTITUA-POR-SUA-CHAVE-ALEATORIA"
```

Se a chave já estiver configurada, não é necessário repetir esse passo.

### Primeiro administrador

Configure o nome e o e-mail:

```powershell
dotnet user-secrets set "InitialAdmin:Name" "Administrador TechStore"
dotnet user-secrets set "InitialAdmin:Email" "admin@techstore.local"
```

Execute o bloco abaixo no PowerShell e informe uma senha com pelo menos 12 caracteres na janela de credenciais:

```powershell
$adminCredential = Get-Credential -UserName "admin@techstore.local" -Message "Defina uma senha com pelo menos 12 caracteres"

@{
    "InitialAdmin:Password" = $adminCredential.GetNetworkCredential().Password
} | ConvertTo-Json -Compress | dotnet user-secrets set

Remove-Variable adminCredential
```

Quando não existe administrador, a API cria uma conta com essas credenciais.

Se já existe um administrador, nenhuma conta ou senha é alterada.

Se o e-mail configurado pertencer a uma conta sem perfil Admin, a inicialização solicita um e-mail diferente.

User Secrets é utilizado no ambiente de desenvolvimento. Em outros ambientes, forneça as configurações por um mecanismo de segredos ou por variáveis de ambiente.

## Executando a API

A partir da pasta principal do projeto:

```powershell
cd TechStore.API
dotnet run
```

No ambiente de desenvolvimento utilizado no projeto, o endereço é:

http://localhost:5221

Confira o endereço exibido no terminal e mantenha a API em execução enquanto utiliza o Web ou o Mobile.

## Executando o Web

Com a API rodando, abra outro terminal na pasta principal do projeto:

```powershell
cd TechStore.Web
dotnet run
```

Abra no navegador o endereço exibido no terminal do Web.

No primeiro acesso a um banco novo, utilize o e-mail e a senha configurados para o administrador.

Se o banco já possui um administrador, utilize as credenciais existentes.

## Executando o Mobile no Windows

Com a API rodando, abra outro terminal na pasta principal do projeto:

```powershell
cd TechStore.Mobile
dotnet run -f net10.0-windows10.0.19041.0
```

## Testes automatizados

Na pasta principal do projeto, execute:

```powershell
dotnet test .\TechStore.Tests\TechStore.Tests.csproj
```

Os testes utilizam um banco SQLite temporário em memória, independente do banco utilizado pela aplicação.

São verificados 14 casos, incluindo:

- Entrada com atualização do saldo e registro da movimentação.
- Saída com atualização do saldo e registro da movimentação.
- Saída igual ao saldo disponível.
- Rejeição de saída superior ao saldo.
- Rejeição de quantidade zero ou negativa.
- Rejeição de tipos de movimentação inválidos.
- Rejeição de produto inexistente.
- Normalização do tipo da movimentação.

Esses testes verificam o controller e a persistência das regras de estoque. Não abrangem autenticação, interfaces ou concorrência.

Para gerar o relatório TRX, execute na pasta principal:

```powershell
dotnet test .\TechStore.Tests\TechStore.Tests.csproj --logger "trx;LogFileName=estoque.trx" --results-directory .\evidencias\testes
```

O relatório será salvo em:

```text
evidencias/testes/estoque.trx
```

## Android

O projeto possui target Android por meio do .NET MAUI.

A configuração do ambiente, a compilação e os testes no Android ainda estão pendentes.

Também é necessário adequar o endereço da API ao dispositivo ou emulador.

## Validação e pendências

- API, Web e Mobile Windows: execução e integração verificadas manualmente.
- Regras de estoque: 14 casos automatizados aprovados.
- Primeiro administrador: criação e login verificados em banco separado.
- Android: configuração do ambiente, compilação e testes pendentes.
- Demais melhorias solicitadas na revisão acadêmica: em andamento.
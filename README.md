# TechStore

Sistema de gerenciamento de estoque desenvolvido com C# e .NET 10.

## Sobre o projeto

O TechStore foi desenvolvido para o Projeto Integrado Multidisciplinar IV do curso de Análise e Desenvolvimento de Sistemas.

A solução é composta por API REST, aplicação Web MVC e aplicativo .NET MAUI para Windows, utilizando a mesma API e o mesmo banco de dados.

O sistema permite gerenciar produtos, categorias, usuários e movimentações de estoque, além de consultar indicadores e produtos com estoque baixo.

## Tecnologias

- C#
- .NET 10
- ASP.NET Core
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- JWT
- .NET MAUI para Windows
- HTML e CSS
- Bootstrap
- xUnit
- Git e GitHub
- GitHub Actions
- Docker

## Estrutura do projeto

| Projeto | Responsabilidade |
|---------|------------------|
| TechStore.API | Autenticação, regras de negócio e acesso ao banco de dados. |
| TechStore.Web | Aplicação Web MVC que consome a API. |
| TechStore.Mobile | Aplicativo .NET MAUI para Windows que consome a API. |
| TechStore.Tests | Testes automatizados das regras de estoque e das triggers. |

Outros arquivos importantes:

- `database/TechStore.sql`: script SQL da estrutura do banco e das triggers.
- `.github/workflows/ci.yml`: pipeline de integração contínua.
- `TechStore.API/Dockerfile`: configuração da imagem Docker da API.
- `.dockerignore`: exclusões do contexto de construção da imagem.
- `evidencias/testes`: relatórios de testes registrados no projeto.

## Funcionalidades

- Autenticação com JWT.
- Controle de acesso pelos perfis Admin e User.
- Criação do primeiro administrador por configuração.
- Cadastro, consulta, edição e exclusão de produtos.
- Cadastro, consulta, edição e exclusão de categorias.
- Gerenciamento de usuários pelo administrador.
- Pesquisa de produtos por nome ou código.
- Registro de entradas e saídas de estoque.
- Histórico de movimentações.
- Consulta de produtos com estoque baixo.
- Dashboard com indicadores.
- Sessão persistente no aplicativo MAUI.
- Logout e tratamento de sessão expirada.

## Regras de negócio

- Código de produto único.
- E-mail de usuário único.
- Produtos cadastrados com estoque inicial zero.
- Alteração do estoque por movimentações de entrada e saída.
- Quantidade da movimentação maior que zero.
- Bloqueio de saída superior ao saldo disponível.
- Registro das movimentações válidas no histórico.
- Bloqueio de exclusão de produtos com movimentações.
- Bloqueio de exclusão de categorias com produtos.
- Proteção contra remoção do último administrador.
- Autenticação obrigatória nos recursos protegidos.

## Arquitetura

A aplicação Web e o aplicativo MAUI utilizam a mesma API:

- TechStore.Web → TechStore.API → SQLite.
- TechStore.Mobile no Windows → TechStore.API → SQLite.

A API concentra as regras de negócio e o acesso aos dados.

As interfaces consultam e atualizam as informações por requisições HTTP.

## Banco de dados

O projeto utiliza SQLite com Entity Framework Core e migrations.

Principais entidades:

- Product
- Category
- StockMovement
- User

Relacionamentos:

- Uma categoria pode possuir vários produtos.
- Um produto pode ter uma categoria.
- Um produto pode possuir várias movimentações.
- Cada movimentação pertence a um produto.

A API aplica as migrations durante a inicialização.

### Triggers de proteção do estoque

Foram implementadas duas triggers na tabela Products:

- `TR_Products_PreventNegativeStock_Insert`: impede a inserção de produtos com quantidade negativa.
- `TR_Products_PreventNegativeStock_Update`: impede a atualização da quantidade para um valor negativo.

As triggers funcionam como proteção adicional no banco. A atualização do saldo e o registro das movimentações são realizados pela API.

O script SQL está disponível em:

```text
database/TechStore.sql
```

## Segurança

A autenticação utiliza JWT com validação de:

- Emissor.
- Destinatário.
- Validade.
- Assinatura.

As senhas são armazenadas com hash por meio do PasswordHasher.

A chave JWT e as credenciais do primeiro administrador são configuradas localmente usando .NET User Secrets no ambiente de desenvolvimento.

## Pré-requisitos

- SDK do .NET 10.
- Git.
- Windows com as ferramentas do .NET MAUI instaladas para executar o aplicativo TechStore.Mobile.
- Visual Studio Code ou outro editor compatível com .NET.

## Obtendo o projeto

No terminal:

```powershell
git clone https://github.com/andersonvazvieira/TechStore.git
cd TechStore
```

## Configuração inicial da API

Na pasta principal do projeto:

```powershell
cd TechStore.API
```

### Configurar a chave JWT

Defina uma chave aleatória com pelo menos 32 bytes.

Substitua o marcador abaixo pela sua própria chave:

```powershell
dotnet user-secrets set "Jwt:Key" "SUBSTITUA-POR-SUA-CHAVE-ALEATORIA"
```

### Configurar o primeiro administrador

Configure nome e e-mail:

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

Quando não existe administrador, a API cria a primeira conta com essas configurações.

Se já existe um administrador, as contas e senhas existentes são preservadas. Alterar a configuração não redefine a senha de uma conta existente.

Se o e-mail configurado já pertencer a um usuário sem perfil Admin, configure outro e-mail para o primeiro administrador.

## Executando a API

A partir da pasta principal do projeto:

```powershell
cd TechStore.API
dotnet run
```

No perfil HTTP de desenvolvimento, a API utiliza:

```text
http://localhost:5221
```

Confira o endereço exibido no terminal.

Mantenha a API em execução enquanto utiliza a aplicação Web ou o aplicativo MAUI.

## Executando a aplicação Web

Com a API em execução, abra outro terminal na pasta principal do projeto:

```powershell
cd TechStore.Web
dotnet run
```

Abra no navegador o endereço exibido no terminal da aplicação Web.

Utilize o e-mail e a senha do administrador configurado ou de um usuário já cadastrado.

## Executando o aplicativo MAUI no Windows

Com a API em execução, abra outro terminal na pasta principal do projeto:

```powershell
cd TechStore.Mobile
dotnet run -f net10.0-windows10.0.19041.0
```

O projeto TechStore.Mobile está configurado para execução no Windows.

Utilize as mesmas credenciais da aplicação Web.

## Testes automatizados

Na pasta principal do projeto:

```powershell
dotnet test .\TechStore.Tests\TechStore.Tests.csproj
```

A suíte possui 17 casos automatizados com xUnit e SQLite temporário em memória, independente do banco da aplicação.

Os testes verificam:

- Entrada com atualização do saldo e registro da movimentação.
- Saída com atualização do saldo e registro da movimentação.
- Saída igual ao saldo disponível.
- Rejeição de saída superior ao saldo.
- Rejeição de quantidade zero ou negativa.
- Rejeição de tipos de movimentação inválidos.
- Rejeição de produto inexistente.
- Normalização do tipo da movimentação.
- Bloqueio de inserção com estoque negativo pela trigger.
- Bloqueio de atualização para estoque negativo pela trigger.
- Aceitação de estoque zero ou positivo.

### Gerando relatório dos testes

Na pasta principal do projeto:

```powershell
dotnet test .\TechStore.Tests\TechStore.Tests.csproj --logger "trx;LogFileName=estoque.trx" --results-directory .\evidencias\testes
```

O relatório será salvo em:

```text
evidencias/testes/estoque.trx
```

## Integração contínua

O GitHub Actions executa o pipeline em pushes e pull requests destinados à branch main. Também permite execução manual.

O processo:

1. Obtém o código do repositório.
2. Configura o SDK do .NET 10.
3. Restaura as dependências.
4. Compila a API e o projeto de testes.
5. Executa os testes automatizados.
6. Disponibiliza o relatório TRX.
7. Constrói a imagem Docker da API.
8. Inicializa um contêiner com configurações temporárias.
9. Verifica uma rota protegida.
10. Testa o login do administrador e a emissão do token.

As execuções podem ser acompanhadas na aba Actions do repositório.

## Docker

A API possui um Dockerfile com etapas de compilação e execução.

A imagem é construída e sua inicialização é verificada pelo pipeline do GitHub Actions.

Com Docker instalado, a imagem também pode ser construída a partir da pasta principal:

```powershell
docker build -f TechStore.API/Dockerfile -t techstore-api .
```

A execução do contêiner exige as configurações de banco, JWT e primeiro administrador apropriadas ao ambiente.

## Acessibilidade

Foram implementadas melhorias no layout, no login e nas telas de produtos da aplicação Web, incluindo:

- Rótulos associados aos campos.
- Indicação visual de foco.
- Atalho para o conteúdo principal.
- Mensagens de validação vinculadas aos campos.
- Identificação de cabeçalhos nas tabelas.
- Identificação dos produtos nos links de edição e exclusão.

## Validações realizadas

- Execução da API.
- Execução da aplicação Web.
- Execução do aplicativo MAUI no Windows.
- Integração das interfaces com a API.
- Autenticação e controle de acesso.
- Criação do primeiro administrador.
- Entradas, saídas e histórico de estoque.
- Consulta de produtos e estoque baixo.
- Persistência de sessão e logout no MAUI.
- Aprovação dos 17 testes automatizados.
- Execução do pipeline de integração contínua.
- Construção e inicialização da API em contêiner Docker.

## Repositório

https://github.com/andersonvazvieira/TechStore
# TechStore

Sistema de gerenciamento de estoque desenvolvido com .NET.

## Sobre o projeto

O TechStore é uma aplicação para gerenciamento de produtos e estoque, composta por API REST, aplicação Web e aplicação Mobile.

O sistema permite controlar produtos, categorias, usuários, entradas e saídas de estoque, movimentações e produtos com estoque baixo.

## Tecnologias

- C#
- .NET 10
- ASP.NET Core
- ASP.NET Core MVC
- REST API
- Entity Framework Core
- SQLite
- JWT
- .NET MAUI
- HTML/CSS
- Bootstrap

## Estrutura

### TechStore.API

API responsável pelas regras de negócio, autenticação e acesso ao banco de dados.

### TechStore.Web

Aplicação Web MVC que consome a TechStore.API.

### TechStore.Mobile

Aplicação desenvolvida com .NET MAUI que também consome a TechStore.API.

## Funcionalidades

- Autenticação com JWT
- Controle de acesso por perfil
- Administrador e usuário
- Cadastro de produtos
- Cadastro de categorias
- Controle de estoque
- Entrada de produtos
- Saída de produtos
- Bloqueio de estoque negativo
- Histórico de movimentações
- Produtos com estoque baixo
- Dashboard
- Pesquisa de produtos
- Gerenciamento de usuários pelo administrador
- Sessão persistente no aplicativo Mobile
- Logout
- Tratamento de sessão expirada

## Arquitetura

Web e Mobile utilizam a mesma API:

TechStore.Web → TechStore.API → SQLite

TechStore.Mobile → TechStore.API → SQLite

Dessa forma, as informações permanecem centralizadas e sincronizadas.

## Banco de dados

O projeto utiliza SQLite com Entity Framework Core e migrations.

Principais entidades:

- Product
- Category
- StockMovement
- User

## Segurança

A autenticação utiliza JWT.

As senhas dos usuários são armazenadas utilizando hash.

A chave JWT de desenvolvimento não é armazenada no repositório e deve ser configurada localmente utilizando .NET User Secrets.

Exemplo:

dotnet user-secrets set "Jwt:Key" "SUA-CHAVE-JWT"

## Executando a API

Entre em:

TechStore.API

Execute:

dotnet run

Por padrão, durante o desenvolvimento, a API utiliza:

http://localhost:5221

## Executando o Web

Entre em:

TechStore.Web

Execute:

dotnet run

## Executando o Mobile no Windows

Entre em:

TechStore.Mobile

Execute:

dotnet run -f net10.0-windows10.0.19041.0

## Android

O projeto possui target Android por meio do .NET MAUI.

Para compilar e executar no Android é necessário ter Android SDK/JDK e ambiente Android devidamente configurados.

## Status

API: concluída.

Web: concluído.

Mobile Windows: concluído e integrado à API.

Android: pendente de configuração do ambiente Android para compilação e testes.
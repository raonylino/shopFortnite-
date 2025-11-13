# Guia de Uso - Razor Pages Frontend

## Visão Geral

O projeto agora possui um **frontend completo com Razor Pages** que consome a API REST interna. O sistema mantém **Swagger acessível** para desenvolvimento e testes da API.

## URLs Disponíveis

- **http://localhost:5106/** - Home do site (Razor Pages)
- **http://localhost:5106/swagger** - Documentação Swagger da API
- **http://localhost:5106/api/...** - Endpoints REST da API

## Páginas Disponíveis

### 1. Home (`/`)
- Página inicial do site
- Mostra status de autenticação
- Exibe saldo de V-Bucks do usuário logado
- Lista as principais funcionalidades

### 2. Conta

#### `/Account/Login`
- Formulário de login (email + senha)
- Salva JWT token em cookie HttpOnly seguro
- Redireciona para a página de origem após login

#### `/Account/Register`
- Formulário de cadastro (email + senha + confirmação)
- **Bônus**: 10.000 V-Bucks para novos usuários
- Faz login automático após cadastro

#### `/Account/Logout`
- Limpa todos os cookies de autenticação
- Redireciona para home

### 3. Cosméticos

#### `/Cosmetics`
- **Listagem paginada** de todos os cosméticos
- **Filtros disponíveis**:
  - Nome (busca textual)
  - Tipo (Outfit, Glider, Pickaxe, Emote)
  - Raridade (Common, Uncommon, Rare, Epic, Legendary)
  - Status (À venda / Indisponível)
  - Novos (checkbox)
- **Paginação**: 20 itens por página
- Cards coloridos por raridade
- Botão "Detalhes" em cada cosmético

#### `/Cosmetics/Details/{id}`
- Imagem grande do cosmético
- Informações completas (nome, tipo, raridade, preço)
- **Funcionalidades autenticadas**:
  - Mostra saldo atual do usuário
  - Botão **"Comprar Agora"** (se não possui e tem saldo)
  - Botão **"Devolver"** (se já possui, com reembolso de 80%)
  - Mensagens de sucesso/erro
- Atualiza saldo nos cookies automaticamente

### 4. Perfil (`/Profile`)
- **Requer autenticação**
- **Informações do usuário**:
  - Email e ID
  - Saldo de V-Bucks
- **Meus Cosméticos**:
  - Grid com todos os cosméticos adquiridos
  - Imagens com borda colorida por raridade
- **Histórico de Transações**:
  - Últimas 20 transações
  - Data, tipo (compra/devolução), valor, saldo após

### 5. Usuários (`/Users`)
- Lista **todos os usuários** cadastrados
- Para cada usuário mostra:
  - Email e saldo
  - Grid com até 6 cosméticos (miniaturas)
  - Data de cadastro
- **Público** (não requer autenticação)

## Autenticação

### Sistema de Cookies
- **JWT Token**: Armazenado em cookie HttpOnly
- **Dados do usuário**: UserId, UserEmail, Vbucks
- **Segurança**: HttpOnly, Secure, SameSite=Strict
- **Expiração**: 24 horas

### Proteção de Rotas
As páginas que requerem autenticação redirecionam para `/Account/Login` com `returnUrl`.

## Consumo da API

### ApiClientService
Todas as páginas usam `IApiClientService` que:
- Consome os endpoints REST via `HttpClientFactory`
- Adiciona token JWT automaticamente nos requests
- Trata erros e exceções
- Serializa/deserializa DTOs com JSON

### Endpoints Consumidos
```csharp
// Autenticação
POST /api/auth/register
POST /api/auth/login

// Cosméticos
GET  /api/cosmetics?page=1&pageSize=20&name=...&type=...
GET  /api/cosmetics/{id}

// Usuários
GET  /api/users
GET  /api/users/{id}
GET  /api/users/{userId}/cosmetics
GET  /api/users/{userId}/transactions

// Compra/Devolução
POST /api/users/{userId}/cosmetics/purchase
POST /api/users/{userId}/cosmetics/return
```

## Estilização

### Bootstrap 5
- Layout responsivo
- Componentes: cards, forms, badges, tables
- Navbar com links dinâmicos (muda com autenticação)

### Cores por Raridade
```css
Common    - Cinza   (#b0b0b0)
Uncommon  - Verde   (#5cb85c)
Rare      - Azul    (#5bc0de)
Epic      - Roxo    (#9b59b6)
Legendary - Dourado (#f39c12)
```

### Ícones Bootstrap Icons
- `bi-shop` - Logo da loja
- `bi-bag` - Cosméticos
- `bi-person-circle` - Perfil
- `bi-wallet2` - V-Bucks
- `bi-cart-plus` - Comprar
- `bi-arrow-counterclockwise` - Devolver

## Fluxo de Uso Típico

1. **Primeiro acesso**:
   - Acesse `/Account/Register`
   - Crie uma conta (receba 10k V-Bucks)
   - Seja redirecionado para home logado

2. **Explorar cosméticos**:
   - Vá para `/Cosmetics`
   - Use os filtros (ex: Type=Outfit, Rarity=Legendary)
   - Clique em "Detalhes" em um cosmético

3. **Comprar cosmético**:
   - Na página de detalhes, clique "Comprar Agora"
   - Receba mensagem de sucesso
   - Veja saldo atualizado

4. **Ver perfil**:
   - Acesse `/Profile`
   - Veja seus cosméticos adquiridos
   - Confira histórico de transações

5. **Devolver cosmético**:
   - Em `/Cosmetics/Details/{id}` de um item que você possui
   - Clique "Devolver" (receberá 80% de volta)

6. **Ver outros usuários**:
   - Acesse `/Users`
   - Veja o que outros usuários compraram

## Desenvolvimento

### Adicionar nova página
1. Crie `Pages/NomeDaPagina.cshtml` (view)
2. Crie `Pages/NomeDaPagina.cshtml.cs` (PageModel)
3. Injete `IApiClientService` no construtor
4. Use métodos do service para chamar API
5. Adicione link na navbar em `_Layout.cshtml`

### Chamar novo endpoint da API
1. Adicione método na interface `IApiClientService`
2. Implemente em `ApiClientService.cs`
3. Use `GetAsync` ou `PostAsync<TRequest, TResponse>`
4. Chame do PageModel da Razor Page

## Observações Importantes

- **Sincronização Fortnite**: Ocorre automaticamente a cada 6 horas
- **Saldo inicial**: Novos usuários recebem 10.000 V-Bucks
- **Reembolso**: Devoluções retornam 80% do preço pago
- **Validações**: Implementadas tanto no frontend quanto na API
- **HTTPS**: Configure certificado para produção (atualmente HTTP)

## Testando Localmente

```powershell
cd "c:\Users\Raony\Documents\ShopFortnite"
dotnet run --project ShopFortnite

# Acesse no navegador:
# http://localhost:5106/          - Site
# http://localhost:5106/swagger   - API Docs
```

## Arquitetura

```
Browser
  ↓ HTTP Request
Razor Pages (Frontend)
  ↓ via ApiClientService
REST API Controllers
  ↓ via UseCases
Application Layer (Business Logic)
  ↓ via Repositories
Infrastructure Layer (EF Core)
  ↓
SQLite Database
```

## Benefícios desta Abordagem

✅ **Separação de responsabilidades**: Frontend e Backend desacoplados
✅ **Testável**: Pode testar API via Swagger e frontend separadamente
✅ **Escalável**: Pode trocar frontend ou adicionar mobile app consumindo mesma API
✅ **Seguro**: JWT em cookies HttpOnly, validações em ambas camadas
✅ **Profissional**: Clean Architecture + DDD + MVVM + DTOs

---

**Autor**: GitHub Copilot
**Data**: 2024
**Versão**: ASP.NET Core 8.0

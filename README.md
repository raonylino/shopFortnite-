# ShopFortnite

API para gerenciamento de cosméticos do Fortnite com arquitetura limpa (Clean Architecture + DDD).

## 🏗️ Arquitetura

O projeto segue os princípios de Clean Architecture, DDD (Domain-Driven Design) e MVVM, organizado nas seguintes camadas:

- **Domain**: Entidades, agregados, value objects e interfaces de repositório
- **Application**: DTOs, casos de uso, serviços, AutoMapper profiles e validadores FluentValidation
- **Infrastructure**: Implementação de repositórios, DbContext EF Core, serviços externos e migrations
- **WebApi**: Controllers, middlewares, autenticação JWT e configuração Swagger

## ✨ Funcionalidades

### 🔐 Autenticação
- Registro de usuários com 10.000 v-bucks iniciais
- Login com JWT (JSON Web Token)
- Endpoints: `POST /api/auth/register` e `POST /api/auth/login`

### 🎮 Sincronização Fortnite
- Sincronização automática a cada 6 horas com a API Fortnite
- Endpoints consumidos:
  - `https://fortnite-api.com/v2/cosmetics/br`
  - `https://fortnite-api.com/v2/cosmetics/new`
  - `https://fortnite-api.com/v2/shop`

### 🛒 Compra e Devolução
- `POST /api/cosmetics/{id}/purchase` - Comprar cosmético
- `POST /api/cosmetics/{id}/return` - Devolver cosmético
- Validação de saldo, disponibilidade e compras duplicadas

### 📊 Consultas
- `GET /api/cosmetics` - Listar cosméticos (paginado com filtros)
  - Filtros: nome, tipo, raridade, novos, à venda, data
- `GET /api/cosmetics/{id}` - Detalhes de um cosmético
- `GET /api/users` - Lista pública de usuários
- `GET /api/users/{id}` - Cosméticos de um usuário específico

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- Docker (opcional)

### Executar Localmente

```bash
# Restaurar dependências
dotnet restore

# Executar migrations
cd ShopFortnite
dotnet ef database update

# Executar o projeto
dotnet run
```

Acesse:
- API: `https://localhost:5106`
- Swagger: `https://localhost:5106/swagger`

### Executar com Docker

```bash
# Build e executar
docker-compose up -d

# Parar
docker-compose down
```

Acesse:
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

## 📦 Tecnologias

- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM
- **SQLite** - Banco de dados
- **AutoMapper** - Mapeamento objeto-objeto
- **FluentValidation** - Validação de dados
- **BCrypt.Net** - Hash de senhas
- **Swashbuckle** - Documentação Swagger/OpenAPI
- **xUnit + Moq** - Testes unitários
- **JWT** - Autenticação

## 🧪 Testes

```bash
# Executar testes
dotnet test
```

Os testes cobrem:
- Autenticação (registro e login)
- Compra e devolução de cosméticos
- Sincronização com API Fortnite (mockada)

## 📝 Modelos de Dados

### User
- Id, Email, PasswordHash, Vbucks, CreatedAt

### Cosmetic
- Id, ExternalId, Name, Type, Rarity, Price, ImageUrl, IsNew, IsForSale, AddedDate

### UserCosmetic
- UserId, CosmeticId, PurchaseDate, ReturnedDate, PriceAtPurchase

### Transaction
- Id, UserId, CosmeticId, Type (Purchase/Return), Amount, Date

## 🔑 Configuração JWT

Configure as seguintes variáveis no `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "sua-chave-secreta-aqui-minimo-32-caracteres",
    "Issuer": "ShopFortnite",
    "Audience": "ShopFortniteUsers"
  }
}
```

## 📄 Licença

Este projeto é livre para uso educacional e demonstração.

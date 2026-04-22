# Good Hamburger — Sistema de Pedidos

API REST para registro e gerenciamento de pedidos da lanchonete Good Hamburger, construída com .NET 8 e ASP.NET Core.

---

## Como executar

**Pré-requisitos:** .NET 8 SDK

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd GoodHamburger

# Restaurar dependências
dotnet restore

# Rodar a API
dotnet run --project GoodHamburger.Api

# A API estará disponível em:
# http://localhost:5251
# Swagger UI: http://localhost:5251/swagger
```

**Rodar os testes:**

```bash
dotnet test
```

---

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/menu` | Lista o cardápio completo |
| `GET` | `/api/orders` | Lista todos os pedidos |
| `GET` | `/api/orders/{id}` | Busca pedido por ID (GUID) |
| `POST` | `/api/orders` | Cria um novo pedido |
| `PUT` | `/api/orders/{id}` | Atualiza um pedido existente |
| `DELETE` | `/api/orders/{id}` | Remove um pedido |

### GET /api/menu

```json
[
  { "id": "x-burger", "name": "X Burger",     "price": 5.00, "category": "Sanduíche"      },
  { "id": "x-egg",    "name": "X Egg",        "price": 4.50, "category": "Sanduíche"      },
  { "id": "x-bacon",  "name": "X Bacon",      "price": 7.00, "category": "Sanduíche"      },
  { "id": "fries",    "name": "Batata Frita", "price": 2.00, "category": "Acompanhamento" },
  { "id": "soda",     "name": "Refrigerante", "price": 2.50, "category": "Acompanhamento" }
]
```

### POST /api/orders — criar pedido

**Request:**
```json
{
  "itemIds": ["x-burger", "fries", "soda"]
}
```

**Response `201 Created`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    { "menuItemId": "x-burger", "name": "X Burger",    "unitPrice": 5.00 },
    { "menuItemId": "fries",    "name": "Batata Frita", "unitPrice": 2.00 },
    { "menuItemId": "soda",     "name": "Refrigerante", "unitPrice": 2.50 }
  ],
  "subtotal": 9.50,
  "discountPercent": 20,
  "discountAmount": 1.90,
  "total": 7.60,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

### PUT /api/orders/{id} — atualizar pedido

Substitui completamente os itens do pedido e recalcula desconto/total.

**Request:** mesmo formato do `POST`.

### Respostas de erro

| Situação | Status | Exemplo de mensagem |
|---|---|---|
| Pedido não encontrado | `404 Not Found` | `"Pedido {id} não encontrado."` |
| Item duplicado no pedido | `400 Bad Request` | `"O pedido contém itens duplicados."` |
| Dois sanduíches no pedido | `400 Bad Request` | `"O pedido só pode ter um sanduíche."` |
| Pedido sem sanduíche | `400 Bad Request` | `"O pedido deve ter pelo menos um sanduíche."` |
| ID de item inválido | `400 Bad Request` | `"Item '{id}' não encontrado no cardápio."` |

---

## Regras de negócio

- Todo pedido deve ter **exatamente um** sanduíche (obrigatório)
- Opcionalmente pode ter **uma** batata frita e/ou **um** refrigerante
- Itens duplicados resultam em `400 Bad Request`

### Tabela de descontos

| Combinação | Desconto |
|---|---|
| Sanduíche + Batata + Refrigerante | 20% |
| Sanduíche + Refrigerante | 15% |
| Sanduíche + Batata | 10% |
| Apenas Sanduíche | 0% |

O desconto é aplicado sobre o subtotal. Valores são arredondados com `Math.Round(..., 2)` (arredondamento bancário padrão do .NET — `MidpointRounding.ToEven`).

**Exemplo de arredondamento:** X Bacon (R$ 7,00) + Refrigerante (R$ 2,50) = R$ 9,50. 15% de R$ 9,50 = R$ 1,425 → arredonda para R$ 1,42 → total R$ 8,08.

---

## Arquitetura e decisões técnicas

### Estrutura de pastas

```
GoodHamburger/
├── GoodHamburger.Api/
│   ├── Controllers/      # Camada HTTP — recebe requisições, mapeia erros para status codes
│   ├── Domain/           # Entidades, enums e catálogo de itens
│   ├── DTOs/             # Contratos de entrada e saída da API
│   ├── Repositories/     # Abstração de persistência (interface + implementação em memória)
│   └── Services/         # Regras de negócio (OrderService + DiscountCalculator)
└── GoodHamburger.Tests/  # Testes unitários (xUnit + FluentAssertions)
```

### Arquitetura em camadas (Controller → Service → Repository)

Optei por uma arquitetura em camadas simples e sem over-engineering. O fluxo é direto: o controller recebe a requisição HTTP, delega ao serviço, que aplica as regras de negócio e acessa o repositório. Para o escopo do desafio, isso é suficiente e fácil de navegar. Adicionar CQRS, MediatR ou qualquer outro pattern de indireção seria complexidade desnecessária — aqui não há múltiplos handlers para o mesmo caso de uso, nem pipeline behaviors.

### DiscountCalculator separado do OrderService

A lógica de desconto ficou em uma classe estática `DiscountCalculator` em vez de dentro do `OrderService`. A motivação é testabilidade: consigo testar todas as combinações de desconto de forma isolada, sem precisar instanciar o serviço ou mockar o repositório. O `DiscountCalculator` recebe um `Order` e muta os campos calculados diretamente — simples e direto.

### Validações via exceções tipadas

Em vez de um `Result<T>` pattern ou FluentValidation, optei por lançar exceções com mensagens claras (`KeyNotFoundException`, `InvalidOperationException`, `ArgumentException`) e capturá-las nos controllers com `try/catch` por tipo. Para uma API desta complexidade, essa abordagem é pragmática e legível. Um middleware global de exceções seria o próximo passo natural se o projeto crescesse.

### Repositório em memória com `ConcurrentDictionary`

Optei por `InMemoryOrderRepository` com `ConcurrentDictionary` ao invés de banco de dados. O enunciado não especifica persistência, e introduzir EF Core + banco de dados aumentaria o setup sem agregar nada às regras de negócio que precisavam ser demonstradas. O `ConcurrentDictionary` garante thread safety para requisições paralelas sem lock explícito. A interface `IOrderRepository` está desacoplada da implementação — trocar por EF Core seria uma questão de criar uma nova classe que implemente a mesma interface, sem tocar em nada além do `Program.cs`.

### Primary constructors (C# 12)

Controllers e serviços usam primary constructors para injeção de dependência. Reduz verbosidade sem perda de legibilidade — em vez de declarar campo privado + construtor + atribuição, tudo fica em uma linha na assinatura da classe.

### Injeção de dependência

- `IOrderRepository` registrado como `Singleton` — faz sentido para repositório em memória, já que o estado precisa persistir enquanto a aplicação estiver rodando.
- `IOrderService` registrado como `Scoped` — ciclo de vida padrão para serviços que dependem de outros componentes.

### Swagger habilitado em todos os ambientes

Para um sistema interno de pedidos, deixar o Swagger ativo facilita testes rápidos sem precisar de Postman ou outro cliente. Em produção isso poderia ser restrito, mas não era o foco aqui.

### CORS aberto (`AllowAnyOrigin`)

Habilitado para permitir consumo da API por qualquer cliente frontend (Blazor, React, etc.) durante o desenvolvimento, sem configuração adicional. Em produção, seria substituído por uma política restrita com origins específicos.

---

## O que ficou fora

- **Frontend Blazor** — opcional no enunciado. A API está completamente documentada via Swagger e o arquivo `.http` contém todos os exemplos de chamada. A interface pode ser adicionada depois consumindo os endpoints existentes sem modificações.
- **Banco de dados** — a troca do repositório em memória por EF Core + SQLite seria uma nova implementação de `IOrderRepository` (~40 linhas) e o registro no `Program.cs`. As regras de negócio e a API não seriam tocadas.
- **Autenticação/autorização** — não mencionado no escopo.
- **Paginação** no `GET /orders` — irrelevante para o volume esperado em um sistema de lanchonete local.
- **Integração tests (WebApplicationFactory)** — teria cobertura do pipeline HTTP completo, mas os 18 testes unitários já cobrem todos os cenários de negócio.

---

## Testes

18 testes cobrindo:

**`DiscountRulesTests` (6 testes):**
- Todas as combinações de desconto: 20%, 15%, 10% e 0%
- Verificação dos valores calculados com os preços reais do cardápio
- Caso de arredondamento: X Bacon + Refrigerante com desconto de 15%

**`OrderServiceTests` (12 testes):**
- Criação de pedido válido retorna pedido com ID e total correto
- Erro ao criar com item duplicado
- Erro ao criar com ID de item desconhecido
- Erro ao criar com dois sanduíches
- Erro ao criar pedido sem sanduíche
- `GetById` retorna pedido existente
- `GetById` lança exceção para ID inexistente
- `GetAll` retorna todos os pedidos criados
- `Update` recalcula desconto corretamente e preenche `UpdatedAt`
- `Update` lança exceção para ID inexistente
- `Delete` remove o pedido com sucesso
- `Delete` lança exceção para ID inexistente

```bash
dotnet test
# 18 passed, 0 failed
```

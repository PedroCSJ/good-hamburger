# Good Hamburger

API de pedidos para a lanchonete Good Hamburger. .NET 8 + ASP.NET Core.

## Rodando o projeto

Precisa ter o .NET 8 SDK instalado.

```bash
git clone <url-do-repositorio>
cd GoodHamburger
dotnet run --project GoodHamburger.Api
```

Sobe em `http://localhost:5251`. O Swagger fica em `http://localhost:5251/swagger`.

```bash
# testes
dotnet test
```

## Endpoints

| Método | Rota | |
|---|---|---|
| GET | `/api/menu` | cardápio |
| GET | `/api/orders` | lista pedidos |
| GET | `/api/orders/{id}` | pedido por id |
| POST | `/api/orders` | cria pedido |
| PUT | `/api/orders/{id}` | atualiza pedido |
| DELETE | `/api/orders/{id}` | remove pedido |

### Criar pedido

```json
POST /api/orders
{
  "itemIds": ["x-burger", "fries", "soda"]
}
```

Resposta `201`:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    { "menuItemId": "x-burger", "name": "X Burger", "unitPrice": 5.00 },
    { "menuItemId": "fries", "name": "Batata Frita", "unitPrice": 2.00 },
    { "menuItemId": "soda", "name": "Refrigerante", "unitPrice": 2.50 }
  ],
  "subtotal": 9.50,
  "discountPercent": 20,
  "discountAmount": 1.90,
  "total": 7.60,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

IDs disponíveis: `x-burger`, `x-egg`, `x-bacon`, `fries`, `soda`.

O `PUT` tem o mesmo formato do `POST` e substitui os itens do pedido por completo.

### Erros

- `404` — pedido não encontrado
- `400` — item duplicado, dois sanduíches, pedido sem sanduíche, ID de item inválido

## Regras de negócio

Todo pedido precisa ter exatamente um sanduíche. Batata e refrigerante são opcionais, no máximo um de cada.

| Combinação | Desconto |
|---|---|
| Sanduíche + Batata + Refrigerante | 20% |
| Sanduíche + Refrigerante | 15% |
| Sanduíche + Batata | 10% |
| Só sanduíche | 0% |

Arredondamento com `Math.Round(..., 2)`. Exemplo: X Bacon + Refrigerante = R$9,50, 15% = R$1,425 → R$1,42, total R$8,08.

## Decisões técnicas

Arquitetura em camadas simples: Controller → Service → Repository. Não usei MediatR nem CQRS porque não faz sentido para o tamanho do projeto — seria só indireção desnecessária.

A lógica de desconto ficou numa classe estática `DiscountCalculator` separada do `OrderService` para conseguir testar as regras de desconto sem precisar instanciar o serviço inteiro.

Usei repositório em memória com `ConcurrentDictionary` em vez de banco de dados. O enunciado não pede persistência e EF Core aqui seria setup pra nada. A interface `IOrderRepository` está desacoplada — pra trocar por banco de dados é só criar uma nova implementação e registrar no `Program.cs`.

Erros são lançados como exceções tipadas no serviço e capturados nos controllers. Para esse tamanho de projeto é mais simples do que `Result<T>` ou FluentValidation.

## O que ficou fora

- **Blazor** — opcional, a API está funcional e documentada via Swagger
- **Banco de dados** — trocar o repositório em memória por EF Core não mexe em nada além do `Program.cs`
- **Autenticação** — fora do escopo
- **Testes de integração** — os 18 testes unitários cobrem todos os cenários de negócio

## Testes

18 testes no total. `DiscountRulesTests` cobre as 4 combinações de desconto e os cálculos com os valores reais do cardápio. `OrderServiceTests` cobre o CRUD completo e todos os casos de erro.

using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Api.DTOs;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "A lista de itens é obrigatória.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
    public List<string> ItemIds { get; set; } = [];
}

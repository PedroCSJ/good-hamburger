using GoodHamburger.Api.Domain;
using GoodHamburger.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuItemResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var items = MenuCatalog.Items.Select(i => new MenuItemResponse
        {
            Id       = i.Id,
            Name     = i.Name,
            Price    = i.Price,
            Category = i.Type switch
            {
                MenuItemType.Sandwich   => "Sanduíche",
                MenuItemType.FrenchFries => "Acompanhamento",
                MenuItemType.Soda       => "Acompanhamento",
                _                       => "Outro"
            }
        });

        return Ok(items);
    }
}

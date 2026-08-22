using Cart.Api.Models;
using Cart.Api.Services;
using ECommerce.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken ct) =>
        Ok(await cartService.GetCartAsync(User.GetUserId(), ct));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct) =>
        Ok(await cartService.AddItemAsync(User.GetUserId(), request, ct));

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid productId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var cart = await cartService.UpdateItemAsync(User.GetUserId(), productId, request, ct);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct) =>
        Ok(await cartService.RemoveItemAsync(User.GetUserId(), productId, ct));
}

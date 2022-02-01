using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ShoppingController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public ShoppingController(IOrderService orderService, IBasketService basketService, ICatalogService catalogService)
        {
            _orderService = orderService;
            _basketService = basketService;
            _catalogService = catalogService;
        }

        [HttpGet("{userName}",Name ="GetShopping")]
        public async Task<ActionResult<ShoppingModel>> GetShopping(string userName)
        {
            //getbasket with username
            var basket = await _basketService.GetBasket(userName);

            //iterate basket items and consume prroducts item product Id
            foreach (var item in basket.Items)
            {
                var product = await _catalogService.GetCatalog(item.ProductId);
                // set additional fields
                item.ProductName = product.Name;
                item.Category = product.Category;
                item.Summary = product.Summary;
                item.Description = product.Description;
                item.ImageFile = product.ImageFile;
            }

            var orders = await _orderService.GetOrdersByUserName(userName);
            // Map product related members into basket item dto extended columns
            var shoppingModel = new ShoppingModel
            {
                UserName = userName,
                BasketWithProducts = basket,
                Orders = orders
            };
            

            return Ok(shoppingModel);
        }
    }
}

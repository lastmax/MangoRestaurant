using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var productList = new List<ProductDTO>();
            var response = await _productService.GetAllProductsAsync<ResponseDTO>("");
            if (response != null && response.IsSuccess)
                productList = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
            
            return View(productList);
        }

        [Authorize]
        public async Task<IActionResult> Details(int productId)
        {
            var product = new ProductDTO();
            var response = await _productService.GetProductByIdAsync<ResponseDTO>(productId, "");
            if (response != null && response.IsSuccess)
                product = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));

            return View(product);
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductDTO productDTO)
        {
            CartDTO cartDTO = new()
            {
                CartHeader = new CartHeaderDTO
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }
            };

            CartDetailsDTO cartDetails = new()
            {
                Count = productDTO.Count,
                ProductId = productDTO.ProductId
            };

            var resp = await _productService.GetProductByIdAsync<ResponseDTO>(productDTO.ProductId, "");
            if (resp != null && resp.IsSuccess)
                cartDetails.Product = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(resp.Result));

            cartDTO.CartDetails = new List<CartDetailsDTO> { cartDetails };
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var addToCartResp = await _cartService.AddToCartAsync<ResponseDTO>(cartDTO, accessToken);

            if (addToCartResp != null && addToCartResp.IsSuccess)
                return RedirectToAction(nameof(Index));

            return View(productDTO);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}
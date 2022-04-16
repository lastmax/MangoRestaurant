using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            var list = new List<ProductDTO>();
            var response = await _productService.GetAllProductsAsync<ResponseDTO>();

            if(response != null && response.IsSuccess)
                list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));

            return View(list);
        }

        public async Task<IActionResult> CreateProduct()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProductAsync<ResponseDTO>(model);
                if (response != null && response.IsSuccess)
                    return RedirectToAction(nameof(ProductIndex));
            }

            return View(model);
        }

        public async Task<IActionResult> EditProduct(int productId)
        {
            var response = await _productService.GetProductByIdAsync<ResponseDTO>(productId);
            if (response != null && response.IsSuccess)
            {
                var model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.UpdateProductAsync<ResponseDTO>(model);
                if (response != null && response.IsSuccess)
                    return RedirectToAction(nameof(ProductIndex));
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var response = await _productService.GetProductByIdAsync<ResponseDTO>(productId);
            if (response != null && response.IsSuccess)
            {
                var model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(ProductDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.DeleteProductAsync<ResponseDTO>(model.ProductId);
                if (response != null && response.IsSuccess)
                    return RedirectToAction(nameof(ProductIndex));
            }

            return View(model);
        }
    }
}

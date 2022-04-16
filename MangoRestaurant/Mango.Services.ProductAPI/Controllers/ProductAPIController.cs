using Mango.Services.ProductAPI.Models.Dto;
using Mango.Services.ProductAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/products")]
    public class ProductAPIController : ControllerBase
    {
        protected ResponseDTO _response;
        private IProductRepository _productRepository;

        public ProductAPIController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            this._response = new ResponseDTO();
        }

        [HttpGet]
        public async Task<object> Get()
        {
            try
            {
                var productsDtos = await _productRepository.GetProducts();
                _response.Result = productsDtos;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<object> Get(int id)
        {
            try
            {
                var productDto = await _productRepository.GetProductById(id);
                _response.Result = productDto;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost]
        public async Task<object> Post([FromBody] ProductDTO productDTO)
        {
            try
            {
                var model = await _productRepository.CreateUpdateProduct(productDTO);
                _response.Result = model;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPut]
        public async Task<object> Put([FromBody] ProductDTO productDTO)
        {
            try
            {
                var model = await _productRepository.CreateUpdateProduct(productDTO);
                _response.Result = model;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<object> Put(int id)
        {
            try
            {
                var isSuccess = await _productRepository.Delete(id);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        private void HandleError(Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }
    }
}

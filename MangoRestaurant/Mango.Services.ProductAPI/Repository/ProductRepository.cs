﻿using AutoMapper;
using Mango.Services.ProductAPI.DbContexts;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public ProductRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductDTO> CreateUpdateProduct(ProductDTO productDto)
        {
            var product = _mapper.Map<ProductDTO, Product>(productDto);

            if (product.ProductId > 0)
                _db.Products.Update(product);
            else
                _db.Products.Add(product);

            await _db.SaveChangesAsync();
            return _mapper.Map<Product, ProductDTO>(product);
        }

        public async Task<bool> Delete(int productId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null) return false;

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ProductDTO> GetProductById(int productId)
        {
            var product = await _db.Products.Where(x => x.ProductId == productId).FirstOrDefaultAsync();
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<IEnumerable<ProductDTO>> GetProducts()
        {
            var productList = await _db.Products.ToListAsync();
            return _mapper.Map<List<ProductDTO>>(productList);
        }
    }
}

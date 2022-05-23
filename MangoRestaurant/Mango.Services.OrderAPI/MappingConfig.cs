using AutoMapper;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
               config.CreateMap<CartDetailsDTO, OrderDetails>()
                   .ForMember(d => d.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                   .ForMember(d => d.Price, opt => opt.MapFrom(src => src.Product.Price)
               ).ReverseMap();

               config.CreateMap<CheckoutHeaderDTO, OrderHeader>()
                   .ForMember(d => d.OrderTime, opt => opt.MapFrom(src => DateTime.Now))
                   .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(src => false))
                   .ForMember(d => d.CartTotalItems, opt => opt.MapFrom(src => src.CartDetails.Sum(p => p.Count)))
                   .ForMember(d => d.OrderDetails, opt => opt.MapFrom(
                       (src, dest, i, context) => context.Mapper.Map<List<OrderDetails>>(src.CartDetails))
                   ).ReverseMap();
            });

            return mappingConfig;
        }
    }
}

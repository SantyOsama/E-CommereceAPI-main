﻿using AutoMapper;
using TestToken.DTO;
using TestToken.DTO.CartDtos;
using TestToken.DTO.OrderDto;
using TestToken.DTO.PaymentDto;
using TestToken.DTO.ProductDto;
using TestToken.DTO.UserDtos;
using TestToken.DTO.WishlistDto;
using TestToken.Models;

namespace TestToken.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDto,ApplicationUser>().ReverseMap();
            CreateMap<userDto, ApplicationUser>().ReverseMap();
            CreateMap<Brand,BrandDto>().ReverseMap();
            CreateMap<Cart, CartDto>().ReverseMap();
            CreateMap<CartItem, CartItemDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Order,OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.Category, opt => opt.Ignore()) 
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<WishList,WishlistDto>().ReverseMap();
            CreateMap<WishlistItemsDto, WishListItem>().ReverseMap();
            CreateMap<Payment, PaymentRequestDto>().ReverseMap();
        }

    }
}

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestToken.Data;
using TestToken.DTO;
using TestToken.DTO.CartDtos;
using TestToken.Models;
using TestToken.Repositories.GenericRepository;
using TestToken.Repositories.Interfaces;

namespace TestToken.Repositories.Services
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartItemRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto> GetAllItems(int cartId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId)
                .AsNoTracking()
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new ResponseDto
                {
                    Message = "No items found for this cart.",
                    IsSucceeded = false,
                    StatusCode = 404,
                    model = new List<CartItemDto>()
                };
            }

            var dtoList = _mapper.Map<List<CartItemDto>>(cartItems);

            return new ResponseDto
            {
                IsSucceeded = true,
                StatusCode = 200,
                model = dtoList
            };
        }


        public async Task<ResponseDto> GetItemById(int id)
        {
            var existingItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (existingItem == null)
            {
                return new ResponseDto
                {
                    Message = "Item not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            var dto = _mapper.Map<CartItemDto>(existingItem);
            return new ResponseDto
            {
                IsSucceeded = true,
                StatusCode = 200,
                model = dto
            };
        }

        public async Task<ResponseDto> AddItem(CartItemDto item)
        {
            var cartExists = await _context.Carts.AnyAsync(c => c.Id == item.CartId);
            if (!cartExists)
            {
                return new ResponseDto
                {
                    Message = "Cart not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == item.ProductId);
            if (!productExists)
            {
                return new ResponseDto
                {
                    Message = "Product not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            if (item.quantity < 1)
            {
                return new ResponseDto
                {
                    Message = "Quantity must be at least 1",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            var addedItem = _mapper.Map<CartItem>(item);
            _context.CartItems.Add(addedItem);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<CartItemDto>(addedItem);
            return new ResponseDto
            {
                Message = "Item added to cart successfully",
                IsSucceeded = true,
                StatusCode = 201,
                model = dto
            };
        }

        public async Task<ResponseDto> UpdateItem(int id, CartItem cartItem)
        {
            var existingItem = await _context.CartItems.FindAsync(id);
            if (existingItem == null)
            {
                return new ResponseDto
                {
                    Message = "Item not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            if (cartItem.Quantity < 1)
            {
                return new ResponseDto
                {
                    Message = "Quantity must be at least 1",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // تحديث الحقول المراد تحديثها فقط
            existingItem.Quantity = cartItem.Quantity;
            existingItem.ProductId = cartItem.ProductId;
            existingItem.CartId = cartItem.CartId;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<CartItemDto>(existingItem);
            return new ResponseDto
            {
                Message = "Item updated successfully",
                IsSucceeded = true,
                StatusCode = 200,
                model = dto
            };
        }

        public async Task<ResponseDto> DeleteItem(int id)
        {
            var existingItem = await _context.CartItems.FindAsync(id);
            if (existingItem == null)
            {
                return new ResponseDto
                {
                    Message = "Item not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            _context.CartItems.Remove(existingItem);
            await _context.SaveChangesAsync();

            return new ResponseDto
            {
                Message = "Item deleted successfully",
                IsSucceeded = true,
                StatusCode = 200
            };
        }
    }
}

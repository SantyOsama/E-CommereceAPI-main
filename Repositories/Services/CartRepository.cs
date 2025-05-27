using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestToken.Data;
using TestToken.DTO;
using TestToken.DTO.CartDtos;
using TestToken.DTO.UserDtos;
using TestToken.Models;
using TestToken.Repositories.GenericRepository;
using TestToken.Repositories.Interfaces;

namespace TestToken.Repositories.Services
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper) : base(context)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto> AddCart(string customerId)
        {
            // تأكد من وجود المستخدم
            var userExists = await _context.Users.AnyAsync(u => u.Id == customerId);
            if (!userExists)
            {
                return new ResponseDto
                {
                    Message = "User not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            // تأكد أن المستخدم ليس لديه كارت مسبقاً
            var existingCart = await _context.Carts.AnyAsync(c => c.CustomerId == customerId);
            if (existingCart)
            {
                return new ResponseDto
                {
                    Message = "Cart already exists for this user!",
                    IsSucceeded = false,
                    StatusCode = 409
                };
            }

            var newCart = new Cart
            {
                CustomerId = customerId,
                TotalPrice = 0,
                Percentage = null
            };

            await _context.Carts.AddAsync(newCart);
            await _context.SaveChangesAsync();

            return new ResponseDto
            {
                Message = "New cart added successfully!",
                IsSucceeded = true,
                StatusCode = 201,
                model = new { cartId = newCart.Id }
            };
        }

        public async Task<ResponseDto> ApplyDiscountCode(int id, DiscountCodeDto discountPercentage)
        {
            if (!discountPercentage.IsActive)
            {
                return new ResponseDto
                {
                    Message = "Discount not valid!",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return new ResponseDto
                {
                    Message = "Cart not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            if (cart.TotalPrice <= 0)
            {
                return new ResponseDto
                {
                    Message = "Cart is empty or total price is zero, cannot apply discount!",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // تحقق من عدم تطبيق خصم سابق
            if (cart.Percentage != null)
            {
                return new ResponseDto
                {
                    Message = "Discount already applied!",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            cart.TotalPrice *= (1 - discountPercentage.Percentage / 100);
            cart.Percentage = discountPercentage.Percentage;

            await _context.SaveChangesAsync();

            var updatedCart = _mapper.Map<CartDto>(cart);
            return new ResponseDto
            {
                Message = "Discount applied successfully",
                IsSucceeded = true,
                StatusCode = 200,
                model = updatedCart
            };
        }

        public async Task<ResponseDto> DeleteCart(int id)
        {
            var existingCart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == id);
            if (existingCart == null)
            {
                return new ResponseDto
                {
                    Message = "Cart not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            // حذف العناصر المرتبطة أولاً إذا لم يكن الحذف التتابعي مفعلًا في قاعدة البيانات
            if (existingCart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(existingCart.CartItems);
            }

            _context.Carts.Remove(existingCart);
            await _context.SaveChangesAsync();

            return new ResponseDto
            {
                Message = "Cart deleted successfully",
                IsSucceeded = true,
                StatusCode = 200
            };
        }

        public async Task<ResponseDto> GetAllCart()
        {
            var carts = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .AsNoTracking()
                .ToListAsync();

            if (!carts.Any())
            {
                return new ResponseDto
                {
                    Message = "No carts found",
                    IsSucceeded = false,
                    StatusCode = 404,
                    model = new List<CartDto>()
                };
            }

            var dto = _mapper.Map<List<CartDto>>(carts);
            return new ResponseDto
            {
                IsSucceeded = true,
                StatusCode = 200,
                model = dto
            };
        }

        public async Task<ResponseDto> GetCart(int id)
        {
            var existingCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCart == null)
            {
                return new ResponseDto
                {
                    Message = "Cart not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            var dto = _mapper.Map<CartDto>(existingCart);
            return new ResponseDto
            {
                IsSucceeded = true,
                StatusCode = 200,
                model = dto
            };
        }

        public async Task<ResponseDto> UpdateCart(int id, CartDto cart)
        {
            var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id);
            if (existingCart == null)
            {
                return new ResponseDto
                {
                    Message = "Cart not found!",
                    IsSucceeded = false,
                    StatusCode = 404
                };
            }

            // يمكن استثناء بعض الحقول حسب الحاجة
            _mapper.Map(cart, existingCart);
            await _context.SaveChangesAsync();

            var cartDto = _mapper.Map<CartDto>(existingCart);
            return new ResponseDto
            {
                IsSucceeded = true,
                StatusCode = 200,
                model = cartDto
            };
        }
    }
}
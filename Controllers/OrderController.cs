using AutoMapper;
using LibraryAPI.Context;
using LibraryAPI.Models;
using LibraryAPI.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AddDbContext _dbContext;
        private readonly IMapper _mapper;

        public OrderController(AddDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpPost("createOrder/{userID}/{bookID}")]
        public async Task<IActionResult> CreateOrder(int userID, int bookID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            var dbBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == bookID);

            if (dbBook == null) return BadRequest(new { Message = "Livro não encontrado." });
            if (dbUser == null) return BadRequest(new { Message = "Usuário não encontrado." });
            if (!dbUser.Active) return BadRequest(new { Message = "Usuário não está confirmado." });
            if (dbUser.Blocked) return BadRequest(new { Message = "Usuário está bloqueado." });
            if (dbBook.UserID == userID) return BadRequest(new { Message = "Você não pode pedir um livro próprio" });


            var existingOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserID == userID && o.BookID == bookID);

            if (existingOrder != null) return BadRequest(new { Message = "Já existe um pedido para este usuário e livro." });

            if (dbBook.Ordered) return BadRequest(new { Message = "Este livro já foi pedido." });

            var order = new Order
            {
                UserID = userID,
                BookID = bookID,
                OrderDate = DateTime.UtcNow
            };

            dbBook.Ordered = true;
            var newOrder = _mapper.Map<Order>(order);
            await _dbContext.AddAsync(newOrder);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Pedido criado com sucesso." });
        }

        [HttpGet("getOrders")]

        public async Task <ActionResult<Order>> GetOrders()
        {
            var order = await _dbContext.Orders.Include(u => u.User).ThenInclude(b => b.Books).Include(b => b.Book).Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.UserID,
                o.User.FirstName,
                o.User.LastName,
                o.Book.Title,
                o.BookID,
                o.Book.Ordered
            }).ToListAsync();
            return Ok(order);
        }

        [HttpGet("userOrders/{userID}")]

        public async Task <IActionResult> GetUserOrders(int userID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (dbUser == null) return BadRequest(new { Message = "Usuário não encontrado." });
            if (!dbUser.Active) return BadRequest(new { Message = "Usuário não está confirmado." });
            if (dbUser.Blocked) return BadRequest(new { Message = "Usuário está bloqueado." });
            var userOrders = await _dbContext.Orders.Include(b => b.Book).Where(o => o.UserID == userID).Select(o => new
            {
                o.Book.Id,
                o.Book.Title,
                o.Book.Author,
                o.Book.Price,
                o.Book.Category,
                o.Book.SubCategory,
                o.OrderDate,
                o.Book.Ordered,
            }).ToListAsync();
     
            if (userOrders.Count == 0) return BadRequest(new { Message = "Nao há pedidos para este usuário. " });

            return Ok(userOrders);
        }
    }
}

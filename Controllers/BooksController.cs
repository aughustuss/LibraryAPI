using LibraryAPI.Context;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AddDbContext _dbContext;
        public BooksController(AddDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("{userId}/create")]
        public async Task<IActionResult> RegisterBook(Book book, int userId)
        {
            var dbUser = await _dbContext.Users.FindAsync(userId);
            if(dbUser == null) return BadRequest(new { Message = "Usuário nao encontrado. " });
            if (!dbUser.Active) return BadRequest(new { Message = "Usuário não está confirmado." });
            if (dbUser.Blocked) return BadRequest(new { Message = "Usuário está bloqueado." });
            if (await CheckBookExists(book.Title)) return BadRequest(new { Message = "Este livro já foi cadastrado" });

            book.Ordered = false;
            book.UserID = userId;
            await _dbContext.AddAsync(book);
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "Livro Cadastrado com sucesso."
            });
        }
        
        private async Task<bool> CheckBookExists(string bookName)
        {
            return await _dbContext.Books.AnyAsync(b => b.Title == bookName);
        }

        [HttpGet("books")]
        
        public async Task<ActionResult<Book>> GetBooks()
        {
            return Ok(await _dbContext.Books.ToListAsync());
        }

        [HttpGet("userBooks/{id}")]

        public async Task<ActionResult<Book>> GetUserBooks(int id)
        {
            var dbBooks = await _dbContext.Books.Where(b => b.UserID == id).ToListAsync();
            if (dbBooks.Count == 0)
            {
                return BadRequest(new { Message = "Nenhum livro encontrado deste usuário" });
            }
            return Ok(dbBooks);
        }

        [HttpPut("returnBook/{userID}/{bookID}/{orderID}")]
        public async Task<IActionResult> ReturnBook(int userID, int bookID, int orderID)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderID && o.UserID == userID && o.BookID == bookID);
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (!dbUser.Active) return BadRequest(new { Message = "Usuário não está confirmado." });
            if (dbUser.Blocked) return BadRequest(new { Message = "Usuário está bloqueado." });
            if (order == null) return BadRequest(new { Message = "Pedido não encontrado." });

            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == order.BookID);

            if (book == null) return BadRequest(new { Message = "Livro não encontrado." });

            if (!book.Ordered) return BadRequest(new { Message = "Este livro já foi devolvido." });

            book.Ordered = false;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Livro devolvido com sucesso." });
        }

        [HttpDelete("deleteBook/{bookID}")]

        public async Task <IActionResult> DeleteBook(int bookID)
        {
            var dbBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == bookID);
            if (dbBook == null) return BadRequest(new { Message = "Livro não encontrado para o ID fornecido." });
            _dbContext.Books.Remove(dbBook);
            _dbContext.SaveChanges();
            return Ok(new { Message = "Livro deletado." });
        }

    }
}

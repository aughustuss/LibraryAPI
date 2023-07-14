using LibraryAPI.Context;
using LibraryAPI.Helpers;
using LibraryAPI.Models;
using LibraryAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using AutoMapper;
using LibraryAPI.Models.Dtos;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using LibraryAPI.Utils;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AddDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailservice;

        public UserController(AddDbContext dbContext, IMapper mapper, IConfiguration config, IEmailService emailService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _config = config;
            _emailservice = emailService;
        }

        [HttpPost("register")]
        
        public async Task <IActionResult> RegisterUser([FromBody]UserDTO user)
        {
            var newUser = _mapper.Map<User>(user);
            if(await CheckEmailExists(newUser.Email)) return BadRequest(new { Message = "Email já está em uso." });
            if(newUser == null) return BadRequest(new { Message = "Nao é possível cadastrar sem os dados." });
            newUser.Password = Hasher.HashPassword(newUser.Password);
            newUser.Token = "";
            newUser.Active = false;
            newUser.Blocked = false;
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            newUser.ConfirmEmailToken = emailToken;
            string from = _config["EmmailSettings:From"];
            var emailObj = new Email(newUser.Email, "Confirmação de Conta", EmailBody.EmailStrinBody(newUser.Email, emailToken));
            _emailservice.SendMail(emailObj);
            await _dbContext.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "Registro feito e email enviado com sucesso."
            });
        }

        private async Task<bool> CheckEmailExists(string Email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == Email);
        }

        [HttpPost("confirm-email")]

        public async Task<IActionResult> ConfirmUserEmail(ConfirmEmailDTO confirmEmail)
        {
            var newToken = confirmEmail.EmailToken.Replace(" ", "+");
            var dbUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == confirmEmail.Email);
            if (dbUser == null) return NotFound(new { Message = "Email não encontrado." });
            var tokenCode = dbUser.ConfirmEmailToken;
            if (tokenCode != confirmEmail.EmailToken) return BadRequest(new { Message = "Token inválido ou expirado." });
            dbUser.Active = true;
            _dbContext.Entry(dbUser).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Email confirmado." });
        }

        [HttpPost("authenticate")]

        public async Task <IActionResult> Authenticate([FromBody] User user)
        {
            if(user == null) return BadRequest(new { Message = "Não é possível realizar o login sem as credenciais." });
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if(dbUser == null) return BadRequest(new { Message = "Não existe um usuário com este email." });
            if (!dbUser.Active) return BadRequest(new { Message = "Sua conta ainda não está verificada. Cheque seu email para confirmar a conta." });

            if(!Hasher.UnhashPassword(user.Password, dbUser.Password))
            {
                return BadRequest(new { Message = "Senha inválida. " });
            }
            dbUser.Token = CreateJwt(dbUser);
            var accessToken = dbUser.Token;
            dbUser.TokenExpiration = DateTime.UtcNow.AddDays(5);
            await _dbContext.SaveChangesAsync();

            return Ok(new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = "",
            });
        }

        private static string CreateJwt(User user)
        {
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ri190842mcUlçLIQWer98vg12bvho09858naAHGRY857ashvakoemj1"));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var claims = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.UserRole),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            });
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = credentials,
            };
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.CreateToken(tokenDescriptor);
            return jwtHandler.WriteToken(token);
        }

        [HttpPut("blockUser/{userID}")]

        public async Task <IActionResult> BlockUser(int userID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (dbUser == null) return BadRequest(new { Message = "Usuario não encontrado." });
            if (dbUser.Blocked) return BadRequest(new { Message = "Usuário já está bloqueado." });
            dbUser.Blocked = true;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Bloqueado" });
        }

        [HttpPut("unblockUser/{userID}")]

        public async Task<IActionResult> UnblockUser(int userID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (dbUser == null) return BadRequest(new { Message = "Usuario não encontrado." });
            if (!dbUser.Blocked) return BadRequest(new { Message = "Usuário já está desbloqueado." });
            dbUser.Blocked = false;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Desbloqueado" });
        }

        [HttpPut("enableUser/{userID}")]
        public async Task<IActionResult> EnableUser(int userID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (dbUser == null) return BadRequest(new { Message = "Usuario não encontrado." });
            if (dbUser.Active) return BadRequest(new { Message = "Usuário já está ativado." });
            dbUser.Active = true;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Ativado." });
        }

        [HttpPut("disableUser/{userID}")]
        public async Task<IActionResult> DisableUser(int userID)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userID);
            if (dbUser == null) return BadRequest(new { Message = "Usuario não encontrado." });
            if (!dbUser.Active) return BadRequest(new { Message = "Usuário já está desativado." });
            dbUser.Active = false;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Desativado." });
        }

        [HttpGet("getinfo/{id}")]
        public async Task<IActionResult> GetUserInfo(int id)
        {
           var userByID = await _dbContext.Users.Include(_ => _.Books).Where(_ => _.Id == id).FirstOrDefaultAsync();
           if (userByID == null) return BadRequest(new { Message = "Usuário não existe." });
            var returnedUser = new
            {
                userByID.UserRole,
                userByID.CreatedOn,
                userByID.Fine,
                userByID.FirstName,
                userByID.LastName,
                userByID.Email,
                userByID.Mobile,
                userByID.Blocked,
                userByID.Books.Count,
                userByID.Id,
            };
           return Ok(returnedUser);
        }

        [HttpGet("users")]

        public async Task <ActionResult<User>> GetAllUsers()
        {
            var dbUsers = await _dbContext.Users.ToListAsync();
           
            foreach (var user in dbUsers)
            {
                var orders = await _dbContext.Orders.Include(o => o.Book).Where(o => o.UserID == user.Id).ToListAsync();
                var fine = 0;
                foreach(var order in orders)
                {
                    if (order.Book.Ordered)
                    {
                        var orderDate = order.OrderDate;
                        var maxDate = orderDate.AddDays(10);
                        var currentDate = DateTime.UtcNow;
                        var extraDays = (currentDate - maxDate).Days;
                        extraDays  = extraDays < 0 ? 0 : extraDays;
                        fine = extraDays * 50;
                        user.Fine += fine;
                    }
                }
            }

            var usersData = dbUsers.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Mobile,
                u.Blocked,
                u.Active,
                u.CreatedOn,
                u.UserRole,
                u.Fine
            }).ToList();

            return Ok(usersData);
        }

    }
}

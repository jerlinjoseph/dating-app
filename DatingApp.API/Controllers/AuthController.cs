using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.DTO;
using DatingApp.API.Model;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace DatingApp.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register(UserForRegisterDto dto)
        {
            dto.Username = dto.Username.ToLower();

            if(await _repo.UserExists(dto.Username))
            {
                return BadRequest("User already exists");

            }

            var userToCreate = new User
            {
                Username = dto.Username
            };

            var createdUser = _repo.Register(userToCreate,dto.Password);

            return StatusCode(201);
            
        }

        [HttpPost("login")]
        public async Task<object> Login(UserForLoginDto dto)
        {
            var userFromRepo = await _repo.Login(dto.Username.ToLower(), dto.Password);

            if(userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds

            };

            var tokenhandler = new JwtSecurityTokenHandler();

            var token = tokenhandler.CreateToken(tokenDescriptor);

            return Ok(
                new { token = tokenhandler.WriteToken(token)}
            );


        }
    }
}
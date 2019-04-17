using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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
            _config = config;
            _repo = repo;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request at some point
            //make username case insensitive
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            //check if username is taken
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            //password needs to be created inside the repo
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            //return CreatedAtRoute(); //at some point
            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            //Check if user actually exists
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            //build up a token we are going to return to the user. Our token is going to contain two information
            //user id and username. Token can be used without calling the db. Server doesn't need to go into db to get username

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()), //our id Token is a string.
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            //we need to set a key to sign our token. It is hashed and should not be readable in the token
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value)); //encode security key into byte array we need to store the key in our appsettings file

            //now we have the key. we should generate some sign in credentials and to hash it
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //security token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(1), //valid for 1 hour
                SigningCredentials = creds
            };

            //now we need token handler.
            var tokenHandler = new JwtSecurityTokenHandler(); //using token handler we can create token and pass in the token descriptor

            var token = tokenHandler.CreateToken(tokenDescriptor); //this token will contain our jwt token

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}
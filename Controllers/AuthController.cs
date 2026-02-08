using Assignment_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Assignment_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SqlConnection _con;
        private readonly IConfiguration _config;
        public AuthController(SqlConnection con, IConfiguration config)
        {
            _con = con;
            _config = config;
        }




        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var sql = "SELECT * FROM dbo.Users WHERE Username = @Username AND PasswordHash = @Password";

            await _con.OpenAsync();

            SqlCommand cmd = new SqlCommand(sql, _con);
            cmd.Parameters.AddWithValue("@Username", request.Username);
            cmd.Parameters.AddWithValue("@Password", request.Password);

           
         


            var reader = await cmd.ExecuteReaderAsync();
             if (!reader.HasRows) return Unauthorized("Invalid credentials");
            reader.Close(); 

            await _con.CloseAsync();
         

            // For assignment demo: compare plain password (in real apps use hashing)
          

            var token = GenerateToken(request.Username);
            return Ok(new LoginResponse { Token = token, Username = request.Username });
        }

        private string GenerateToken(string username)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

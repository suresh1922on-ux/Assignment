using Assignment_Web_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data.Common;

namespace Assignment_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly SqlConnection _con;
        private readonly IConfiguration _cong;
        public EmployeesController(SqlConnection con,IConfiguration cong)
        {
            this._con = con;
            this._cong = cong;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            List<Employee> l = new List<Employee>();
            var sql = "SELECT * FROM Employees";

            await using (_con)
            {
                await _con.OpenAsync();
                SqlCommand cmd = new SqlCommand(sql, _con);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    l.Add(new Employee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Department = reader["Department"] as string,
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        DateJoined = reader["DateJoined"] as DateTime?
                    });
                }


            }

            return Ok(l);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {

            List<Employee> l = new List<Employee>();
            var sql = "SELECT * FROM Employees WHERE Id = @Id";
            await using (_con)
            {
                await _con.OpenAsync();
                SqlCommand cmd = new SqlCommand(sql,_con);

                cmd.Parameters.AddWithValue("id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read() == null )
                {
                    return BadRequest();
                }

                while (await reader.ReadAsync())
                {
                    l.Add(new Employee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Department = reader["Department"] as string,
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        DateJoined = reader["DateJoined"] as DateTime?

                    });

                }

      
               


            }
            
            return Ok(l);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Employee emp)
        {
            var sql = @"INSERT INTO Employees
                (FirstName, LastName, Email, Department, Salary, DateJoined)
                VALUES
                (@FirstName, @LastName, @Email, @Department, @Salary, @DateJoined)";

            await using var con = new SqlConnection(_cong.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
            cmd.Parameters.AddWithValue("@LastName", emp.LastName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@DateJoined", emp.DateJoined ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            return Ok("Employee created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee emp)
        {
            var sql = @"UPDATE Employees SET
                FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                Department = @Department,
                Salary = @Salary,
                DateJoined = @DateJoined
                WHERE Id = @Id";

            await using var con = new SqlConnection(_cong.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
            cmd.Parameters.AddWithValue("@LastName", emp.LastName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@DateJoined", emp.DateJoined ?? (object)DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound("Employee not found");

            return Ok("Employee updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sql = "DELETE FROM Employees WHERE Id = @Id";

            await using var con = new SqlConnection(_cong.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound("Employee not found");

            return Ok("Employee deleted");
        }

        [HttpDelete("delete-multiple")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids.Count == 0)
                return BadRequest("No IDs provided");

            var sql = $"DELETE FROM Employees WHERE Id IN ({string.Join(",", ids)})";

            await using var con = new SqlConnection(_cong.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            int rows = await cmd.ExecuteNonQueryAsync();

            return Ok($"{rows} employees deleted");
        }

    }
}

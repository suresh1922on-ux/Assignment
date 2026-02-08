using Assignment_Web_API.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Assignment_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ReportsController : ControllerBase
    {

        private readonly IConfiguration _config;

        public ReportsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("employees/excel")]
        public async Task<IActionResult> EmployeesExcel()
        {
            var table = new DataTable();
            table.Columns.Add("Id");
            table.Columns.Add("First Name");
            table.Columns.Add("Last Name");
            table.Columns.Add("Email");
            table.Columns.Add("Department");
            table.Columns.Add("Salary");
            table.Columns.Add("Date Joined");

            using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Employees", con);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                table.Rows.Add(
                    reader["Id"],
                    reader["FirstName"],
                    reader["LastName"],
                    reader["Email"],
                    reader["Department"],
                    reader["Salary"],
                    reader["DateJoined"]
                );
            }

            using var wb = new XLWorkbook();
            wb.Worksheets.Add(table, "Employees");

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employees.xlsx");
        }


        [HttpGet("attendance/excel")]
        public async Task<IActionResult> AttendanceExcel()
        {
            var table = new DataTable();
            table.Columns.Add("EmployeeId");
            table.Columns.Add("Date");
            table.Columns.Add("Status");

            using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            var sql = "SELECT EmployeeId, Date, Status FROM  Attendance";
            var cmd = new SqlCommand(sql, con);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                table.Rows.Add(
                    reader["EmployeeId"],
                    reader["Date"],
                    reader["Status"]
                );
            }

            using var wb = new XLWorkbook();
            wb.Worksheets.Add(table, "Attendance");

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Attendance.xlsx");
        }






    }
}

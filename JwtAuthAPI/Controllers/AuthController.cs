﻿using JwtAuthAPI.Core;
using JwtAuthAPI.Core.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles() 
        {
            bool isOwnerRoleExists = await _roleManager.RoleExistsAsync(ApplicationConstant.OWNER);
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(ApplicationConstant.ADMIN);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(ApplicationConstant.USER);

            if (isOwnerRoleExists && isAdminRoleExists && isUserRoleExists)
                return Ok("Roles Seeding is Already Done");

            await _roleManager.CreateAsync(new IdentityRole(ApplicationConstant.USER));
            await _roleManager.CreateAsync(new IdentityRole(ApplicationConstant.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(ApplicationConstant.OWNER));
            return Ok("Role Seeding Done Successfully");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        { 
            var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExistsUser != null)
                return BadRequest("UserName Already Exists");
            IdentityUser newUser = new IdentityUser()
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!createUserResult.Succeeded) 
            {
                var errorString = "User Creation Failed Beacause: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return BadRequest(errorString);
            }
            //Add a default user role to all users
            await _userManager.AddToRoleAsync(newUser, ApplicationConstant.USER);
            return Ok("User Created Successfully");
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        { 
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user is null)
                return Unauthorized("Invalid Credentials");

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
                return Unauthorized("Invalid Credentials");

            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            { 
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenereateNewJsonWebToken(authClaims);
            return Ok(token);
        }


        private string GenereateNewJsonWebToken(List<Claim> authClaims) 
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);   
            return token;
        }


        [HttpPost]
        [Route("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);
            if (user is null)
                return BadRequest("Invalid User name !!!!!");

            await _userManager.AddToRoleAsync(user, ApplicationConstant.ADMIN);
            return Ok("User is now an ADMIN");
        }
        
        
        [HttpPost]
        [Route("make-owner")]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);
            if (user is null)
                return BadRequest("Invalid User name !!!!!");

            await _userManager.AddToRoleAsync(user, ApplicationConstant.OWNER);
            return Ok("User is now an OWNER");
        }



    }
}

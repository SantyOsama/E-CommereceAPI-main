﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestToken.DTO.PasswordSettingsDto;
using TestToken.DTO.UserDtos;
using TestToken.UOW;

namespace TestToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsunc(RegisterDto userRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var account = await _unitOfWork.Customers.RegisterAsync(userRegister);
            if (account == null)
                return BadRequest("Registeration Failed!");
            if (account.IsSucceeded)
                return Ok(account);
            return StatusCode(account.StatusCode, account.Message);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(LoginDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var account = await _unitOfWork.Customers.LoginAsync(login);
            if (account == null)
                return BadRequest("Login Failed!");
            if (account.IsSucceeded)
                return Ok(account);
            return StatusCode(account.StatusCode, account.Message);
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(LoginDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var account = await _unitOfWork.Customers.LogoutAsync(login);
            if (account == null)
                return BadRequest("Login Failed!");
            if (account.IsSucceeded)
                return Ok(account);
            return StatusCode(account.StatusCode, new { account.Message });
        }
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfileAsync(userDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
          var response = await _unitOfWork.Customers.UpdateProfile(user);
         return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto password)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Customers.ChangePasswordAsync(password);
            if(response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode);
        }
        [HttpPost("Forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody]string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Customers.ForgotPasswordAsync(email);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, response.model);
        }

            [HttpPut("ResetPassword")]
        public async Task<IActionResult> ResetPassswordAsync(ResetPasswordDto password)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Customers.ResetPasswordAsync(password);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode);
        }
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeletAccountAsync(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Customers.DeleteAccountAsync(loginDto);
            if(response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode);
        }
        [Authorize(Policy ="Admin")]
        [HttpPost("GetRefreshToken")]
        public async Task<IActionResult> GetRefreshTokenAsync([FromBody]string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _unitOfWork.Customers.GenerateRefreshTokenAsync(email);

            if (token.IsSucceeded)
                return StatusCode(token.StatusCode, token.model);

            return StatusCode(token.StatusCode, token.model);
        }
        [Authorize(Policy = "Admin")]
        [HttpPost("RevokeRefreshToken")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var token = await _unitOfWork.Customers.RevokeRefreshTokenAsync(email);
            if (!token)
                return NotFound("User or active Token not found!");
            return Ok("Token revoked successfully");
        }
        [Authorize(Policy = "Admin")]
        [HttpPost("Send-otp")]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var respone = await _unitOfWork.Customers.SendOtpAsync(email);
            if(respone.IsSucceeded)
                return Ok(respone.model);
            return StatusCode(respone.StatusCode, respone.model);   
        }


        [HttpPost("Verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest verifyOtp)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var respone = await _unitOfWork.Customers.VerifyOtpRequest(verifyOtp);
            if (respone.IsSucceeded)
                return Ok(respone);
            return StatusCode(respone.StatusCode, respone.model);
        }
    }
}


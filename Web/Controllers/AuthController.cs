using Domain.Common.Models;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Identity.Welcome.Commands;
using Infrastructure.Identity.Welcome.Contracts;
using Infrastructure.Identity.Welcome.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class AuthController : BaseController
    {
        public AuthController(ILogger<AuthController> logger, ISender mediator) : base(logger, mediator)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _mediator.Send(new LoginCommand(model.Email, model.Password, model.RememberMe));

                if (response.Result.Succeeded)
                {
                    // Check if user has 2FA enabled by checking if Data is null (2FA required)
                    if (response.Data?.Roles?.Any() == true)
                    {
                        return LocalRedirect(returnUrl ?? "/");
                    }
                    else
                    {
                        return RedirectToAction(nameof(Login2fa), new { email = model.Email, rememberMe = model.RememberMe, returnUrl });
                    }
                }

                var errorMessage = response.Result.Errors?.FirstOrDefault() ?? "Login failed";
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login2fa(string email, bool rememberMe, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new Login2faViewModel { Email = email, RememberMe = rememberMe });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login2fa(Login2faViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (response, user) = await _mediator.Send(new Login2FaCommand(model.Email, model.Password, model.RememberMe, model.Code));

                if (response.LoginOk)
                {
                    return LocalRedirect(returnUrl ?? "/");
                }

                ModelState.AddModelError(string.Empty, response.Message ?? "Two-factor authentication failed");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA login");
                ModelState.AddModelError(string.Empty, "An error occurred during authentication. Please try again.");
                return View(model);
            }
        }

        [AuthorizationExtensions.AuthorizePolicies(Policy.LoggedIn)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _mediator.Send(new LogoutCommand());
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new RegisterRequest
                {
                    Email = model.Email,
                    Password = model.Password,
                    Name = $"{model.FirstName} {model.LastName}"
                };

                var response = await _mediator.Send(new RegisterCommand(request));

                if (response.Result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account.";
                    return RedirectToAction(nameof(Login));
                }

                foreach (var error in response.Result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View(new VerifyEmailViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new VerifyEmailRequest
                {
                    Email = model.Email,
                    Code = model.Token
                };

                var response = await _mediator.Send(new VerifyEmailCommand(request));

                if (response.Result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
                    return RedirectToAction(nameof(Login));
                }

                foreach (var error in response.Result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                ModelState.AddModelError(string.Empty, "An error occurred during email verification. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new ForgotPasswordRequest
                {
                    Email = model.Email
                };

                var response = await _mediator.Send(new ForgotPasswordCommand(request));

                TempData["SuccessMessage"] = "If an account with that email exists, we've sent password reset instructions.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new ResetPasswordRequest
                {
                    Email = model.Email,
                    Token = model.Token,
                    NewPassword = model.Password
                };

                var response = await _mediator.Send(new ResetPasswordCommand(request));

                if (response.Result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successfully! You can now log in with your new password.";
                    return RedirectToAction(nameof(Login));
                }

                foreach (var error in response.Result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                ModelState.AddModelError(string.Empty, "An error occurred during password reset. Please try again.");
                return View(model);
            }
        }
    }
} 
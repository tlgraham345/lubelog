﻿using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace CarCareTracker.Controllers
{
    [Authorize(Roles = nameof(UserData.IsAdmin))]
    public class AdminController : Controller
    {
        private ILoginLogic _loginLogic;
        private IUserLogic _userLogic;
        public AdminController(ILoginLogic loginLogic, IUserLogic userLogic)
        {
            _loginLogic = loginLogic;
            _userLogic = userLogic;
        }
        public IActionResult Index()
        {
            var viewModel = new AdminViewModel
            {
                Users = _loginLogic.GetAllUsers(),
                Tokens = _loginLogic.GetAllTokens()
            };
            return View(viewModel);
        }
        public IActionResult GenerateNewToken(string emailAddress, bool autoNotify)
        {
            var result = _loginLogic.GenerateUserToken(emailAddress, autoNotify);
            return Json(result);
        }
        public IActionResult DeleteToken(int tokenId)
        {
            var result = _loginLogic.DeleteUserToken(tokenId);
            return Json(result);
        }
        public IActionResult DeleteUser(int userId)
        {
            var result =_userLogic.DeleteAllAccessToUser(userId) && _loginLogic.DeleteUser(userId);
            return Json(result);
        }
    }
}

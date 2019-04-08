using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDO_API.DataBase.CosmosDB;
using IDO_API.Extensions;
using IDO_API.Models;
using IDO_API.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IDO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        AccountManager accountManager = AccountManager.DefaultManager;
        DataBase.AzureStorage.ContentManager imageContentManager = DataBase.AzureStorage.ContentManager.DefaultManager;
        ContentManager contentManager = ContentManager.DefaultManager;

        [HttpPost]
        public async Task<ActionResult<Response>> ApiCreateAccountAsync()
        {
            try
            {
                var requestData = Request.Form;
                User user = new User(requestData["nickname"], requestData["password"]);
                await accountManager.CreateAccountAsync(user);
                string id = accountManager.GetAccountId(requestData["nickname"]);
                await contentManager.CreateContentDocumentAsync(id);
                await imageContentManager.CreateContainerAsync(id);
                return new SimpleResponse(); // OK
            }
            catch (Exception e)
            {
                return new SimpleResponse(1, e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult<Response>> ApiUpadateAccountInfoAsync()
        {
            try
            {
                var requestData = Request.Form;
                User user = new User(requestData["newnickname"], requestData["newpassword"]);
                user.Id = requestData["nickname"];
                await accountManager.UpadateAccountInfoAsync(requestData["password"], user);
                return new SimpleResponse(); // OK
            }
            catch (Exception e)
            {
                return new SimpleResponse(2, e.Message);
            }
        }

        [HttpGet("/account/{l}/{p}")]
        public ActionResult<Response> ApiGetAccountData(string l, string p)
        {
            try
            {
                return new AccountDataResponse(0, accountManager.GetAccountData(l, p));
            }
            catch (Exception e)
            {
                return new SimpleResponse(3, e.Message);
            }
        }
        [HttpPost("/api/accounts/follow")]
        public ActionResult<Response> ApiFollow()
        {
            try
            {
                var data = Request.Form;
                var nick = data["nickname"];
                var pass = data["password"];
                var follow = data["follow"];
                accountManager.Follow(nick, pass, follow);
                return new SimpleResponse();
            }
            catch (Exception e)
            {
                return new SimpleResponse(4, e.Message);
            }
        }
        [HttpPost("/api/accounts/unfollow")]
        public ActionResult<Response> ApiUnfollow()
        {
            try
            {
                var data = Request.Form;
                var nick = data["nickname"];
                var pass = data["password"];
                var follow = data["unfollow"];
                accountManager.UnFollow(nick, pass, follow);
                return new SimpleResponse();
            }
            catch (Exception e)
            {
                return new SimpleResponse(4, e.Message);
            }
        }
    }
}
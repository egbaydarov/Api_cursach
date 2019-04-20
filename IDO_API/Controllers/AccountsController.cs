using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IDO_API.DataBase.CosmosDB;
using IDO_API.DataBase.Hashing;
using IDO_API.Extensions;
using IDO_API.Models;
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
        public async Task<ActionResult> ApiCreateAccountAsync()
        {
            try
            {
                var requestData = Request.Form;
                var password = SHA.GenerateSaltedHashBase64(requestData["password"]);
                User user = new User(requestData["nickname"], password);
                var res_1 = await accountManager.CreateAccountAsync(user);
                string id = accountManager.GetAccountId(requestData["nickname"]);
                var res_2 =  await contentManager.CreateContentDocumentAsync(id);
                var res_3 = await imageContentManager.CreateContainerAsync(id);
                if (res_1 == -1 || res_2 == -1 || res_3 == -1 || id == null)
                    return BadRequest("Cant create account.");

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> ApiUpadateAccountInfoAsync()
        {
            try
            {
                var requestData = Request.Form;
                var password = SHA.GenerateSaltedHashBase64(requestData["password"]);
                var newpassword = SHA.GenerateSaltedHashBase64(requestData["newpassword"]);
                User user = new User(requestData["newnickname"], newpassword);
                user.Id = requestData["nickname"];

                var result = await accountManager.UpadateAccountInfoAsync(requestData["password"], user);
                if (result == -1)
                    return BadRequest("Cant update account");

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("/account/{l}/{p}")]
        public ActionResult<User> ApiGetAccountData(string l, string p)
        {
            try
            {

                var result = accountManager.GetAccountData(l, p);
                if (result == null)
                    return BadRequest("Wrong users data");
                return result;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("/api/accounts/follow")]
        public async Task<ActionResult> ApiFollow()
        {
            try
            {
                var data = Request.Form;
                var nick = data["nickname"];
                var pass = data["password"];
                var follow = data["follow"];
                var result = await accountManager.Follow(nick, pass, follow);
                if (result == 1)
                    return BadRequest("Can't Follow");
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("/api/accounts/unfollow")]
        public async Task<ActionResult> ApiUnfollow()
        {
            try
            {
                var data = Request.Form;
                var nick = data["nickname"];
                var pass = data["password"];
                var follow = data["unfollow"];
                var result = await accountManager.UnFollow(nick, pass, follow);
                if (result == -1)
                    return BadRequest("Cant unfollow");
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("/searchuser/{nickname}")]
        public ActionResult<List<User>> ApiUserDataSearch(string nickname)
        {
            try
            {
                var users = accountManager.FindUserByNickname(nickname);
                if (users == null)
                    return BadRequest("Can't find users.");
                return users;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("/account/{nickname}")]
        public ActionResult<User> ApiGetUserData(string nickname)
        {
            try
            {
                var user = accountManager.GetProtectedAccountData(nickname);
                if (user == null)
                    return BadRequest("User was not found.");

                return user;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("/goals")]
        public async Task<ActionResult> AddGoal()
        {
            try
            {
                var data = Request.Form;
                var nick = data["nickname"];
                var pass = data["password"];
                var gnick = data["goalsnickname"];
                var desc = data["description"];
                if (!accountManager.IsValidAcccount(nick, pass))
                    return BadRequest("Not valid user data");
                var result = await accountManager.AddGoal(nick, gnick, desc);
                if (result == -1)
                    return BadRequest("Can't add goal");
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("/avatar")]
        public async Task<ActionResult> UploadAvatar([FromHeader]IFormFile file)
        {
            try
            {
                string ext = file.FileName.Split('.')[1].ToLower();
                if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                    throw new ApplicationException("Wrong image extension.");

                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];

                string imagename = "av" + MethodsEx.GetCurrentTimeString() + "." + ext;

                using (Stream image = file.OpenReadStream())
                {
                    if (!accountManager.IsValidAcccount(l, p))
                        throw new ApplicationException("Inccorect user data.");

                    var user = accountManager.GetAccountData(l, p);
                    var res_1 = await imageContentManager.UploadImageAsync(user.Id, imagename, image);
                    var res_2 = await accountManager.UploadAvatar(l, p, imagename);
                    if (res_1 == -1 || res_2 == -1)
                        return BadRequest("Cant upload avatar");
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
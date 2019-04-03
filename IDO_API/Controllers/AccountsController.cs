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
                var requestData = Request.Form.ToArray();
                User user = new User(requestData[0].Value, requestData[1].Value);
                await accountManager.CreateAccountAsync(user);
                string id = accountManager.GetAccountId(requestData[0].Value);
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
                var requestData = Request.Form.ToArray();
                User user = new User(requestData[2].Value, requestData[3].Value);
                user.Id = requestData[0].Value;
                await accountManager.UpadateAccountInfoAsync(requestData[1].Value,user);
                return new SimpleResponse(); // OK
            }
            catch (Exception e)
            {
                return new SimpleResponse(2, e.Message);
            }
        }
        [HttpGet]
        public ActionResult<Response> ApiGetAccountData()
        {
            try
            {
                var requestData = Request.Form.ToArray();
                string l = requestData[0].Value;
                string p = requestData[1].Value;
                
                return new AccountDataResponse(0, accountManager.GetAccountData(l,p));

            }
            catch(Exception e)
            {
                return new SimpleResponse(3, e.Message);
            }
        }
    }
}
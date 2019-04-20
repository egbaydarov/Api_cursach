using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IDO_API.DataBase.CosmosDB;
using IDO_API.Extensions;
using IDO_API.Models;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IDO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentsController : Controller
    {
        AccountManager accountManager = AccountManager.DefaultManager;
        DataBase.AzureStorage.ContentManager imageContentManager = DataBase.AzureStorage.ContentManager.DefaultManager;
        ContentManager contentManager = ContentManager.DefaultManager;


        [HttpGet("/{nickname}/notes")]
        public ActionResult<List<Note>> ApiGetUserContent(string nickname)
        {
            try
            {
                string id = accountManager.GetAccountId(nickname);

                if (id == null)
                    return BadRequest("Wrong users id");

                var result = contentManager.GetNotes(id);

                if (result == null)
                    return BadRequest("Can't get user notes");

                return result;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPut]
        public async Task<ActionResult> ApiPutNote([FromHeader]IFormFile file)
        {
            try
            {
                string ext = file.FileName.Split('.')[1].ToLower();
                if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                    throw new ApplicationException("Wrong image extension.");

                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];
                string descr = requestData["description"];

                string imagename = MethodsEx.GetCurrentTimeString() + "." + ext;

                using (Stream image = file.OpenReadStream())
                {
                    if (!accountManager.IsValidAcccount(l, p))
                        throw new ApplicationException("Inccorect user data.");

                    var user = accountManager.GetAccountData(l, p);
                    await imageContentManager.UploadImageAsync(user.Id, imagename, image);
                    await contentManager.AddNoteAsync(user.Id, new Note(descr, imagename, new List<string>()));
                    return Ok();
                }
            }
            catch (IndexOutOfRangeException)
            {
                return BadRequest("Incorrect image file name.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> ApiReplaceDescription()
        {
            try
            {
                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];
                string notereference = requestData["note"];
                string newdescr = requestData["description"];
                string imagename = MethodsEx.GetCurrentTimeString();

                var newNote = new Note(newdescr, imagename, new List<string>());

                if (accountManager.IsValidAcccount(l, p))
                {
                    string id = accountManager.GetAccountId(l);
                    if (id == null)
                        return BadRequest("Cant find user.");

                    var result = await contentManager.ReplaceNoteAsync(id, notereference, newNote);
                    if (result == -1)
                        return BadRequest("Cant replace note.");

                    return Ok();
                }
                return BadRequest("Is not valid account.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        public async Task<ActionResult> ApiDeleteNote()
        {
            try
            {
                string l = Request.Form["nickname"];
                string p = Request.Form["password"];
                string blobref = Request.Form["note"];
                string id = accountManager.GetAccountId(l);
                if (id == null)
                    return BadRequest("Cant find user");

                var result = await imageContentManager.DeleteImageAsync(id, blobref);

                if (result == -1)
                    return BadRequest("Cant delete Image");

                result = await contentManager.DeleteNoteAsync(id, blobref);

                if (result == -1)
                    return BadRequest("Cant delete Image description");

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("/{nickname}/{blobreference}/download")]
        public async Task<ActionResult> ApiDownloadSingleImage(string nickname, string blobreference)
        {
            try
            {
                Stream str = await imageContentManager.DownloadImageAsync(accountManager.GetAccountId(nickname), blobreference);
                if (str == null)
                    return BadRequest("Wrong Path");

                str.Seek(0, SeekOrigin.Begin);
                return File(str, "image/jpg", blobreference);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("/lukas")]
        public async Task<ActionResult<bool>> ApiGetRespect()
        {
            try
            {
                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];
                string note = requestData["note"];
                string lukased = requestData["lukased"];

                if (!accountManager.IsValidAcccount(l, p))
                    return BadRequest("Wrong username or password.");

                string id = accountManager.GetAccountId(lukased);
                if (id == null)
                    return BadRequest("Can't find user.");

                var result = await contentManager.AddOrDeleteRespectFromUser(l, note, id);
                if (result == null)
                    return BadRequest("Can't handle with respect method.");

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

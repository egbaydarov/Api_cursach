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
using IDO_API.Models.Responses;
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
        [HttpGet]
        public ActionResult<Response> ApiGetUserContent()
        {
            try
            {
                var requestData = Request.Form.ToArray();
                return new NotesDataResponse(0, contentManager.GetNotes(accountManager.GetAccountId(requestData[0].Value)));
            }
            catch (Exception e)
            {
                return new SimpleResponse(4, e.Message);
            }
        }
        [HttpPut]
        public async Task<ActionResult<Response>> ApiPutNote([FromHeader]IFormFile file)
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
                string imagename = MethodsEx.GetCurrentTimeString()+ "." + ext;
                using (Stream image = file.OpenReadStream())
                {
                    if (accountManager.IsValidAcccount(l, p))
                    {
                        var user = accountManager.GetAccountData(l, p);
                        await imageContentManager.UploadAchievementImageAsync(user.Id, imagename, image);
                        await contentManager.AddNoteAsync(user.Id, new Note(descr, imagename));
                        return new SimpleResponse(); // OK
                    }
                    else
                    {
                        throw new ApplicationException("Inccorect user data.");
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                return new SimpleResponse(5, "Incorrect image file name.");
            }
            catch (Exception e)
            {
                return new SimpleResponse(5, e.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult<Response>> ApiReplaceDescription()
        {
            try
            {
                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];
                string notereference = requestData["note"];
                string newdescr = requestData["description"];
                string imagename = MethodsEx.GetCurrentTimeString();
                var newNote = new Note(newdescr, imagename);
                if (accountManager.IsValidAcccount(l, p))
                {
                    await contentManager.ReplaceNoteAsync(accountManager.GetAccountId(l), notereference, newNote);
                    return new SimpleResponse();
                }
                else
                {
                    throw new ApplicationException("Inncorrect User Data.");
                }

            }
            catch (Exception e)
            {
                return new SimpleResponse(0, e.Message);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<Response>> ApiDeleteNote()
        {
            try
            {
                string l = Request.Form["nickname"];
                string p = Request.Form["password"];
                string blobref = Request.Form["note"];
                await imageContentManager.DeleteAchievementImageAsync(accountManager.GetAccountId(l), blobref);
                return new SimpleResponse();
            }
            catch (Exception e)
            {
                return new SimpleResponse(6, e.Message);
            }
        }
        [HttpGet("/image/{nickname}/{blobreference}")]
        public ActionResult<Response> ApiGetSingleNote(string nickname, string blobreference)
        {
            try
            {
                return new SingleNoteResponse(0, contentManager.GetSingleNote(accountManager.GetAccountId(nickname), blobreference));
            }
            catch (Exception e)
            {
                return new SimpleResponse(0, e.Message);
            }
        }
        [HttpGet("/{nickname}/{blobreference}/download")]
        public async Task<ActionResult<Response>> ApiDownloadSingleImage(string nickname, string blobreference)
        {
            try
            {
                using (MemoryStream str = new MemoryStream())
                {
                    await imageContentManager.DownloadAchievementImageAsync(accountManager.GetAccountId(nickname), blobreference, str);
                    byte[] image = new byte[str.Length];
                    await str.ReadAsync(image, 0, image.Length);
                    return new ImageResponse(0, image);
                }
            }
            catch (Exception e)
            {
                return new SimpleResponse(0, e.Message);
            }
        }
    }
}
using IDO_API.DataBase.CosmosDB;
using IDO_API.Extensions;
using IDO_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public async Task<ActionResult> ApiPutNote([FromHeader]IFormFile file, IFormFile file1 = null, IFormFile file2 = null, IFormFile file3 = null, IFormFile file4 = null)
        {
            try
            {
                var requestData = Request.Form;
                string l = requestData["nickname"];
                string p = requestData["password"];
                string descr = requestData["description"];
                if (!accountManager.IsValidAcccount(l, p))
                    throw new ApplicationException("Inccorect user data.");
                var user = accountManager.GetAccountData(l, p);
                string date = MethodsEx.GetCurrentTimeString();
                string imagename = null;
                string imagename1 = null;
                string imagename2 = null;
                string imagename3 = null;
                string imagename4 = null;
                if (file != null)
                {
                    string ext = file.FileName.Split('.')[1].ToLower();
                    if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                        throw new ApplicationException("Wrong image extension.");
                    imagename = date + "." + ext;
                    using (Stream image = file.OpenReadStream())
                        await imageContentManager.UploadImageAsync(user.Id, imagename, image);

                }
                if (file1 != null)
                {
                    string ext = file1.FileName.Split('.')[1].ToLower();
                    if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                        throw new ApplicationException("Wrong image extension.");
                    imagename1 = date + "-1." + ext;
                    using (Stream image = file1.OpenReadStream())
                        await imageContentManager.UploadImageAsync(user.Id, imagename1, image);
                }
                if (file2 != null)
                {
                    string ext = file2.FileName.Split('.')[1].ToLower();
                    if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                        throw new ApplicationException("Wrong image extension.");
                    imagename2 = date + "-2." + ext;
                    using (Stream image = file2.OpenReadStream())
                        await imageContentManager.UploadImageAsync(user.Id, imagename2, image);
                }
                if (file3 != null)
                {
                    string ext = file3.FileName.Split('.')[1].ToLower();
                    if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                        throw new ApplicationException("Wrong image extension.");
                    imagename3 = date + "-3." + ext;
                    using (Stream image = file3.OpenReadStream())
                        await imageContentManager.UploadImageAsync(user.Id, imagename3, image);
                }
                if (file4 != null)
                {
                    string ext = file4.FileName.Split('.')[1].ToLower();
                    if (!ext.Equals("jpeg") && !ext.Equals("jpg") && !ext.Equals("bmp") && !ext.Equals("png"))
                        throw new ApplicationException("Wrong image extension.");
                    imagename4 = date + "-4." + ext;
                    using (Stream image = file4.OpenReadStream())
                        await imageContentManager.UploadImageAsync(user.Id, imagename4, image);
                }
                string[] arr = new string[] { imagename1, imagename2, imagename3, imagename4 };
                var list = new List<string>();
                foreach (var n in arr)
                    if (n != null)
                        list.Add(n);
                await contentManager.AddNoteAsync(user.Id, new Note(descr, imagename,l, list));
                return Ok();
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

                var newNote = new Note(newdescr, imagename,l , new List<string>());

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
        [HttpDelete("/delete/{l}/{p}/{blobref}")]
        public async Task<ActionResult> ApiDeleteNote(string l, string p, string blobref)
        {
            try
            {
                if (!accountManager.IsValidAcccount(l, p))
                    return BadRequest("wrong nickname/pass");
                string id = accountManager.GetAccountId(l);
                if (id == null)
                    return BadRequest("Cant find user");


                var result = await contentManager.DeleteNoteAsync(id, blobref);

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
        public async Task<ActionResult<bool>> ApiPostRespect()
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
        [HttpGet("/feed")]
        public ActionResult<List<Note>> ApiGetFeed()
        {
            try
            {
                var notes = contentManager.GetAllNotes();
                if (notes == null)
                    return BadRequest("Can't get notes");
                return contentManager.GetAllNotes();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        [HttpPost("/report")]
        public async Task<ActionResult> ApiReportBug()
        {
            try
            {
                var requestData = Request.Form;
                string message = requestData["message"];
                await contentManager.ReportBug(message);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

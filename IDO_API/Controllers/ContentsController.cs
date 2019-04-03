using System;
using System.Collections.Generic;
using System.IO;
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
    public class ContentsController : ControllerBase
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
        public async Task<ActionResult<Response>> ApiPutNote()
        {
            try
            {
                var requestData = HttpContext.Items.ToArray();
                string l = (string)requestData[0].Value;
                string p = (string)requestData[1].Value;
                string imagename = MethodsEx.GetCurrentTimeString();
                string descr = (string)requestData[3].Value;
                using (MemoryStream image = (MemoryStream)requestData[2].Value)
                {
                    if (accountManager.IsValidAcccount(l, p))
                    {
                        var user = accountManager.GetAccountData(l, p);
                        await imageContentManager.UploadAchievementImageAsync(user.Id, imagename, image);
                        await contentManager.AddNoteAsync(user.Id, new Models.Note(descr, imagename));
                        return new SimpleResponse(); // OK
                    }
                    else
                    {
                        throw new ApplicationException("Inccorect user data.");
                    }
                }
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
                var requestData = HttpContext.Items.ToArray();
                string l = (string)requestData[0].Value;
                string p = (string)requestData[1].Value;
                string imagename = MethodsEx.GetCurrentTimeString();
                string notereference = (string)requestData[2].Value;
                string newdescr = (string)requestData[3].Value;
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
                string l = Request.Form.ToArray()[0].Value;
                string p = Request.Form.ToArray()[1].Value;
                string blobref = Request.Form.ToArray()[3].Value;
                await imageContentManager.DeleteAchievementImageAsync(accountManager.GetAccountId(l), blobref);
                return new SimpleResponse();
            }
            catch (Exception e)
            {
                return new SimpleResponse(6, e.Message);
            }
        }
        [HttpGet("/{nickname}/{blobreference}")]
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
        [HttpGet("/image/{nickname}/{blobreference}")]
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
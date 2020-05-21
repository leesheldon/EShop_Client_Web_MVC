using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Client_Web_MVC.Services
{
    public interface IPhotoService
    {
        Task<HttpResponseMessage> UploadNewPhoto(int id, HttpPostedFileBase newPhotoFile);

    }
}

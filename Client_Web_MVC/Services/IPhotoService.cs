using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Client_Web_MVC.Services
{
    public interface IPhotoService
    {
        Task<HttpResponseMessage> UploadNewPhoto(int id, HttpPostedFileBase newPhotoFile);

        Task<HttpResponseMessage> SetMainPhoto(int id, int photoId);

        Task<HttpResponseMessage> DeletePhoto(int id, int photoId);

    }
}

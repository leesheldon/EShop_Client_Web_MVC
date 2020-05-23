using Client_Web_MVC.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;

namespace Client_Web_MVC.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUserSession _userSession;

        public PhotoService(IUserSession userSession)
        {
            _userSession = userSession;
        }

        public async Task<HttpResponseMessage> UploadNewPhoto(int id, HttpPostedFileBase newPhotoFile)
        {
            string[] tmp = newPhotoFile.FileName.Split('.');
            string fileExt = tmp[1];

            using (var client = new HttpClient())
            {
                string requestParams = "api/products/" + id.ToString() + "/photo";
                // Access token
                string accessToken = "";
                accessToken = _userSession.AccessToken;

                client.BaseAddress = new Uri(SD.BaseUrl);
                client.DefaultRequestHeaders.Clear();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpContent fileStreamContent = new StreamContent(newPhotoFile.InputStream);
                fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "photo", FileName = newPhotoFile.FileName };
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/" + fileExt);

                var formData = new MultipartFormDataContent();
                formData.Add(fileStreamContent);
                HttpResponseMessage response = await client.PutAsync(requestParams, formData);
                                               
                return response;
            }
        }

        public async Task<HttpResponseMessage> SetMainPhoto(int id, int photoId)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products/" + id.ToString() + "/photo/" + photoId.ToString();
                // Access token
                string accessToken = "";
                accessToken = _userSession.AccessToken;

                client.BaseAddress = new Uri(SD.BaseUrl);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.DefaultRequestHeaders.Clear();

                var request = new HttpRequestMessage(HttpMethod.Post, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }

        public async Task<HttpResponseMessage> DeletePhoto(int id, int photoId)
        {
            using (var client = new HttpClient())
            {
                string requestParams = "api/products/" + id.ToString() + "/photo/" + photoId.ToString();
                // Access token
                string accessToken = "";
                accessToken = _userSession.AccessToken;

                client.BaseAddress = new Uri(SD.BaseUrl);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.DefaultRequestHeaders.Clear();

                var request = new HttpRequestMessage(HttpMethod.Delete, requestParams);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }

    }
}
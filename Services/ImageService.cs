

using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{

    public class ImageService : IImageService
    {
        private readonly string _defaultUserImageSrc = "/img/defaultUserImage.jpg";
        private readonly string _defaultCompanyImageSrc = "/img/defaultCompanyImage.jpg";
        private readonly string _defaultProjectImageSrc = "/img/ProjectImage.jpg";

        //To Do: Blog Customizations


        public string ConvertByteArrayToFile(byte[] fileData, string extension, int? imageType)
        {
            if ((fileData == null || fileData.Length == 0) && imageType != null)
            {
                switch (imageType)
                {
                    //BlogUser Image based on DefaultImage Enum
                    case 1: return _defaultUserImageSrc;
                    //BlogPost Image based on DefaultImage Enum
                    case 2: return _defaultProjectImageSrc;
                    //Category Image based on DefaultImage Enum
                    case 3: return _defaultCompanyImageSrc;
                }
            }

            try
            {
                string ImageBase64Data = Convert.ToBase64String(fileData!);
                return string.Format($"data:{extension};base64,{ImageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }
        }
 

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }

}



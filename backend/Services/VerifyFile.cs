using SixLabors.ImageSharp;

public class VerifyFileService
{

    private readonly IWebHostEnvironment environment;

    public VerifyFileService(IWebHostEnvironment environment) 
    {
        this.environment = environment;
    }

    public async Task<List<string>> VerifyFile(int UsersProductsId, List<IFormFile> photos) 
    {
        if (photos.Count > 4)
        {
            throw new AppErr(400, "Photos can not be more than 4 photos");
        }

        foreach(IFormFile file in photos)
        {
            if (file.Length == 0)
            {
                throw new AppErr(400, "Empty files are not allowed");
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new AppErr(400, "Each photo must be 5mg at most");
            }
        }  

        string FolderPath = Path.Combine(
            environment.ContentRootPath,
            "A_USERS_PHOTOS",
            $"property_{UsersProductsId}"
        );

        Directory.CreateDirectory(FolderPath);

        List<string> saves = new();

        foreach(IFormFile photo in photos)
        {
            string extention = Path.GetExtension(photo.FileName).ToLowerInvariant();

            string[] allowedExtention = {".jpg", ".jpeg", ".png", ".webp"};

            if (!allowedExtention.Contains(extention))
            {
                throw new AppErr(500, "Invalid file format");
            }

            string fileName = $"{Guid.NewGuid()}{extention}";

            string filePath = Path.Combine(FolderPath, fileName);

            await using FileStream stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write
            );

            await photo.CopyToAsync(stream);

            try
            {
                await using FileStream ImageStream = File.OpenRead(filePath);
                using Image image = await Image.LoadAsync(ImageStream);
            }
            catch (Exception Err) when (
                Err is UnknownImageFormatException ||
                Err is InvalidImageContentException ||
                Err is NotSupportedException
            )
            {
                File.Delete(filePath);

                throw new AppErr(400, "The uploaded file is not a valid image format");
            }
         

            saves.Add(
                Path.Combine("A_USERS_PHOTOS", $"property_{UsersProductsId}", fileName).Replace("\\", "/")
            );
        } 
        return saves;
    }
}










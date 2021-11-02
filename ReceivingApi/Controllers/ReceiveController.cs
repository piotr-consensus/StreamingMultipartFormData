using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ReceivingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiveController : ControllerBase
    {
        /// <summary>
        ///     <see
        ///         href="https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/mvc/models/file-uploads/samples/5.x/LargeFilesSample/Controllers/FileUploadController.cs" />
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ReceiveLargeFile()
        {
            var request = HttpContext.Request;

            if (!request.HasFormContentType
                || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
                || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            /* This throws an IOException: Unexpected end of Stream, the content may have already been read by another component.  */
            var section = await reader.ReadNextSectionAsync();
            
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader
                    && contentDisposition!.DispositionType.Equals("form-data")
                    && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    /* Fake copy to nothing since it doesn't even get here */
                    await section.Body.CopyToAsync(Stream.Null);
                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }
    }
}
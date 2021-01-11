﻿using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    public class PhotoModel
    {
        public IFormFile Photo { get; set; }
        public string Description { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger<PhotosController> _logger;
        private readonly OmgImageServerDbContext _dbContext;
        private readonly string _targetFilePath;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".jpg" };

        public PhotosController(IFileProvider fileProvider, ILogger<PhotosController> logger, OmgImageServerDbContext dbContext)
        {
            _fileProvider = fileProvider;
            _fileSizeLimit = 2097152;

            // To save physical files to a path provided by configuration:
            _targetFilePath = Path.Combine((Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), "omgimgflow_photos");
            _logger = logger;
            _dbContext = dbContext;
        }
        // GET: api/<PhotosController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var files = _fileProvider.GetDirectoryContents("omgimgflow_photos");

            return files.Select(f => WebUtility.HtmlDecode($"{f.Name}")).ToList();
        }

        // GET api/<PhotosController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PhotosController>
        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PhotoModel photoModel)
        {
            var trustedFileNameForDisplay = "";
            if (photoModel.Photo != null && photoModel.Photo.Length > 0)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        photoModel.Photo.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName);

                    var trustedFileNameForFileStorage = contentDisposition.FileName; // Path.GetRandomFileName();

                    var fileContent = await FileHelpers.ProcessFormFile<IFormFile>(photoModel.Photo, ModelState, _permittedExtensions, _fileSizeLimit);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    using (var targetStream = System.IO.File.Create(
                                Path.Combine(_targetFilePath, trustedFileNameForFileStorage.Replace("\"", ""))))
                    {
                        await targetStream.WriteAsync(fileContent);

                        _logger.LogInformation(
                            "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                            "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                            trustedFileNameForDisplay, _targetFilePath,
                            trustedFileNameForFileStorage);
                        var omgImage = new Models.OmgImage(trustedFileNameForDisplay);
                        omgImage.Description = photoModel.Description;
                        omgImage.AddTag("First");
                        _dbContext.OmgImages.Add(omgImage);
                        await _dbContext.SaveChangesAsync();
                    }
                }                
            }
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok();
        }

        // PUT api/<PhotosController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PhotosController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

﻿using API.Data;
using API.Dtos;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IEnumerable<PhotoDto> Get()
        {
            var photos = _dbContext.OmgImages
                .Include(i => i.Tags)
                .AsEnumerable()
                .Select(image =>
                {
                    var photoDto = new PhotoDto(image.Id, WebUtility.HtmlDecode($"{image.Filename}").Replace("\"", ""), image.Title, image.Description, image.Width, image.Height);
                    image.Tags.ToList().ForEach(tag => photoDto.AddTag(tag.Name));

                    return photoDto;
                });

            return photos;
        }

        // GET api/<PhotosController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDto>> Get(Guid id)
        {
            var photo = await _dbContext.OmgImages
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(image => image.Id == id);

            if (null == photo)
            {
                throw new NullReferenceException();
            }

            var photoDto = new PhotoDto(photo.Id, WebUtility.HtmlDecode($"{photo.Filename}").Replace("\"", ""), photo.Title, photo.Description, photo.Width, photo.Height);
            photo.Tags.ToList().ForEach(tag => photoDto.AddTag(tag.Name));

            return photoDto;
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

                    var trustedFileNameForFileStorage = contentDisposition.FileName;

                    var fileContent = await FileHelpers.ProcessFormFile<IFormFile>(photoModel.Photo, ModelState, _permittedExtensions, _fileSizeLimit);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    var filePath = Path.Combine(_targetFilePath, trustedFileNameForFileStorage.Replace("\"", ""));
                    using var image = new MagickImage(fileContent);

                    var omgImage = new Models.OmgImage(trustedFileNameForDisplay, photoModel.Title, image.Width, image.Height)
                    {
                        Description = photoModel.Description
                    };

                    photoModel.Tags.ForEach(tag =>
                    {
                        omgImage.AddTag(tag);
                    });

                    _dbContext.OmgImages.Add(omgImage);
                    await _dbContext.SaveChangesAsync();
                    image.Write(filePath);

                    _logger.LogInformation(
                        "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                        "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                        trustedFileNameForDisplay, _targetFilePath,
                        trustedFileNameForFileStorage);

                    return CreatedAtAction(nameof(this.Get), new { id = omgImage.Id }, omgImage);
                }
            }

            return BadRequest();
        }

        // PUT api/<PhotosController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromForm] PhotoModel photoModel)
        {
            var trustedFileNameForDisplay = "";
            var imageWidth = 0;
            var imageHeight = 0;
            if (photoModel.Photo != null && photoModel.Photo.Length > 0)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        photoModel.Photo.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName);

                    var trustedFileNameForFileStorage = contentDisposition.FileName;

                    var fileContent = await FileHelpers.ProcessFormFile<IFormFile>(photoModel.Photo, ModelState, _permittedExtensions, _fileSizeLimit);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    var filePath = Path.Combine(_targetFilePath, trustedFileNameForFileStorage.Replace("\"", ""));
                    using var image = new MagickImage(fileContent);
                    imageWidth = image.Width;
                    imageHeight = image.Height;

                    image.Write(filePath);

                    _logger.LogInformation(
                        "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                        "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                        trustedFileNameForDisplay, _targetFilePath,
                        trustedFileNameForFileStorage);
                }

            }


            var omgImage = await _dbContext.OmgImages
                .Include(image => image.Tags)
                .FirstOrDefaultAsync(image => image.Id == id);

            if (omgImage == null)
            {
                throw new NullReferenceException();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            omgImage.Title = photoModel.Title;
            omgImage.Description = photoModel.Description;
            
            if (!String.IsNullOrEmpty(trustedFileNameForDisplay))
            {
                omgImage.SetFilename(trustedFileNameForDisplay);
            }

            if (imageWidth != 0 && imageHeight != 0) {
                omgImage.SetDimension(imageWidth, imageHeight);
            }

            var tagsToRemove = omgImage.Tags.Where(tag => !photoModel.Tags.Contains(tag.Name)).ToList();

            tagsToRemove.ForEach(tag =>
                _dbContext.Entry(tag).State = EntityState.Deleted
            );

            photoModel.Tags.ForEach(tagToAdd =>
            {
                if (!omgImage.Tags.Any(tag => tag.Name.ToLower() == tagToAdd.ToLower()))
                {
                    omgImage.AddTag(tagToAdd);
                }
            });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated image");

            return NoContent();
        }

        // DELETE api/<PhotosController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {

            try
            {
                var omgImage = await _dbContext.OmgImages
                    .Include(i => i.Tags)
                    .FirstOrDefaultAsync(image => image.Id == id);

                if (null == omgImage)
                {
                    throw new NullReferenceException();
                }

                _dbContext.RemoveRange(omgImage.Tags);
                _dbContext.OmgImages.Remove(omgImage);

                await _dbContext.SaveChangesAsync();

                var filename = omgImage.Filename.Replace("\"", "");
                System.IO.File.Delete(Path.Combine(_targetFilePath, filename));
            }
            catch (IOException ioException)
            {
                Console.WriteLine(ioException.Message);
            }

            return NoContent();
        }
    }
}

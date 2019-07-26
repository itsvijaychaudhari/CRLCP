using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRLCP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRLCP.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValidationImageTextController : ControllerBase
    {

        private readonly CLRCP_MASTERContext _masterContext;
        private readonly VALIDATION_INFOContext _validationInfoContext;
        private readonly JsonResponse _jsonResponse;
        private readonly ImageToTextContext imageToTextContext;
        private readonly IMAGEContext iMAGEContext;

        public ValidationImageTextController(CLRCP_MASTERContext context,
                                VALIDATION_INFOContext ValidationInfoContext,
                                JsonResponse jsonResponse,
                                ImageToTextContext imageToTextContext,
                                IMAGEContext iMAGEContext)
        {
            _masterContext = context;
            _validationInfoContext = ValidationInfoContext;
            _jsonResponse = jsonResponse;
            this.imageToTextContext = imageToTextContext;
            this.iMAGEContext = iMAGEContext;
        }
        [HttpGet]
        [ActionName("GetValidationData")]
        public IActionResult GetValidationData_ImageText(int DatasetId, int UserId, int LanguageId, int DomainId)
        {
            if (DatasetId != 0 && UserId != 0 && LanguageId != 0 && DomainId != 0)
            {
                int? max_collection_user = _masterContext.Datasets.Where(x => x.DatasetId == DatasetId)
                                                           .Select(x => x.MaxCollectionUsers)
                                                           .SingleOrDefault();
                DatasetSubcategoryMapping datasetSubcategoryMapping = _masterContext.DatasetSubcategoryMapping
                                                            .Where(x => x.DatasetId == DatasetId)
                                                            .SingleOrDefault();

                if (datasetSubcategoryMapping != null)
                {
                    SubCategories sourcetableName = _masterContext.SubCategories.Find(datasetSubcategoryMapping.SourceSubcategoryId);
                    SubCategories destTableName = _masterContext.SubCategories.Find(datasetSubcategoryMapping.DestinationSubcategoryId);

                    if (destTableName.Name == "ImageText")
                    {
                        if (max_collection_user != null && max_collection_user != 0)
                        {
                            List<long> UsersvalidatedText = _validationInfoContext.ImagetextValidationResponseDetail.Where(x => x.UserId == UserId).Select(e => e.RefAutoid).ToList();
                            List<long> selectedText = imageToTextContext.ImageText.Where(x => x.UserId != UserId && x.IsValid == null
                                               && x.TotalValidationUsersCount < max_collection_user && x.OutputLangId == LanguageId && x.DomainId == DomainId)
                                              .Select(e => e.AutoId).ToList();

                            long id = selectedText.Except(UsersvalidatedText).FirstOrDefault();

                            var lst = selectedText.Except(UsersvalidatedText);

                            ValidationImageTextModel validationImageTextModel = imageToTextContext.ImageText.Where(x => x.AutoId == id)
                                               .Select(e => new ValidationImageTextModel
                                               {
                                                   DestAutoId = e.AutoId,
                                                   SourceDataId = e.DataId,
                                                   DestinationData = e.OutputData
                                               }).FirstOrDefault();
                            validationImageTextModel.SourceData =Convert.ToBase64String( iMAGEContext.Images.Where(x => x.DataId == validationImageTextModel.SourceDataId).Select(e => e.Image).FirstOrDefault());
                            validationImageTextModel.DatasetID = DatasetId;
                            return Ok(validationImageTextModel);
                        }
                        return NotFound();

                    }
                    return NotFound();
                }
            }
            return BadRequest();
        }

        [HttpGet]
        [ActionName("SetValidationData")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SetValidationData_ImageText(int UserId, int DestAutoId, int DatasetId, int IsMatch)
        {
            DatasetSubcategoryMappingValidation datasetSubcategoryMappingValidation = _masterContext.DatasetSubcategoryMappingValidation
                                                           .Where(x => x.DatasetId == DatasetId)
                                                           .SingleOrDefault();
            if (datasetSubcategoryMappingValidation != null)
            {
                SubCategories destTableNameValidation = _masterContext.SubCategories.Find(datasetSubcategoryMappingValidation.DestinationSubcategoryId);

                if (destTableNameValidation.Name == "IMAGETEXT_VALIDATION_RESPONSE_DETAIL")
                {
                    int IsValidFlag = 0;
                    if (IsMatch == 1 /*&& NoCrossTalk == 1 && IsClear == 1*/)
                    {
                        IsValidFlag = 1;
                    }
                    _validationInfoContext.ImagetextValidationResponseDetail.Add(new ImagetextValidationResponseDetail
                    {
                        UserId = UserId,
                        RefAutoid = DestAutoId,
                        IsMatch = IsMatch,
                       
                        //NoCrossTalk = NoCrossTalk,
                        //IsClear = IsClear,
                        ValidationFlag = IsValidFlag
                    });



                    ///set count
                    DatasetSubcategoryMapping datasetSubcategoryMapping = _masterContext.DatasetSubcategoryMapping
                                                            .Where(x => x.DatasetId == DatasetId)
                                                            .SingleOrDefault();

                    if (datasetSubcategoryMapping != null)
                    {
                        SubCategories destTableName = _masterContext.SubCategories.Find(datasetSubcategoryMapping.DestinationSubcategoryId);

                        if (destTableName.Name == "ImageText")
                        {
                            ImageText imageText = imageToTextContext.ImageText.Where(x => x.AutoId == DestAutoId).Select(x => x).SingleOrDefault();
                            if (imageText != null)
                            {
                                imageText.TotalValidationUsersCount += 1;
                                if (IsMatch == 1 /*&& NoCrossTalk == 1 && IsClear == 1*/)
                                {
                                    imageText.VoteCount += 1;

                                }
                                int? maxValidationUsers = _masterContext.Datasets.Where(x => x.DatasetId == DatasetId)
                                                           .Select(x => x.MaxValidationUsers)
                                                           .FirstOrDefault();
                                if (maxValidationUsers != null)
                                {
                                    if (maxValidationUsers * 0.5 < imageText.VoteCount)
                                    {
                                        imageText.IsValid = 1;
                                    }
                                    else if (maxValidationUsers * 0.5 < (imageText.TotalValidationUsersCount - imageText.VoteCount))
                                    {
                                        imageText.IsValid = 0;
                                    }

                                    try
                                    {
                                        _validationInfoContext.SaveChangesAsync();
                                        imageToTextContext.SaveChangesAsync();
                                    }
                                    catch (Exception)
                                    {
                                        _jsonResponse.Response = "Internal Exception";
                                        return BadRequest(_jsonResponse);
                                    }
                                    _jsonResponse.IsSuccessful = true;
                                    _jsonResponse.Response = "Saved";
                                    return Ok(_jsonResponse);
                                }
                            }
                        }
                    }
                }
            }
            _jsonResponse.IsSuccessful = false;
            _jsonResponse.Response = "Details not match";
            return NotFound(_jsonResponse);
        }

    }
}
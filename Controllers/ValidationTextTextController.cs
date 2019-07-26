using System;
using System.Collections.Generic;
using System.Linq;
using CRLCP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRLCP.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValidationTextTextController : ControllerBase
    {
        private readonly CLRCP_MASTERContext _MasterContext;
        private readonly VALIDATION_INFOContext _validationInfoContext;
        private readonly JsonResponse _jsonResponse;
        private readonly TEXTContext _textContext;
        private readonly TextToTextContext _textToTextContext;

        public ValidationTextTextController(CLRCP_MASTERContext context,
                                VALIDATION_INFOContext ValidationInfoContext,
                                JsonResponse jsonResponse,
                                TEXTContext textContext,
                                TextToTextContext textToTextContext)
        {
            _MasterContext = context;
            _validationInfoContext = ValidationInfoContext;
            _jsonResponse = jsonResponse;
            _textContext = textContext;
            _textToTextContext = textToTextContext;
        }


        [HttpGet]
        [ActionName("GetValidationData")]
        public IActionResult GetValidationData_TextText(int DatasetId, int UserId, int SourceLangId, int DestLangId, int DomainId)
        {
            if (DatasetId != 0 && UserId != 0 && SourceLangId != 0  && DomainId != 0)
            {
                //get max user which process the validation for current dataset
                int? max_collection_user = _MasterContext.Datasets.Where(x => x.DatasetId == DatasetId)
                                                           .Select(x => x.MaxCollectionUsers)
                                                           .SingleOrDefault();

                //find dataset subcategory mapping { source table and destination table }
                DatasetSubcategoryMapping datasetSubcategoryMapping = _MasterContext.DatasetSubcategoryMapping
                                                            .Where(x => x.DatasetId == DatasetId)
                                                            .SingleOrDefault();

                if (datasetSubcategoryMapping != null)
                {
                    string sourcetableName = _MasterContext.SubCategories.Find(datasetSubcategoryMapping.SourceSubcategoryId).Name;
                    string destTableName = _MasterContext.SubCategories.Find(datasetSubcategoryMapping.DestinationSubcategoryId).Name;

                    if (destTableName == "TextText")
                    {
                        if (max_collection_user != null && max_collection_user != 0)
                        {
                            //Get all sentences id which is already validate by user from Validation Info Database 
                            List<long> UsersvalidatedText = _validationInfoContext.TexttextValidationResponseDetail.Where(x => x.UserId == UserId).Select(e => e.RefAutoid).ToList();
                            //get all sentences which is not contributed by current user and other filters
                            List<long> SentencesToProcess = _textToTextContext.TextText.Where(x => x.UserId != UserId && x.IsValid == null
                                               && x.TotalValidationUsersCount < max_collection_user && x.OutputLangId == DestLangId && x.DomainId == DomainId)
                                              .Select(e => e.AutoId).ToList();
                            //substract already validate sentences from remaining sentences {get id of first sentence}
                            long id = SentencesToProcess.Except(UsersvalidatedText).FirstOrDefault();

                            ValidationTextToTextModel validationTexToTextModel = _textToTextContext.TextText.Where(x => x.AutoId == id)
                                               .Select(e => new ValidationTextToTextModel
                                               {
                                                   DestAutoId = e.AutoId,
                                                   SourceDataId = e.DataId,
                                                   DestinationData = e.OutputData
                                               }).FirstOrDefault();
                            validationTexToTextModel.SourceData = _textContext.Text.Where(x => x.DataId == validationTexToTextModel.SourceDataId).Select(e => e.Text1).FirstOrDefault();
                            validationTexToTextModel.DatasetID = DatasetId;
                            return Ok(validationTexToTextModel);
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
            DatasetSubcategoryMappingValidation datasetSubcategoryMappingValidation = _MasterContext.DatasetSubcategoryMappingValidation
                                                           .Where(x => x.DatasetId == DatasetId)
                                                           .SingleOrDefault();
            if (datasetSubcategoryMappingValidation != null)
            {
                SubCategories destTableNameValidation = _MasterContext.SubCategories.Find(datasetSubcategoryMappingValidation.DestinationSubcategoryId);

                if (destTableNameValidation.Name == "TEXTTEXT_VALIDATION_RESPONSE_DETAIL")
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
                    DatasetSubcategoryMapping datasetSubcategoryMapping = _MasterContext.DatasetSubcategoryMapping
                                                            .Where(x => x.DatasetId == DatasetId)
                                                            .SingleOrDefault();

                    if (datasetSubcategoryMapping != null)
                    {
                        SubCategories destTableName = _MasterContext.SubCategories.Find(datasetSubcategoryMapping.DestinationSubcategoryId);

                        if (destTableName.Name == "ImageText")
                        {
                            TextText imageText = _textToTextContext.TextText.Where(x => x.AutoId == DestAutoId).Select(x => x).SingleOrDefault();
                            if (imageText != null)
                            {
                                imageText.TotalValidationUsersCount += 1;
                                if (IsMatch == 1 /*&& NoCrossTalk == 1 && IsClear == 1*/)
                                {
                                    imageText.VoteCount += 1;

                                }
                                int? maxValidationUsers = _MasterContext.Datasets.Where(x => x.DatasetId == DatasetId)
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
                                        _textToTextContext.SaveChangesAsync();
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
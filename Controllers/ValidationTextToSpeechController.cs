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
    public class ValidationTextToSpeechController : ControllerBase
    {
        private readonly CLRCP_MASTERContext _masterContext;
        private readonly TEXTContext _TEXTcontext;
        private readonly TextToSpeechContext _texttoSpeechContext;
        private readonly VALIDATION_INFOContext _validationInfoContext;
        private readonly JsonResponse _jsonResponse;

        public ValidationTextToSpeechController(CLRCP_MASTERContext context,
                                TEXTContext TEXTContext,
                                TextToSpeechContext textToSpeech,
                                VALIDATION_INFOContext ValidationInfoContext,
                                JsonResponse jsonResponse)
        {
            _masterContext = context;
            _TEXTcontext = TEXTContext;
            _texttoSpeechContext = textToSpeech;
            _validationInfoContext = ValidationInfoContext;
            _jsonResponse = jsonResponse;
        }

        [HttpGet]
        [ActionName("GetValidationData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetValidationData_TextSpeech(int DatasetId, int UserId, int LanguageId, int DomainId)
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

                    if (destTableName.Name == "TextSpeech")
                    {
                        if (max_collection_user != null && max_collection_user != 0)
                        {
                            List<long> allText = _validationInfoContext.TextspeechValidationResponseDetail.Where(x => x.UserId == UserId).Select(e => e.RefAutoid).ToList();
                            List<long> selectedText = _texttoSpeechContext.TextSpeech.Where(x => x.UserId != UserId && x.IsValid == null
                                               && x.TotalValidationUsersCount < max_collection_user && x.LangId == LanguageId && x.DomainId == DomainId)
                                              .Select(e => e.AutoId).ToList();

                            long id = selectedText.Except(allText).FirstOrDefault();

                            var lst = selectedText.Except(allText);

                            ValidationTextSpeechModel validationTextSpeechModel = _texttoSpeechContext.TextSpeech.Where(x=>x.AutoId == id)
                                               .Select(e => new ValidationTextSpeechModel
                                               {
                                                   DestAutoId = e.AutoId,
                                                   SourceDataId = e.DataId,
                                                   DestinationData = e.OutputData
                                               }).FirstOrDefault();
                            validationTextSpeechModel.SourceData = _TEXTcontext.Text.Where(x => x.DataId == validationTextSpeechModel.SourceDataId).Select(e => e.Text1).FirstOrDefault();
                            validationTextSpeechModel.DatasetID = DatasetId;

                            /*ValidationTextSpeechModel validationTextSpeechModel = textToSpeech.TextSpeech.Where(x => x.UserId != UserId && x.IsValid == null
                                               && x.TotalValidationUsersCount < max_collection_user && x.LangId == LanguageId && x.DomainId == DomainId )
                                               .Select(e => new ValidationTextSpeechModel
                                               {
                                                   DestAutoId = e.AutoId,
                                                   SourceDataId = e.DataId,
                                                   DestinationData = e.OutputData
                                               }).FirstOrDefault();
                            validationTextSpeechModel.SourceData = _TEXTcontext.Text.Where(x => x.DataId == validationTextSpeechModel.SourceDataId).Select(e => e.Text1).FirstOrDefault();
                            validationTextSpeechModel.DatasetID = DatasetId;*/
                            return Ok(validationTextSpeechModel);
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
        public IActionResult SetValidationData_TextSpeech(int UserId,int DestAutoId , int DatasetId,int IsMatch)
        {
            DatasetSubcategoryMappingValidation datasetSubcategoryMappingValidation = _masterContext.DatasetSubcategoryMappingValidation
                                                           .Where(x => x.DatasetId == DatasetId)
                                                           .SingleOrDefault();
            if (datasetSubcategoryMappingValidation != null)
            {
                SubCategories destTableNameValidation = _masterContext.SubCategories.Find(datasetSubcategoryMappingValidation.DestinationSubcategoryId);

                if (destTableNameValidation.Name == "TEXTSPEECH_VALIDATION_RESPONSE_DETAIL")
                {
                    int IsValidFlag = 0;
                    if (IsMatch == 1 /*&& NoCrossTalk == 1 && IsClear == 1*/)
                    {
                        IsValidFlag = 1;
                    }
                    _validationInfoContext.TextspeechValidationResponseDetail.Add(new TextspeechValidationResponseDetail
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

                        if (destTableName.Name == "TextSpeech")
                        {
                            TextSpeech textSpeech = _texttoSpeechContext.TextSpeech.Where(x => x.AutoId == DestAutoId).Select(x => x).SingleOrDefault();
                            if (textSpeech != null)
                            {
                                textSpeech.TotalValidationUsersCount += 1;
                                if (IsMatch == 1 /*&& NoCrossTalk == 1 && IsClear == 1*/)
                                {
                                    textSpeech.VoteCount += 1;
                                    
                                }
                                int? maxValidationUsers = _masterContext.Datasets.Where(x => x.DatasetId == DatasetId)
                                                           .Select(x => x.MaxValidationUsers)
                                                           .FirstOrDefault();
                                if (maxValidationUsers != null)
                                {
                                    if (maxValidationUsers * 0.5 < textSpeech.VoteCount)
                                    {
                                        textSpeech.IsValid = 1;
                                    }
                                    else if (maxValidationUsers * 0.5 < (textSpeech.TotalValidationUsersCount - textSpeech.VoteCount))
                                    {
                                        textSpeech.IsValid = 0;
                                    }

                                    try
                                    {
                                        _validationInfoContext.SaveChangesAsync();
                                        _texttoSpeechContext.SaveChangesAsync();
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
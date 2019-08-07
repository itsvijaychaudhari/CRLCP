using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRLCP.Dashboard;
using CRLCP.Models;
using CRLCP.Models.DBO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;


namespace CRLCP.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly CLRCP_MASTERContext _CLRCP_MASTERContext;
        private readonly TEXTContext _TEXTContext;
        private JsonResponse jsonResponse;
        private readonly IMAGEContext _IMAGEContext;

        public MasterController(CLRCP_MASTERContext context,
                                TEXTContext textContext,
                                JsonResponse JsonResponse,
                                IMAGEContext iMAGEContext)
        {
            _CLRCP_MASTERContext = context;
            _TEXTContext = textContext;
            jsonResponse = JsonResponse;
            _IMAGEContext = iMAGEContext;
        }

        [HttpGet]
        [ActionName("GetListDataset")]
        //API TO GET HOMEPAGE DASHBOARD COUNTS user wise (ADMIN HOMEPAGE)
        public IEnumerable<SelectListItem> GetDataSet()
        {
            //List<SelectListItem>
            var _datasets = _CLRCP_MASTERContext.Datasets;
            IEnumerable<SelectListItem> _DataSetListItems = _datasets.Select(e => new SelectListItem
            {
                Value = e.DatasetId.ToString(),
                Text = e.Description
            });
            //_DataSetListItems.Sort(Function(a, b) a.Text < b.Text)
            return _DataSetListItems;
        }

        [HttpGet]    //API TO GET HOMEPAGE DASHBOARD COUNTS user wise (ADMIN HOMEPAGE)
        public List<SelectListItem> GetLanguageIDs()
        {
            //List<SelectListItem>
            var _languageIds = _CLRCP_MASTERContext.LanguageIdMapping;
            List<SelectListItem> _languageIdsListItems = _languageIds.Select(e => new SelectListItem
            {
                Value = e.LanguageId.ToString(),
                Text = e.Description
            }).ToList();
            //_DataSetListItems.Sort(Function(a, b) a.Text < b.Text)
            return _languageIdsListItems;
        }

        //source_ID
        //Domain_ID
        [HttpGet]    //API TO GET HOMEPAGE DASHBOARD COUNTS user wise (ADMIN HOMEPAGE)
        public List<SelectListItem> GetSourceIDs()
        {
            //List<SelectListItem>
            var _sourceIds = _CLRCP_MASTERContext.SourceIdMapping;
            List<SelectListItem> _sourceIdsListItems = _sourceIds.Select(e => new SelectListItem
            {
                Value = e.SourceId.ToString(),
                Text = e.Value
            }).ToList();
            //_DataSetListItems.Sort(Function(a, b) a.Text < b.Text)
            return _sourceIdsListItems;
        }

        [HttpGet]    //API TO GET HOMEPAGE DASHBOARD COUNTS user wise (ADMIN HOMEPAGE)
        public List<SelectListItem> GetDomainIDs()
        {
            //List<SelectListItem>
            var _domainIds = _CLRCP_MASTERContext.DomainIdMapping;
            List<SelectListItem> _domainIdsListItems = _domainIds.Select(e => new SelectListItem
            {
                Value = e.DomainId.ToString(),
                Text = e.Value
            }).ToList();
            //_DataSetListItems.Sort(Function(a, b) a.Text < b.Text)
            return _domainIdsListItems;
        }


        [HttpPost]
        public bool AddText([FromBody]TextModel _textModel)
        {
            bool success = false;
            int _newDomainId = _textModel.DomainId;
            try
            {
                CommonServices _commonServices = new CommonServices();
                if (_textModel.DomainId == 0)
                {
                    _newDomainId = _commonServices.AddDomainIDs(_textModel.NewDomainToAdd.ToString());
                }



                string[] lines = _textModel.Text1.Split(new[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Count(); i++)
                {
                    Text model = new Text();
                    model.DomainId = _newDomainId;
                    model.DatasetId = _textModel.DatasetId;
                    model.AddedOn = _textModel.AddedOn;
                    model.AdditionalInfo = _textModel.AdditionalInfo;
                    model.DataId = _textModel.DataId;
                    model.LangId = _textModel.LangId;
                    model.SourceId = _textModel.SourceId;
                    model.Text1 = lines[i];

                    _TEXTContext.Add(model);

                    success = true;
                }
                _TEXTContext.SaveChanges();
            }
            catch (Exception )
            {
                success = false;
            }
            return success;
        }

        [HttpPost]
        public IActionResult EditDataset(Datasets _datasets)
        {
            try
            {
                var datasets = _CLRCP_MASTERContext.Datasets.Where(e => e.DatasetId == _datasets.DatasetId).FirstOrDefault();
                datasets.Name = _datasets.Name;
                datasets.Description = _datasets.Description;
                datasets.MaxCollectionUsers = _datasets.MaxCollectionUsers;
                datasets.MaxValidationUsers = _datasets.MaxValidationUsers;
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }

        [HttpGet]
        [ActionName("GetDataset")]
        public List<Datasets> GetDataset()
        {
            try
            {
                List<Datasets> _datasetList = _CLRCP_MASTERContext.Datasets.ToList();
                return _datasetList;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteDataset(Datasets _datasets)
        {
            try
            {
                var datasets = _CLRCP_MASTERContext.Datasets.Where(e => e.DatasetId == _datasets.DatasetId).FirstOrDefault();

                _CLRCP_MASTERContext.Datasets.Remove(datasets);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddDataset(Datasets _datasets)
        {
            try
            {
                Datasets datasets = new Datasets
                {
                    Name = _datasets.Name,
                    Description = _datasets.Description,
                    MaxCollectionUsers = _datasets.MaxCollectionUsers,
                    MaxValidationUsers = _datasets.MaxValidationUsers,
                    IsVisible = _datasets.IsVisible
                };
                _CLRCP_MASTERContext.Datasets.Add(datasets);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }



        [HttpPost]
        public IActionResult EditDatasetSubcategoryMapping(DatasetSubcategoryMapping _datasetSubcategoryMapping)
        {
            try
            {
                var datasetSubcategoryMapping = _CLRCP_MASTERContext.DatasetSubcategoryMapping.Where(e => e.AutoId == _datasetSubcategoryMapping.AutoId).FirstOrDefault();
                datasetSubcategoryMapping.DatasetId = _datasetSubcategoryMapping.DatasetId;
                datasetSubcategoryMapping.SourceSubcategoryId = _datasetSubcategoryMapping.SourceSubcategoryId;
                datasetSubcategoryMapping.DestinationSubcategoryId = _datasetSubcategoryMapping.DestinationSubcategoryId;
                datasetSubcategoryMapping.SourceSubcategoryId2 = _datasetSubcategoryMapping.SourceSubcategoryId2;
                datasetSubcategoryMapping.DestinationSubcategoryId2 = _datasetSubcategoryMapping.DestinationSubcategoryId2;
                datasetSubcategoryMapping.SourceSubcategoryId3 = _datasetSubcategoryMapping.SourceSubcategoryId3;
                datasetSubcategoryMapping.DestinationSubcategoryId3 = _datasetSubcategoryMapping.DestinationSubcategoryId3;
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<DatasetSubcategoryMapping> GetDatasetSubcategoryMapping()
        {
            try
            {
                List<DatasetSubcategoryMapping> _datasetSubcategoryMapping = _CLRCP_MASTERContext.DatasetSubcategoryMapping.ToList();
                return _datasetSubcategoryMapping;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteDatasetSubcategoryMapping(DatasetSubcategoryMapping _datasetSubcategoryMapping)
        {
            try
            {
                var datasetSubcategoryMapping = _CLRCP_MASTERContext.DatasetSubcategoryMapping.Where(e => e.AutoId == _datasetSubcategoryMapping.AutoId).FirstOrDefault();

                _CLRCP_MASTERContext.DatasetSubcategoryMapping.Remove(datasetSubcategoryMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddDatasetSubcategoryMapping(DatasetSubcategoryMapping _datasetSubcategoryMapping)
        {
            try
            {
                DatasetSubcategoryMapping datasetSubcategoryMapping = new DatasetSubcategoryMapping
                {
                    DatasetId = _datasetSubcategoryMapping.DatasetId,
                    SourceSubcategoryId = _datasetSubcategoryMapping.SourceSubcategoryId,
                    DestinationSubcategoryId = _datasetSubcategoryMapping.DestinationSubcategoryId,
                    SourceSubcategoryId2 = _datasetSubcategoryMapping.SourceSubcategoryId2,
                    DestinationSubcategoryId2 = _datasetSubcategoryMapping.DestinationSubcategoryId2,
                    SourceSubcategoryId3 = _datasetSubcategoryMapping.SourceSubcategoryId3,
                    DestinationSubcategoryId3 = _datasetSubcategoryMapping.DestinationSubcategoryId3
                };
                _CLRCP_MASTERContext.DatasetSubcategoryMapping.Add(datasetSubcategoryMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }



        [HttpPost]
        public IActionResult EditCategories(Categories _catagories)
        {
            try
            {
                var catagories = _CLRCP_MASTERContext.Categories.Where(e => e.CategoryId == _catagories.CategoryId).FirstOrDefault();

                catagories.Name = _catagories.Name;
                catagories.Description = _catagories.Description;

                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<Categories> GetCategories()
        {
            try
            {
                List<Categories> _catagories = _CLRCP_MASTERContext.Categories.ToList();
                return _catagories;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteCategories(Categories _catagories)
        {
            try
            {
                var catagories = _CLRCP_MASTERContext.Categories.Where(e => e.CategoryId == _catagories.CategoryId).FirstOrDefault();

                _CLRCP_MASTERContext.Categories.Remove(catagories);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddCategories(Categories _catagories)
        {
            try
            {
                Categories catagories = new Categories
                {
                    Name = _catagories.Name,
                    Description = _catagories.Description
                };
                _CLRCP_MASTERContext.Categories.Add(catagories);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }


        [HttpPost]
        public IActionResult EditDatasetSubcategoryMappingValidation(DatasetSubcategoryMappingValidation _datasetSubcategoryMappingValidation)
        {
            try
            {
                var datasetSubcategoryMappingValidation = _CLRCP_MASTERContext.DatasetSubcategoryMappingValidation.Where(e => e.AutoId == _datasetSubcategoryMappingValidation.AutoId).FirstOrDefault();

                datasetSubcategoryMappingValidation.DatasetId = _datasetSubcategoryMappingValidation.DatasetId;
                datasetSubcategoryMappingValidation.DestinationSubcategoryId = _datasetSubcategoryMappingValidation.DestinationSubcategoryId;

                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<DatasetSubcategoryMappingValidation> GetDatasetSubcategoryMappingValidation()
        {
            try
            {
                List<DatasetSubcategoryMappingValidation> _datasetSubcategoryMappingValidation = _CLRCP_MASTERContext.DatasetSubcategoryMappingValidation.ToList();
                return _datasetSubcategoryMappingValidation;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteDatasetSubcategoryMappingValidation(DatasetSubcategoryMappingValidation _datasetSubcategoryMappingValidation)
        {
            try
            {
                var datasetSubcategoryMappingValidation = _CLRCP_MASTERContext.DatasetSubcategoryMappingValidation.Where(e => e.AutoId == _datasetSubcategoryMappingValidation.AutoId).FirstOrDefault();

                _CLRCP_MASTERContext.DatasetSubcategoryMappingValidation.Remove(datasetSubcategoryMappingValidation);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddDatasetSubcategoryMappingValidation(DatasetSubcategoryMappingValidation _datasetSubcategoryMappingValidation)
        {
            try
            {
                DatasetSubcategoryMappingValidation datasetSubcategoryMappingValidation = new DatasetSubcategoryMappingValidation
                {
                    DatasetId = _datasetSubcategoryMappingValidation.DatasetId,
                    DestinationSubcategoryId = _datasetSubcategoryMappingValidation.DestinationSubcategoryId
                };
                _CLRCP_MASTERContext.DatasetSubcategoryMappingValidation.Add(datasetSubcategoryMappingValidation);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }



        [HttpPost]
        public IActionResult EditDomainIdMapping(DomainIdMapping _domainIdMapping)
        {
            try
            {
                var domainIdMapping = _CLRCP_MASTERContext.DomainIdMapping.Where(e => e.DomainId == _domainIdMapping.DomainId).FirstOrDefault();


                domainIdMapping.Value = _domainIdMapping.Value;

                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<DomainIdMapping> GetDomainIdMapping()
        {
            try
            {
                List<DomainIdMapping> _domainIdMapping = _CLRCP_MASTERContext.DomainIdMapping.ToList();
                return _domainIdMapping;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteDomainIdMapping(DomainIdMapping _domainIdMapping)
        {
            try
            {
                var domainIdMapping = _CLRCP_MASTERContext.DomainIdMapping.Where(e => e.DomainId == _domainIdMapping.DomainId).FirstOrDefault();

                _CLRCP_MASTERContext.DomainIdMapping.Remove(domainIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddDomainIdMapping(DomainIdMapping _domainIdMapping)
        {
            try
            {
                DomainIdMapping domainIdMapping = new DomainIdMapping
                {
                    Value = _domainIdMapping.Value,

                };
                _CLRCP_MASTERContext.DomainIdMapping.Add(domainIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }



        [HttpPost]
        public IActionResult EditLanguageIdMapping(LanguageIdMapping _languageIdMapping)
        {
            try
            {
                var languageIdMapping = _CLRCP_MASTERContext.LanguageIdMapping.Where(e => e.LanguageId == _languageIdMapping.LanguageId).FirstOrDefault();


                languageIdMapping.Value = _languageIdMapping.Value;
                languageIdMapping.Description = _languageIdMapping.Description;

                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<LanguageIdMapping> GetLanguageIdMapping()
        {
            try
            {
                List<LanguageIdMapping> _languageIdMapping = _CLRCP_MASTERContext.LanguageIdMapping.ToList();
                return _languageIdMapping;
            }
            catch (Exception )
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteLanguageIdMapping(LanguageIdMapping _languageIdMapping)
        {
            try
            {
                var domainIdMapping = _CLRCP_MASTERContext.LanguageIdMapping.Where(e => e.LanguageId == _languageIdMapping.LanguageId).FirstOrDefault();

                _CLRCP_MASTERContext.LanguageIdMapping.Remove(domainIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddLanguageIdMapping(LanguageIdMapping _languageIdMapping)
        {
            try
            {
                LanguageIdMapping languageIdMapping = new LanguageIdMapping
                {
                    Value = _languageIdMapping.Value,
                    Description = _languageIdMapping.Description

                };
                _CLRCP_MASTERContext.LanguageIdMapping.Add(languageIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }




        [HttpPost]
        public IActionResult EditQualificationIdMapping(QualificationIdMapping _qualificationIdMapping)
        {
            try
            {
                var qualificationIdMapping = _CLRCP_MASTERContext.QualificationIdMapping.Where(e => e.QualificationId == _qualificationIdMapping.QualificationId).FirstOrDefault();
                qualificationIdMapping.Value = _qualificationIdMapping.Value;
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<QualificationIdMapping> GetQualificationIdMapping()
        {
            try
            {
                List<QualificationIdMapping> _qualificationIdMapping = _CLRCP_MASTERContext.QualificationIdMapping.ToList();
                return _qualificationIdMapping;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteQualificationIdMapping(QualificationIdMapping _qualificationIdMapping)
        {
            try
            {
                var qualificationIdMapping = _CLRCP_MASTERContext.QualificationIdMapping.Where(e => e.QualificationId == _qualificationIdMapping.QualificationId).FirstOrDefault();

                _CLRCP_MASTERContext.QualificationIdMapping.Remove(qualificationIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddQualificationIdMapping(QualificationIdMapping _qualificationIdMapping)
        {
            try
            {
                QualificationIdMapping qualificationIdMapping = new QualificationIdMapping
                {
                    Value = _qualificationIdMapping.Value,


                };
                _CLRCP_MASTERContext.QualificationIdMapping.Add(qualificationIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }



        [HttpPost]
        public IActionResult EditSourceIdMapping(SourceIdMapping _sourceIdMapping)
        {
            try
            {
                var sourceIdMapping = _CLRCP_MASTERContext.SourceIdMapping.Where(e => e.SourceId == _sourceIdMapping.SourceId).FirstOrDefault();
                sourceIdMapping.Value = _sourceIdMapping.Value;
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception )
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<SourceIdMapping> GetSourceIdMapping()
        {
            try
            {
                List<SourceIdMapping> _sourceIdMapping = _CLRCP_MASTERContext.SourceIdMapping.ToList();
                return _sourceIdMapping;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteSourceIdMapping(SourceIdMapping _sourceIdMapping)
        {
            try
            {
                var sourceIdMapping = _CLRCP_MASTERContext.SourceIdMapping.Where(e => e.SourceId == _sourceIdMapping.SourceId).FirstOrDefault();

                _CLRCP_MASTERContext.SourceIdMapping.Remove(sourceIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddSourceIdMapping(SourceIdMapping _sourceIdMapping)
        {
            try
            {
                SourceIdMapping sourceIdMapping = new SourceIdMapping
                {
                    Value = _sourceIdMapping.Value,


                };
                _CLRCP_MASTERContext.SourceIdMapping.Add(sourceIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult EditSubCategories(SubCategories _subCategories)
        {
            try
            {
                var sourceIdMapping = _CLRCP_MASTERContext.SubCategories.Where(e => e.SubcategoryId == _subCategories.SubcategoryId).FirstOrDefault();
                sourceIdMapping.CategoryId = _subCategories.CategoryId;
                sourceIdMapping.Name = _subCategories.Name;
                sourceIdMapping.Description = _subCategories.Description;
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }
        [HttpGet]
        public List<SubCategories> GetSubCategories()
        {
            try
            {
                List<SubCategories> _subCategories = _CLRCP_MASTERContext.SubCategories.ToList();
                return _subCategories;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpPost]
        public IActionResult DeleteSubCategories(SubCategories _subCategories)
        {
            try
            {
                var sourceIdMapping = _CLRCP_MASTERContext.SubCategories.Where(e => e.SubcategoryId == _subCategories.SubcategoryId).FirstOrDefault();

                _CLRCP_MASTERContext.SubCategories.Remove(sourceIdMapping);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }
        [HttpPost]
        public IActionResult AddSubCategories(SubCategories _subCategories)
        {
            try
            {
                SubCategories subCategories = new SubCategories
                {
                    CategoryId = _subCategories.CategoryId,
                    Name = _subCategories.Name,
                    Description = _subCategories.Description

                };
                _CLRCP_MASTERContext.SubCategories.Add(subCategories);
                _CLRCP_MASTERContext.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult SaveLangAccToUserId([FromBody]UsersLanguage model)
        {
            try
            {
                //delete record from UserLanguageMapping table 
                List<UserLanguageMapping> userLanguage = _CLRCP_MASTERContext.UserLanguageMapping.Where(x => x.UserId == model.UserId).ToList();
                if (userLanguage.Count > 0)
                {
                    _CLRCP_MASTERContext.UserLanguageMapping.RemoveRange(userLanguage);
                    _CLRCP_MASTERContext.SaveChanges();
                }

                //List<UserLanguageMapping> userLanguage =  _context.UserLanguageMapping.Where(x => x.UserId == model.UserId).ToList();

                //foreach (int lang in model.languageId)
                //{
                //    if (!userLanguage.Contains(new UserLanguageMapping { UserId = model.UserId, LanguageId = lang }))
                //    {
                //        userLanguage.Add(new UserLanguageMapping
                //        {
                //            UserId = model.UserId,
                //            LanguageId = lang
                //        });
                //    }
                //}

                //if (userLanguage.Count > 0)
                //{
                //    _context.UserLanguageMapping.UpdateRange(userLanguage);
                //    var obj = _context.SaveChanges();
                //}

                //insert all value in UserLanguageMapping
                int count = model.languageId.Count();
                for (int i = 0; i < count; i++)
                {
                    _CLRCP_MASTERContext.UserLanguageMapping.Add(new UserLanguageMapping
                    {
                        UserId = model.UserId,
                        LanguageId = model.languageId[i]
                    });
                }
                _CLRCP_MASTERContext.SaveChanges();
                jsonResponse.IsSuccessful = true;
                jsonResponse.Response = "Data saved successfully";
                return Ok(jsonResponse);
            }
            catch (Exception)
            {
                jsonResponse.IsSuccessful = true;
                jsonResponse.Response = "Exception Occured";
                return BadRequest(jsonResponse);
            }
        }


        [HttpGet]
        [ActionName("GetLanguage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult GetLangUserWise(int UserId)
        {
            try
            {
                //UsersLanguage usersLanguage = new UsersLanguage();
               // List<UserLanguageMapping> list = _context.UserLanguageMapping.Where(e => e.UserId == UserId).ToList();
                List<int> language = _CLRCP_MASTERContext.UserLanguageMapping.Where(e => e.UserId == UserId).Select(y=>y.LanguageId).ToList();

               // usersLanguage.UserId = UserId;
                //usersLanguage.languageId = _context.LanguageIdMapping.Where(e => list.(x => x.LanguageId)) .ToList();
                List<LanguageIdMapping> languageIdMappings = _CLRCP_MASTERContext.LanguageIdMapping.Where(x => language.Contains(x.LanguageId)).ToList();

                return Ok(languageIdMappings);
            }
            catch (Exception)
            {
                return BadRequest(jsonResponse.Response = "Internal Exception");
            }
        }


        [HttpGet]
        [ActionName("GetDomainId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult GetDomainIdFromText(int DatasetId)
        {
            jsonResponse.Response = "Internal Server Error";
            try
            {
                List<DatasetSubcategoryMapping> lstdatasetSubCatMap = _CLRCP_MASTERContext.DatasetSubcategoryMapping.ToList();
                int sourceTableId = lstdatasetSubCatMap.Where(e => e.DatasetId == DatasetId).Select(e => e.SourceSubcategoryId).FirstOrDefault();
                string SourceDBName = _CLRCP_MASTERContext.SubCategories.Where(e => e.SubcategoryId == sourceTableId).Select(e => e.Name).FirstOrDefault();
                if (SourceDBName == "Text")
                {
                    var _list = new HashSet<int>(_TEXTContext.Text.Select(e => e.DomainId));
                    List<DomainIdMapping> _domainIdMapping = _CLRCP_MASTERContext.DomainIdMapping.ToList();
                    List<DomainIdMapping> _domainIdMappingoutput = _domainIdMapping.Where(e => _list.Contains(e.DomainId)).ToList();
                    return Ok(_domainIdMappingoutput);
                }
                else if (SourceDBName == "Images")
                {
                    var _list = new HashSet<int>(_IMAGEContext.Images.Select(e => e.DomainId));
                    List<DomainIdMapping> _domainIdMapping = _CLRCP_MASTERContext.DomainIdMapping.ToList();
                    List<DomainIdMapping> _domainIdMappingoutput = _domainIdMapping.Where(e => _list.Contains(e.DomainId)).ToList();
                    return Ok(_domainIdMappingoutput);
                }
            }
            catch (Exception)
            {
                
                return BadRequest(jsonResponse);
            }
            return BadRequest(jsonResponse);
        }

    }
}
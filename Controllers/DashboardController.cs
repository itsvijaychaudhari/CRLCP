using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRLCP.Dashboard;
using CRLCP.Helper;
using CRLCP.Models;
using CRLCP.Models.Dashboard;
using CRLCP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRLCP.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
       
        
        private CLRCP_MASTERContext _context;
        private TEXTContext _textContext;
        private TextToSpeechContext _textToSpeechContext;
        private IMAGEContext _imageContext;
        private ImageToTextContext _imageToTextContext;
        private TextToTextContext _textToTextContext;
        private readonly VALIDATION_INFOContext vALIDATION_INFOContext;

        public DashboardController(CLRCP_MASTERContext context,
                                   TEXTContext textContext, 
                                   TextToSpeechContext textToSpeechContext,
                                   IMAGEContext imageContext, 
                                   ImageToTextContext imageToTextContext,
                                   TextToTextContext textToTextContext,
                                   VALIDATION_INFOContext vALIDATION_INFOContext,
                                   IOptions<AppSettings> appSettings)
        {
            _context = context;
            _textContext = textContext;
            _textToSpeechContext = textToSpeechContext;
            _imageContext = imageContext;
            _imageToTextContext = imageToTextContext;
            _textToTextContext = textToTextContext;
            this.vALIDATION_INFOContext = vALIDATION_INFOContext;
        }

        

        //POST api/Dashboard
        [HttpGet]    //API TO GET HOMEPAGE DASHBOARD COUNTS (ADMIN HOMEPAGE)
        public List<HomePageModel> GetHomePage()
        {
            List<HomePageModel> GeneralHomePageDataList = new List<HomePageModel>();
            try
            {
                List<DatasetSubcategoryMapping> model = new List<DatasetSubcategoryMapping>();
                model = _context.DatasetSubcategoryMapping.ToList();
                int sourceDbCount = 0;
                int destinationDbCount = 0;
                int valDestinationDbCount = 0;
                //foreach to fetch overall source and destination counts per dataset
                //for eg: ASR     source count:2  destination count: 15
                foreach (var item in model)
                {
                    
                    HomePageModel homePageCount = new HomePageModel();
                    destinationDbCount = 0;
                    sourceDbCount = 0;
                    valDestinationDbCount = 0;

                    string DatasetName = Convert.ToString(_context.Datasets.Where(e => e.DatasetId == item.DatasetId).Select(e => e.Name).FirstOrDefault()) ?? "";

                    string SourceDB = Convert.ToString(_context.SubCategories.Where(e => e.SubcategoryId == item.SourceSubcategoryId).Select(e => e.Name).FirstOrDefault()) ?? "";

                    string DestinationDB = Convert.ToString(_context.SubCategories.Where(e => e.SubcategoryId == item.DestinationSubcategoryId).Select(e => e.Name).FirstOrDefault()) ?? "";

                    if(SourceDB == "Text")
                    {
                         sourceDbCount = _textContext.Text.Where(e => e.DatasetId == item.DatasetId).Count();
                        if (DestinationDB == "TextSpeech")
                        {
                            destinationDbCount = _textToSpeechContext.TextSpeech.Where(e => e.DatasetId == item.DatasetId).Count();
                            //List<long> textSpeeches = _textToSpeechContext.TextSpeech.Where(e => e.DatasetId == item.DatasetId).Select(x=>x.AutoId).ToList();
                            //valDestinationDbCount= vALIDATION_INFOContext.TextspeechValidationResponseDetail.Where(e => textSpeeches.Contains(e.RefAutoid)).Count();
                            List<long> UserValData = vALIDATION_INFOContext.TextspeechValidationResponseDetail.Select(X => X.RefAutoid).ToList();
                            List<long> textSpeeches = _textToSpeechContext.TextSpeech.Where(x => x.DatasetId == item.DatasetId).Select(e => e.AutoId).ToList();
                            valDestinationDbCount = UserValData.Intersect(textSpeeches).Count();


                        }
                        else if (DestinationDB == "TextText")
                        {
                            destinationDbCount = _textToTextContext.TextText.Where(e => e.DatasetId == item.DatasetId).Count();
                            //List<long> textTextes = _textToTextContext.TextText.Where(e => e.DatasetId == item.DatasetId).Select(x => x.AutoId).ToList();
                            //valDestinationDbCount += vALIDATION_INFOContext.TexttextValidationResponseDetail.Where(e => textTextes.Contains(e.RefAutoid)).Count();
                            List<long> UserValData = vALIDATION_INFOContext.TexttextValidationResponseDetail.Select(X => X.RefAutoid).ToList();
                            List<long> texttexts = _textToTextContext.TextText.Where(x => x.DatasetId == item.DatasetId).Select(e => e.AutoId).ToList();
                            valDestinationDbCount = UserValData.Intersect(texttexts).Count();
                        }
                    }

                    if (SourceDB == "Images")
                    {
                        sourceDbCount = _imageContext.Images.Where(e => e.DatasetId == item.DatasetId).Count();
                        if (DestinationDB == "ImageText")
                        {
                            destinationDbCount = _imageToTextContext.ImageText.Where(e => e.DatasetId == item.DatasetId).Count();
                            //List<long> imageTextes = _imageToTextContext.ImageText.Where(e => e.DatasetId == item.DatasetId).Select(x => x.AutoId).ToList();
                            //valDestinationDbCount = vALIDATION_INFOContext.ImagetextValidationResponseDetail.Where(e => imageTextes.Contains(e.RefAutoid)).Count();
                            List<long> UserValData = vALIDATION_INFOContext.TexttextValidationResponseDetail.Select(X => X.RefAutoid).ToList();
                            List<long> texttexts = _textToTextContext.TextText.Where(x => x.DatasetId == item.DatasetId).Select(e => e.AutoId).ToList();
                            valDestinationDbCount = UserValData.Intersect(texttexts).Count();
                        }

                    }


                    homePageCount.DataSetName = DatasetName;
                    homePageCount.SourceDataCount = sourceDbCount;
                    homePageCount.CollectedDataCount = destinationDbCount;
                    homePageCount.ValidatedDataCount = valDestinationDbCount;
                    GeneralHomePageDataList.Add(homePageCount);
                }
                return GeneralHomePageDataList;
            }
            catch (Exception)
            {
                return null;
                //return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]    //API TO GET HOMEPAGE DASHBOARD COUNTS user wise (ADMIN HOMEPAGE)
        public List<UserWiseDataCountModel> GetHomePageCountUserWise()
        {
            //list of userwise data model
            List<UserWiseDataCountModel> UserWiseDataList = new List<UserWiseDataCountModel>();

            List<HomePageUserWiseCount> HomePageUserwiseDataList = new List<HomePageUserWiseCount>();
            try
            {
                //list of all datasets
                List<Datasets> Datasets = new List<Datasets>();
                Datasets = _context.Datasets.ToList();

                //list of dataset and destination
                List<DatasetSubcategoryMapping> model = new List<DatasetSubcategoryMapping>();
                model = _context.DatasetSubcategoryMapping.ToList();

                foreach (var item in Datasets)
                {
                    int destinationDb_ = Convert.ToInt32(model.Where(e => e.DatasetId == item.DatasetId).Select(e=>e.DestinationSubcategoryId).FirstOrDefault());

                    string DestinationDBName = Convert.ToString(_context.SubCategories.Where(e => e.SubcategoryId == destinationDb_).Select(e => e.Name).FirstOrDefault()) ?? "";

                    if (DestinationDBName == "TextSpeech")
                    {
                        var firstCount = _textToSpeechContext.TextSpeech.Where(e => e.DatasetId == item.DatasetId).GroupBy(e => e.UserId).Select(e => new UserWiseDataCountModel
                        {
                            UserId = e.Key,
                            Datasetcount = e.Count(),
                            UserName = "",
                            DataSetName = "",
                            DatasetId = item.DatasetId,
                        }).ToList();

                        var userInfo = _context.UserInfo;

                        var secondCount = firstCount.Join(userInfo, c => c.UserId, u => u.UserId, (c, u) => new UserWiseDataCountModel
                        {
                            UserId = u.UserId,
                            UserName = u.Name,
                            Datasetcount = c.Datasetcount,
                            DatasetId = c.DatasetId,
                            DataSetName = "",

                        });
                        var thirdCount = Datasets.Join(secondCount, d => d.DatasetId, s => s.DatasetId, (d, s) => new UserWiseDataCountModel
                        {

                            UserId = s.UserId,
                            UserName = s.UserName,
                            Datasetcount = s.Datasetcount,
                            DatasetId = s.DatasetId,
                            DataSetName = d.Name,

                        });


                        foreach (var data in thirdCount)
                        {
                            UserWiseDataList.Add(data);
                        }
                    }
                }

            }


           

                //return GeneralHomePageDataList;
            //return null;
          
            catch (Exception )
            {
                return null;
                //return BadRequest(new { message = ex.Message });
            }
            return UserWiseDataList;
        }

        [AllowAnonymous]
        //single user dashboard count
        [HttpGet]
        public IEnumerable<DashboardModel> GetUserWorkReport(int UserId)
        {
            //store result in user list
            List<DashboardModel> userWiseDataCount = new List<DashboardModel>();
            //get all dataset and find at destination folder using userid filter

            //get dataset
            IEnumerable<Datasets> datasets = _context.Datasets.ToList();

            //get destination folders
            IEnumerable<DatasetSubcategoryMapping> destinationList = _context.DatasetSubcategoryMapping.ToList();

            foreach (var dataset in datasets)
            {
                int srcDbId = Convert.ToInt32(destinationList.Where(e => e.DatasetId == dataset.DatasetId).Select(e => e.SourceSubcategoryId).SingleOrDefault());
                int destinationDbId = Convert.ToInt32(destinationList.Where(e => e.DatasetId == dataset.DatasetId).Select(e => e.DestinationSubcategoryId).SingleOrDefault());
                //int ValdestinationCount = 0;
                string srcDbname = Convert.ToString(_context.SubCategories.Where(e => e.SubcategoryId == srcDbId).Select(e => e.Name).SingleOrDefault());
                string DestinationDBName = Convert.ToString(_context.SubCategories.Where(e => e.SubcategoryId == destinationDbId).Select(e => e.Name).SingleOrDefault());

                if (srcDbname == "Text")
                {
                    int sourceDatasetCount = _textContext.Text.Where(e => e.DatasetId == dataset.DatasetId).Count();
                    
                    if (DestinationDBName == "TextSpeech")
                    {
                        
                        List<long> UserValData = vALIDATION_INFOContext.TextspeechValidationResponseDetail.Where(x => x.UserId == UserId).Select(X => X.RefAutoid).ToList();
                        List<long> textSpeeches = _textToSpeechContext.TextSpeech.Where(x => x.DatasetId == dataset.DatasetId ).Select(e => e.AutoId).ToList();
                        List<long> actualUserValidatedForDataset = UserValData.Intersect(textSpeeches).ToList();

                        userWiseDataCount.Add(new DashboardModel
                        {
                            DatasetName = dataset.Name,
                            SrcDatasetCount = sourceDatasetCount,
                            DestDatasetCount = _textToSpeechContext.TextSpeech.Where(x => x.DatasetId == dataset.DatasetId && x.UserId == UserId).Select(e => e).Count(),
                            //ValDatasetCount = vALIDATION_INFOContext.TextspeechValidationResponseDetail.Where(x => textSpeeches.Contains(x.RefAutoid)).Count()
                            ValDatasetCount = actualUserValidatedForDataset.Count()
                        });
                        //var a = vALIDATION_INFOContext.TextspeechValidationResponseDetail.Where(x => textSpeeches.Contains(x.RefAutoid));
                        //var abc = vALIDATION_INFOContext.TextspeechValidationResponseDetail.Select(x=>x.RefAutoid).Intersect(textSpeeches);
                    }
                    else if (DestinationDBName == "TextText")
                    {
                        //List<long> textTextes = _textToTextContext.TextText.Where(x => x.DatasetId == dataset.DatasetId && x.UserId == UserId).Select(e => e.AutoId).ToList();
                        List<long> UserValData = vALIDATION_INFOContext.TexttextValidationResponseDetail.Where(x => x.UserId == UserId).Select(X => X.RefAutoid).ToList();
                        List<long> texttexts = _textToTextContext.TextText.Where(x => x.DatasetId == dataset.DatasetId).Select(e => e.AutoId).ToList();
                        List<long> actualUserValidatedForDataset = UserValData.Intersect(texttexts).ToList();

                        userWiseDataCount.Add(new DashboardModel
                        {
                            DatasetName = dataset.Name,
                            SrcDatasetCount = sourceDatasetCount,
                            DestDatasetCount = _textToTextContext.TextText.Where(x => x.DatasetId == dataset.DatasetId && x.UserId == UserId).Select(e => e).Count(),
                            //ValDatasetCount = vALIDATION_INFOContext.TexttextValidationResponseDetail.Where(x => textTextes.Contains(x.RefAutoid)).Count()
                            ValDatasetCount = actualUserValidatedForDataset.Count()
                        });
                    }
                }

                if (srcDbname == "Images")
                {
                    int sourceDatasetCount = _imageContext.Images.Where(e => e.DatasetId == dataset.DatasetId).Count();
                    if (DestinationDBName == "ImageText")
                    {
                        //List<long> ImageText = _imageToTextContext.ImageText.Where(x => x.DatasetId == dataset.DatasetId && x.UserId == UserId).Select(e => e.AutoId).ToList();
                        List<long> UserValData = vALIDATION_INFOContext.ImagetextValidationResponseDetail.Where(x => x.UserId == UserId).Select(X => X.RefAutoid).ToList();
                        List<long> imagetexts = _imageToTextContext.ImageText.Where(x => x.DatasetId == dataset.DatasetId).Select(e => e.AutoId).ToList();
                        List<long> actualUserValidatedForDataset = UserValData.Intersect(imagetexts).ToList();

                        userWiseDataCount.Add(new DashboardModel
                        {
                            DatasetName = dataset.Name,
                            SrcDatasetCount = sourceDatasetCount,
                            DestDatasetCount = _imageToTextContext.ImageText.Where(x => x.DatasetId == dataset.DatasetId && x.UserId == UserId).Select(e => e).Count(),
                            //ValDatasetCount = vALIDATION_INFOContext.ImagetextValidationResponseDetail.Where(x=> ImageText.Contains(x.RefAutoid)).Count()
                            ValDatasetCount = actualUserValidatedForDataset.Count()
                        });
                    }

                }
            }
            return userWiseDataCount;
        }
    }
}
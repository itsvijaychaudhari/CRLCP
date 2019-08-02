using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

using CRLCP.Helper;
using CRLCP.Models;
using CRLCP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace CRLCP.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly IUserRepository _userRepo;
        private readonly JsonResponse _jsonResponse;
        private readonly AppSettings _appSettings;
        private readonly CLRCP_MASTERContext _masterContext;

        public AccountController(CLRCP_MASTERContext context, 
                                    IUserRepository userService, 
                                    IOptions<AppSettings> appSettings, 
                                    JsonResponse jsonResponse)
        {
            _masterContext = context;
            _userRepo = userService;
            _jsonResponse = jsonResponse;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Register([FromBody]LoginDetails user)
        {
            try
            {
                _userRepo.Create(user);

                _masterContext.UserInfo.Add(
                    new UserInfo
                    {
                        UserId = user.UserId,
                        Name = string.Empty,
                        Age = 0,
                        Gender = string.Empty,
                        LangId1 = 1,//TODO
                        LangId2 = -1,
                        LangId3 = -1,
                        QualificationId = -1

                    });
                await _masterContext.SaveChangesAsync();
                return Created("", user.UserId);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var user = _userRepo.Authenticate(login);
            if (user == null)
                return Unauthorized(new
                {
                    IsAuthenticate = false,
                    Token = string.Empty,
                    UserId = 0,
                    UserType = 0
                });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.EmailId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new
            {
                IsAuthenticate = true,
                Token = tokenString,
                UserId = user.UserId,
                UserType = user.UserType
            });

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Update([FromBody] UserInfo userInfo)
        {
            try
            {
                UserInfo user = _masterContext.UserInfo.FirstOrDefault(x => x.UserId == userInfo.UserId);
                if (user == null)
                {
                    return NotFound(_jsonResponse.Response = "User Not Found");
                }
                else
                {
                    user.Name = userInfo.Name;
                    user.Age = userInfo.Age;
                    user.Gender = userInfo.Gender;
                    user.LangId1 = userInfo.LangId1;
                    user.LangId2 = userInfo.LangId2;
                    user.LangId3 = userInfo.LangId3;
                    user.QualificationId = userInfo.QualificationId;
                    try
                    {
                        _masterContext.UserInfo.Update(user);
                        _masterContext.SaveChanges();
                        _jsonResponse.IsSuccessful = true;
                        _jsonResponse.Response = "Profile Update successfully.";
                        return Ok(_jsonResponse);
                    }
                    catch (Exception)
                    {
                        _jsonResponse.IsSuccessful = false;
                        _jsonResponse.Response = "Internal Exception.";
                        return BadRequest(_jsonResponse);
                    }
                }
            }
            catch (Exception)
            {
                return BadRequest(_jsonResponse.Response = "Unable to Update Profile");
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonResponse), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetProfile(int UserId)
        {
            try
            {
                UserInfo userInfo = _masterContext.UserInfo.FirstOrDefault(x => x.UserId == UserId);
                return Ok(userInfo);
            }
            catch (Exception)
            {

                _jsonResponse.IsSuccessful = false;
                _jsonResponse.Response = "User not found";
                return Ok(_jsonResponse);


            }
            
        }


        [HttpGet]
        public IEnumerable<LanguageIdMapping> GetLanguages()
        {
            return _masterContext.LanguageIdMapping.ToList();
        }


        [HttpGet]
        public IEnumerable<QualificationIdMapping> GetQualifications()
        {
            return _masterContext.QualificationIdMapping.ToList();
        }

       
        [HttpGet]
        [AllowAnonymous]
        [ActionName("CheckUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult CheckUserName(string username)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                var IsFound = _masterContext.LoginDetails.Where(x => x.EmailId == username).SingleOrDefault();
                if (IsFound == null)
                {
                    _jsonResponse.IsSuccessful = true;
                    _jsonResponse.Response = "Username is available";
                    return Ok(_jsonResponse);
                }
                _jsonResponse.IsSuccessful = false;
                _jsonResponse.Response = "Username is not available";
                return Ok(_jsonResponse);
            }
            return BadRequest();
        }

    }
}
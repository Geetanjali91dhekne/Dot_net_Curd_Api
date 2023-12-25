using AuthenticationService.Models;
using AuthenticationService.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Runtime;
using ConfigurationSettings = System.Configuration.ConfigurationManager;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace AuthenticationService.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;
        private readonly RespondModel respondModel;
        private readonly FleetManagerContext _context;



        public LoginRepository(IHttpContextAccessor contextAccessor, IConfiguration config, FleetManagerContext context)
        {
            _contextAccessor = contextAccessor;
            _config = config;
            this.respondModel = new RespondModel();
            _context = context;

        }

        public RespondModel GetLoginUser(string? userId, string? password)
        {
            try
            {
                RespondModel respondModel = new RespondModel();
                var login = new Login();
                var _settings = _config.GetSection("Settings").Get<Settings>();

                if (userId != "")
                {
                    login.domainId = userId;
                    login.password = password;
                    login.url = _settings.Url!;
                    login.appType = _settings.AppType!;
                }
                else
                {
                    //var userName = _contextAccessor.HttpContext.User.Identity.Name;
                    var windowsIdentity = WindowsIdentity.GetCurrent();
                    var userName = windowsIdentity?.Name;

                    if (userName != null)
                    {
                        login.domainId = userName.Split("\\")[1];
                        login.password = "";
                        login.appType = _settings.AppType!;
                        login.url = _settings.Url!;
                    }
                }

                var user = GetUserDetails(login);

                if (user != null)
                {
                    var permissions = GetUserPermissions(user);

                    if (permissions != null)
                    {
                        var token = CreateToken(login, user);

                        if (token != null)
                        {

                            respondModel.code = Constants.httpCodeSuccess;
                            respondModel.data = new
                            {
                                username = login.domainId,
                                authenticationToken = token,
                                permissions
                            };
                            respondModel.status = true;
                            respondModel.message = "Login Successful!";
                        }
                        else
                        {
                            respondModel.message = "Cannot create token!";
                        }
                    }
                    else
                    {
                        respondModel.message = "User has no permissions!";
                    }
                }
                else
                {
                    respondModel.message = "Invalid Credentials!";
                }

                return respondModel;
            }
            catch (Exception ex)
            {
                respondModel.message = ex.Message;
                return respondModel;
            }
        }

        private List<object> GetUserPermissions(List<RolePermissionList> rolePermissionLists)
        {
            var permissions = new List<object>();

            foreach (var rolePermission in rolePermissionLists)
            {
                var permissionObj = new
                {
                    roleId = rolePermission.RoleId,
                    role = rolePermission.Role,
                    permission = rolePermission.Permission.Select(permission => new
                    {
                        id = permission.Id,
                        entityCategory = permission.EntityCategory,
                        entityName = permission.EntityName,
                        actions = permission.Actions
                    }).ToList()
                };

                permissions.Add(permissionObj);
            }

            return permissions;
        }




        #region for getting log in user details from url
        //private List<RolePermissionList> GetUserDetails(Login _login)
        private List<RolePermissionList> GetUserDetails(Login _login)
        {

            try
            {
                // Checking from api user is active or not
                string domainId = Base64Encode(_login.domainId);

                string password = _login.password;
                string appType = _login.appType;
                string URI = _login.url!;
                string myParameters = $"domainId={domainId}&password={password}&appType={appType}";

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol =
                //      SecurityProtocolType.Tls
                //    | SecurityProtocolType.Tls11
                //    | SecurityProtocolType.Tls12;
                // | SecurityProtocolType.Ssl3;

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Checking data from api
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string htmlResult = wc.UploadString(URI, myParameters);

                    var userResult = ConvertUserDetailsToJson(htmlResult);


                    if (userResult != null && userResult.IsActive)
                    {
                        List<RolePermissionList> rolePermissionLists = new List<RolePermissionList>();
                        List<RolePermissionMapping> rolePermissionMappings = new List<RolePermissionMapping>();
                        RolePermissionList rolePermissionList = new RolePermissionList();
                        // User Is Active & Found in AD.
                        var roleList = _context.TUserRoleMappings.
                        Where(n => n.Username == _login.domainId).ToList();

                        foreach (var role in roleList)
                        {

                            int roleIds = role.FkRoleId;
                            var permissionList = _context.TRolePermissionMappings
                                .Where(c => c.FkRoleId == roleIds)
                                .ToList();

                            List<Permissions> permissionObjList = new List<Permissions>();
                            foreach (var permission in permissionList)
                            {
                                var permissionId = permission.FkPermissionId;
                                var permissionDetails = _context.TPermissions
                                .Where(p => p.Id == permissionId).FirstOrDefault();

                                var _permissionDetails = new Permissions
                                {
                                    Id = permissionDetails.Id,
                                    EntityCategory = permissionDetails.EntityCategory,
                                    EntityName = permissionDetails.EntityName,
                                    Actions = permissionDetails.Actions,
                                };
                                permissionObjList.Add(_permissionDetails);
                            }
                            var roleNameList = _context.TRoleMasters.Where(p => p.Id == roleIds).ToList();
                            foreach (var roles in roleNameList)
                            {
                                var rolePermission = new RolePermissionList
                                {
                                    RoleId = roles.Id,
                                    Role = roles.RoleName,
                                    Permission = permissionObjList
                                };
                                rolePermissionLists.Add(rolePermission);
                            }


                        }

                        return rolePermissionLists;



                    }
                    else
                    {

                        return null;
                    }

                }


            }
            catch (Exception)
            {

                return null;

            }

        }
        #endregion for getting log in user details from url

        // For Converting to Base64 string
        #region encoding domainid
        private static string Base64Encode(string text)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textBytes);
        }
        #endregion encoding domainid

        // Conerting JSON result
        #region converting string to json

        private static Login ConvertUserDetailsToJson(string text)
        {
            return JsonConvert.DeserializeObject<Login>(text)!;
        }
        #endregion converting string to json

        #region create token


        private string CreateToken(Login login, List<RolePermissionList> rolePermissionLists)
        {

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("G3VF4C6KFV43JH6GKCDFGJH45V36JHGV3H4C6F3GJC63HG45GH6V345GHHJ4623FJL3HCVMO1P23PZ07W8");
                var issuer = "arbems.com";
                var audience = "Public";
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login.domainId)

            };
                foreach (var rolePermission in rolePermissionLists)
                {

                    claims.Add(new Claim("Role", rolePermission.Role));

                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Subject = new ClaimsIdentity(claims.ToArray()),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return (tokenHandler.WriteToken(token));
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                return null;
            }
        }
        #endregion create token

    }
}
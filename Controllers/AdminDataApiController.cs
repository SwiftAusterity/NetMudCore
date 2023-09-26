using NetMudCore.Authentication;
using NetMudCore.Cartography;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.Players;
using NetMudCore.Data.Room;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.Room;
using NetMudCore.Physics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NetMudCore.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Builder")]
    public class AdminDataApiController : Controller
    {
        private UserManager<ApplicationUser>? _userManager;
        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager ?? HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        public string GetEntityModelView(long modelId)
        {
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
            {
                return string.Empty;
            }

            return Render.FlattenModelForWeb(model);
        }

        [HttpGet]
        public string[] GetDimensionalData(long id)
        {
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(id);

            if (model == null)
            {
                return System.Array.Empty<string>();
            }

            return model.ModelPlanes.Select(plane => plane.TagName).Distinct().ToArray();
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderRoomForEditWithRadius/{id}/{radius}", Name = "RenderRoomForEditWithRadius")]
        public string RenderRoomForEditWithRadius(long id, int radius)
        {
            IRoomTemplate centerRoom = TemplateCache.Get<IRoomTemplate>(id);

            if (centerRoom == null || radius < 0)
            {
                return "Invalid inputs.";
            }

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLocaleMapForEdit/{id}/{zIndex}", Name = "RenderLocaleMapForEdit")]
        public string[] RenderLocaleMapForEdit(long id, int zIndex)
        {
            ILocaleTemplate locale = TemplateCache.Get<ILocaleTemplate>(id);

            if (locale == null)
            {
                return new string[] { "Invalid inputs." };
            }

            System.Tuple<string, string, string> maps = Rendering.RenderRadiusMap(locale, 10, zIndex);

            return new string[] { maps.Item1, maps.Item2, maps.Item3 };
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLiveRoomForEditWithRadius/{radius}")]
        public string RenderLiveRoomForEditWithRadius(string birthMark, int radius)
        {
            IRoom centerRoom = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), birthMark));

            if (centerRoom == null || radius < 0)
            {
                return "Invalid inputs.";
            }

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLiveLocaleMapForEdit/{zIndex}")]
        public string[] RenderLiveLocaleMapForEdit(string birthMark, int zIndex)
        {
            ILocale locale = LiveCache.Get<ILocale>(new LiveCacheKey(typeof(ILocale), birthMark));

            if (locale == null)
            {
                return new string[] { "Invalid inputs." };
            }

            System.Tuple<string, string, string> maps = Rendering.RenderRadiusMap(locale, 10, zIndex);

            return new string[] { maps.Item1, maps.Item2, maps.Item3 };
        }


        [HttpGet]
        [Route("api/AdminDataApi/GetDictata/{languageCode}/{wordType}/{term}", Name = "AdminAPI_GetDictata")]
        public JsonResult GetDictata(string languageCode, LexicalType wordType, string term)
        {
            IEnumerable<ILexeme> words = ConfigDataCache.GetAll<ILexeme>().Where(dict => dict.GetForm(wordType) != null && dict.Name.Contains(term) && dict.Language.GoogleLanguageCode.Equals(languageCode));

            return Json(words.Select(word => word.Name).ToArray());
        }

        [HttpGet]
        [Route("api/AdminDataApi/GetRoomsOfLocale/{localeId}", Name = "AdminAPI_GetRoomsOfLocale")]
        public JsonResult GetRoomsOfLocale(int localeId)
        {
            IEnumerable<IRoomTemplate> rooms = ConfigDataCache.GetAll<IRoomTemplate>().Where(room => room.ParentLocation.Id == localeId);

            return Json(rooms.ToDictionary(room => room.Id, room => room.Name));
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public async Task<string> ChangeAccountRoleAsync(string accountName, short role)
        {
            RoleManager<IdentityRole> roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            List<IdentityRole> validRoles = roleManager.Roles.ToList();

            ApplicationUser? user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (user == null)
            {
                return "failure";
            }

            Account account = user.GameAccount;
            StaffRank userRole = ApplicationUser.GetStaffRank(User);

            if (userRole != StaffRank.Admin)
            {
                if (string.IsNullOrWhiteSpace(accountName) || account.GlobalIdentityHandle.Equals(accountName) || role >= (short)ApplicationUser.GetStaffRank(User))
                {
                    return "failure";
                }
            }

            ApplicationUser? userToModify = await UserManager.FindByNameAsync(accountName);

            if (userToModify == null)
            {
                return "failure";
            }

            foreach (string? currentRole in validRoles.Select(vr => vr.Name))
            {
                if(currentRole != null)
                {
                    bool hasRole = await UserManager.IsInRoleAsync(userToModify, currentRole);
                    if(hasRole)
                    {
                        _ = UserManager.RemoveFromRoleAsync(userToModify, currentRole);
                    }
                }
            }

            _ = UserManager.AddToRoleAsync(userToModify, ((StaffRank)role).ToString());

            return "success";
        }

        [HttpPost]
        [Route("api/AdminDataApi/Quickbuild/{originId}/{destinationId}/{direction}/{incline}", Name = "AdminAPI_Quickbuild")]
        public async Task<string> QuickbuildAsync(long originId, long destinationId, int direction, int incline)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if(authedUser == null)
            {
                return "Error; Creation failed.";
            }

            IRoomTemplate origin = TemplateCache.Get<IRoomTemplate>(originId);
            IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(destinationId);

            string message = string.Empty;

            if (destination == null)
            {
                destination = new RoomTemplate
                {
                    Name = "Room",
                    Medium = origin.Medium,
                    ParentLocation = origin.ParentLocation,
                    Model = new DimensionalModel()
                    {
                        ModelTemplate = origin.Model.ModelTemplate,
                        Composition = origin.Model.Composition,
                        Height = origin.Model.Height,
                        Length = origin.Model.Length,
                        Width = origin.Model.Width,
                        SurfaceCavitation = origin.Model.SurfaceCavitation,
                        Vacuity = origin.Model.Vacuity
                    }
                };

                destination = (IRoomTemplate)destination.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
            }


            if (destination != null)
            {
                IPathwayTemplate newObj = new PathwayTemplate
                {
                    Name = "Pathway",
                    Destination = destination,
                    Origin = origin,
                    InclineGrade = incline,
                    DegreesFromNorth = direction,
                    Model = new DimensionalModel()
                    {
                        ModelTemplate = origin.Model.ModelTemplate,
                        Composition = origin.Model.Composition,
                        Height = origin.Model.Height,
                        Length = origin.Model.Length,
                        Width = origin.Model.Width,
                        SurfaceCavitation = origin.Model.SurfaceCavitation,
                        Vacuity = origin.Model.Vacuity
                    }
                };

                if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    PathwayTemplate reversePath = new()
                    {
                        Name = newObj.Name,
                        DegreesFromNorth = Utilities.ReverseDirection(newObj.DegreesFromNorth),
                        Origin = newObj.Destination,
                        Destination = newObj.Origin,
                        Model = newObj.Model,
                        InclineGrade = newObj.InclineGrade * -1
                    };

                    if (reversePath.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                    {
                        message = "Reverse Path creation FAILED. Origin path creation SUCCESS.";
                    }

                    LoggingUtility.LogAdminCommandUsage("*WEB* - Quickbuild[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return message;
        }
    }
}

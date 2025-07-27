﻿using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageZoneTemplateViewModel : PagedDataModel<IZoneTemplate>
    {
        public ManageZoneTemplateViewModel(IEnumerable<IZoneTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IZoneTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IZoneTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.World.Name;
            }
        }


        internal override Func<IZoneTemplate, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditZoneTemplateViewModel : PagedDataModel<ILocaleTemplate>
    {
        internal override Func<ILocaleTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<ILocaleTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<ILocaleTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        public AddEditZoneTemplateViewModel(IEnumerable<ILocaleTemplate> items)
        : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        public AddEditZoneTemplateViewModel() : base(Enumerable.Empty<ILocaleTemplate>())
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        public IEnumerable<IGaiaTemplate> ValidWorlds { get; set; }
        public IEnumerable<IFauna> FaunaNaturalResources { get; set; }
        public IEnumerable<IFlora> FloraNaturalResources { get; set; }
        public IEnumerable<IMineral> MineralNaturalResources { get; set; }
        public IZoneTemplate DataObject { get; set; }
    }

    public class AddEditZonePathwayTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditZonePathwayTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomTemplate>();
        }

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }

        [RoomTemplateDataBinder]
        [UIHint("RoomTemplateList")]
        [Display(Name = "Destination", Description = "Where this pathway leads to.")]
        public IRoomTemplate DestinationRoom { get; set; }

        public IPathwayTemplate DataObject { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
    }
}
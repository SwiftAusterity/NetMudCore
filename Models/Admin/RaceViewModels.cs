﻿using NetMudCore.Data.Architectural.ActorBase;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Zone;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageRaceDataViewModel : PagedDataModel<IRace>
    {
        public ManageRaceDataViewModel(IEnumerable<IRace> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRace, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IRace, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IRace, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditRaceViewModel : AddEditTemplateModel<IRace>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("RaceList")]
        [RaceDataBinder]
        public override IRace Template { get; set; }

        public AddEditRaceViewModel() : base(-1)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidZones = TemplateCache.GetAll<IZoneTemplate>();
            DataObject = new Race();
        }

        public AddEditRaceViewModel(long templateId) : base(templateId)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidZones = TemplateCache.GetAll<IZoneTemplate>();
            DataObject = new Race();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Arms = DataTemplate.Arms;
                DataObject.BodyParts = DataTemplate.BodyParts;
                DataObject.Breathes = DataTemplate.Breathes;
                DataObject.DeathNoticeBody = DataTemplate.DeathNoticeBody;
                DataObject.DeathQualityChanges = DataTemplate.DeathQualityChanges;
                DataObject.DietaryNeeds = DataTemplate.DietaryNeeds;
                DataObject.EmergencyLocation = DataTemplate.EmergencyLocation;
                DataObject.Head = DataTemplate.Head;
                DataObject.Legs = DataTemplate.Legs;
                DataObject.SanguinaryMaterial = DataTemplate.SanguinaryMaterial;
                DataObject.StartingLocation = DataTemplate.StartingLocation;
                DataObject.TeethType = DataTemplate.TeethType;
                DataObject.TemperatureTolerance = DataTemplate.TemperatureTolerance;
                DataObject.Torso = DataTemplate.Torso;
                DataObject.VisionRange = DataTemplate.VisionRange;
            }
        }


        public AddEditRaceViewModel(string archivePath, IRace item) : base(archivePath, item)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidZones = TemplateCache.GetAll<IZoneTemplate>();
            DataObject = item;
        }

        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IEnumerable<IInanimateTemplate> ValidItems { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IRace DataObject { get; set; }
    }
}
﻿using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.Data.NaturalResource;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.NaturalResource;
using System.ComponentModel.DataAnnotations;


namespace NetMudCore.Models.Admin
{
    public class ManageFaunaViewModel : PagedDataModel<IFauna>
    {
        public ManageFaunaViewModel(IEnumerable<IFauna> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFauna, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFauna, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFauna, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditFaunaViewModel : AddEditTemplateModel<IFauna>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("FaunaList")]
        [FaunaDataBinder]
        public override IFauna Template { get; set; }

        public AddEditFaunaViewModel() : base(-1)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            DataObject = new Fauna();
        }

        public AddEditFaunaViewModel(long templateId) : base(templateId)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            DataObject = new Fauna();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AmountMultiplier = DataTemplate.AmountMultiplier;
                DataObject.CanSpawnInSystemAreas = DataTemplate.CanSpawnInSystemAreas;
                DataObject.ElevationRange = DataTemplate.ElevationRange;
                DataObject.FemaleRatio = DataTemplate.FemaleRatio;
                DataObject.HumidityRange = DataTemplate.HumidityRange;
                DataObject.OccursIn = DataTemplate.OccursIn;
                DataObject.PopulationHardCap = DataTemplate.PopulationHardCap;
                DataObject.PuissanceVariance = DataTemplate.PuissanceVariance;
                DataObject.Rarity = DataTemplate.Rarity;
                DataObject.Race = DataTemplate.Race;
                DataObject.TemperatureRange = DataTemplate.TemperatureRange;
            }
        }

        public AddEditFaunaViewModel(string archivePath, IFauna item) : base(archivePath, item)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            DataObject = item;
        }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFauna DataObject { get; set; }
    }
}
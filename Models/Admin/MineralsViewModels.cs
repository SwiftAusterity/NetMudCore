﻿using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.Data.NaturalResource;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMudCore.Models.Admin
{
    public class ManageMineralsViewModel : PagedDataModel<IMineral>
    {
        public ManageMineralsViewModel(IEnumerable<IMineral> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IMineral, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IMineral, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IMineral, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditMineralsViewModel : AddEditTemplateModel<IMineral>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("MineralList")]
        [MineralDataBinder]
        public override IMineral Template { get; set; }

        public AddEditMineralsViewModel() : base(-1)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidMinerals = TemplateCache.GetAll<IMineral>();
            DataObject = new Mineral();
        }

        public AddEditMineralsViewModel(long templateId) : base(templateId)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidMinerals = TemplateCache.GetAll<IMineral>();
            DataObject = new Mineral();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AmountMultiplier = DataTemplate.AmountMultiplier;
                DataObject.CanSpawnInSystemAreas = DataTemplate.CanSpawnInSystemAreas;
                DataObject.ElevationRange = DataTemplate.ElevationRange;
                DataObject.HumidityRange = DataTemplate.HumidityRange;
                DataObject.OccursIn = DataTemplate.OccursIn;
                DataObject.PuissanceVariance = DataTemplate.PuissanceVariance;
                DataObject.Rarity = DataTemplate.Rarity;
                DataObject.TemperatureRange = DataTemplate.TemperatureRange;
                DataObject.Dirt = DataTemplate.Dirt;
                DataObject.Fertility = DataTemplate.Fertility;
                DataObject.Ores = DataTemplate.Ores;
                DataObject.Rock = DataTemplate.Rock;
                DataObject.Solubility = DataObject.Solubility;
            }
        }

        public AddEditMineralsViewModel(string archivePath, IMineral item) : base(archivePath, item)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidMinerals = TemplateCache.GetAll<IMineral>().Where(m => m.Id != item.Id);
            DataObject = item;
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IMineral> ValidMinerals { get; set; }
        public IMineral DataObject { get; set; }
    }
}
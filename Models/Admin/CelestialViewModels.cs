﻿using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.Data.Gaia;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageCelestialsViewModel : PagedDataModel<ICelestial>
    {
        public ManageCelestialsViewModel(IEnumerable<ICelestial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ICelestial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<ICelestial, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<ICelestial, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditCelestialViewModel : AddEditTemplateModel<ICelestial>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("CelestialList")]
        [CelestialDataBinder]
        public override ICelestial Template { get; set; }

        public AddEditCelestialViewModel() : base(-1)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>(true);
    }

        public AddEditCelestialViewModel(long templateId) : base(templateId)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>(true);
            DataObject = new Celestial();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Apogee = DataTemplate.Apogee;
                DataObject.Perigree = DataTemplate.Apogee;
                DataObject.Velocity = DataTemplate.Velocity;
                DataObject.Luminosity = DataTemplate.Luminosity;
                DataObject.Model = DataTemplate.Model;
                DataObject.OrientationType = DataTemplate.OrientationType;
            }
        }

        public AddEditCelestialViewModel(string archivePath, ICelestial item) : base(archivePath, item)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>(true);
            DataObject = item;
        }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public ICelestial DataObject { get; set; }
    }
}
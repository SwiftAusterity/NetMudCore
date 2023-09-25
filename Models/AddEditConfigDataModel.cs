using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetMudCore.Models
{
    public abstract class AddEditConfigDataModel<T> : IBaseViewModel where T : IConfigData
    {
        public ApplicationUser? AuthedUser { get; set; }

        public T DataTemplate { get; set; }
        public IEnumerable<T> ValidTemplateBases { get; set; }

        public abstract T Template { get; set; }

        public string ArchivePath { get; set; }
        public string[] Archives { get; set; }

        public AddEditConfigDataModel(string uniqueKey, ConfigDataType type)
        {
            DataTemplate = ConfigDataCache.Get<T>(new ConfigDataCacheKey(typeof(T), uniqueKey, type));
            ValidTemplateBases = ConfigDataCache.GetAll<T>();
            Archives = Array.Empty<string>();
        }

        public AddEditConfigDataModel(string archivePath, ConfigDataType type, T item)
        {
            ConfigData fileAccessor = new();

            DataTemplate = default;
            ValidTemplateBases = ConfigDataCache.GetAll<T>();
            Archives = GetArchiveNames(fileAccessor, type, item.UniqueKey);

            ArchivePath = archivePath;

            if (!string.IsNullOrWhiteSpace(ArchivePath))
            {
                GetArchivedTemplate(fileAccessor, type, item);
            }
        }

        internal void GetArchivedTemplate(ConfigData fileAccessor, ConfigDataType type, T item)
        {
            Type templateType = typeof(T);
            string typeName = templateType.Name;

            if (templateType.IsInterface)
            {
                typeName = typeName[1..];
                templateType = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(templateType));
            }

            DirectoryInfo archiveDir = new(fileAccessor.BaseDirectory + type.ToString() + "/" + fileAccessor.ArchiveDirectoryName + ArchivePath + "/");

            FileInfo[] potentialFiles = archiveDir.GetFiles(item.UniqueKey + "." + typeName);

            if (potentialFiles.Any())
            {
                DataTemplate = (T)fileAccessor.ReadEntity(potentialFiles.First(), templateType);
            }
        }

        internal string[] GetArchiveNames(ConfigData fileAccessor, ConfigDataType type, string itemName)
        {
            string typeName = typeof(T).Name;

            if (typeof(T).IsInterface)
            {
                typeName = typeName[1..];
            }

            string archiveDirName = fileAccessor.BaseDirectory + type.ToString() + "/" + fileAccessor.ArchiveDirectoryName;

            if (!fileAccessor.VerifyDirectory(archiveDirName))
            {
                return Array.Empty<string>();
            }

            DirectoryInfo filesDirectory = new(archiveDirName);

            return filesDirectory.EnumerateDirectories().Where(dir => dir.GetFiles(itemName + "." + typeName, SearchOption.TopDirectoryOnly).Any())
                                                        .Select(dir => dir.Name).ToArray();
        }
    }

}
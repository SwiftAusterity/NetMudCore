using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Architectural;

namespace NetMudCore.Models
{
    public abstract class AddEditTemplateModel<T> : IBaseViewModel where T : IKeyedData
    {
        public ApplicationUser? AuthedUser { get; set; }

        public T DataTemplate { get; set; }
        public IEnumerable<T> ValidTemplateBases { get; set; }

        public abstract T Template { get; set; }

        public string ArchivePath { get; set; }
        public string[] Archives { get; set; }

        public AddEditTemplateModel(long templateId)
        {
            DataTemplate = TemplateCache.Get<T>(templateId);
            ValidTemplateBases = TemplateCache.GetAll<T>(true);
            Archives = Array.Empty<string>();
        }

        public AddEditTemplateModel(string archivePath, T item)
        {
            TemplateData fileAccessor = new();

            DataTemplate = default;
            ValidTemplateBases = TemplateCache.GetAll<T>(true);
            Archives = GetArchiveNames(fileAccessor);

            ArchivePath = archivePath;

            if (!string.IsNullOrWhiteSpace(ArchivePath))
            {
                GetArchivedTemplate(fileAccessor, item);
            }
        }

        internal void GetArchivedTemplate(TemplateData fileAccessor, T item)
        {
            string typeName = typeof(T).Name;
            Type templateType = typeof(T);

            if (typeof(T).IsInterface)
            {
                typeName = typeName[1..];
                templateType = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));
            }

            DirectoryInfo archiveDir = new(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName + ArchivePath + "/" + typeName + "/");

            FileInfo[] potentialFiles = archiveDir.GetFiles(item.Id + "." + typeName);

            if(potentialFiles.Any())
            {
                DataTemplate = (T)fileAccessor.ReadEntity(potentialFiles.First(), templateType);
            }
        }

        internal string[] GetArchiveNames(TemplateData fileAccessor)
        {
            DirectoryInfo filesDirectory = new(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName);

            string typeName = typeof(T).Name;

            if(typeof(T).IsInterface)
            {
                typeName = typeName[1..];
            }

            return filesDirectory.EnumerateDirectories().Where(dir => dir.GetDirectories(typeName, SearchOption.TopDirectoryOnly).Any())
                                                        .Select(dir => dir.Name).ToArray();
        }
    }
}
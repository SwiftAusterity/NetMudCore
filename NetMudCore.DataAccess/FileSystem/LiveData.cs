﻿using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.Utility;
using System;
using System.IO;

namespace NetMudCore.DataAccess.FileSystem
{
    public class LiveData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return null;// HostingEnvironment.MapPath(base.BaseDirectory + "LiveData/");
            }
        }

        public static IEntity ReadEntity(FileInfo file, Type entityType)
        {
            byte[] fileData = ReadFile(file);
            IEntity blankEntity = Activator.CreateInstance(entityType) as IEntity;

            return blankEntity.FromBytes(fileData) as IEntity;
        }

        public void WriteEntity(IEntity entity)
        {
            string baseTypeName = entity.GetType().Name;
            string dirName = BaseDirectory + CurrentDirectoryName + baseTypeName;

            if (!VerifyDirectory(dirName))
            {
                throw new Exception("Unable to locate or create base live data directory.");
            }

            WriteSpecificEntity(new DirectoryInfo(dirName), entity);
        }

        /// <summary>
        /// Writes one entity to Current backup (not players)
        /// </summary>
        /// <param name="dir">Root directory to write to</param>
        /// <param name="entity">The entity to write out</param>
        public static void WriteSpecificEntity(DirectoryInfo dir, IEntity entity)
        {
            string entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dir.FullName + "/" + entityFileName;

            try
            {
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }
        }

        /// <summary>
        /// Removes a live entity from the system and files
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool RemoveEntity(IEntity entity)
        {
            string fileName = GetEntityFilename(entity);
            return ArchiveFile(fileName, fileName);
        }

        /// <summary>
        /// Archives EVERYTHING
        /// </summary>
        public void ArchiveFull(string backupName = "")
        {
            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(BaseDirectory + CurrentDirectoryName, false) && VerifyDirectory(BaseDirectory + ArchiveDirectoryName))
            {
                CullDirectoryCount(BaseDirectory + ArchiveDirectoryName);

                DirectoryInfo currentRoot = new(BaseDirectory + CurrentDirectoryName);

                string backupDir = DatedBackupDirectory;
                if (!string.IsNullOrWhiteSpace(backupName))
                {
                    backupDir = string.Format("{0}{1}{2}/", BaseDirectory, ArchiveDirectoryName, backupName);
                }

                currentRoot.CopyTo(backupDir);
            }

            //something very wrong is happening, it'll get logged
            if (!VerifyDirectory(CurrentDirectoryName))
            {
                throw new Exception("Can not locate or verify current live data directory.");
            }
        }

        /// <summary>
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        private static string GetEntityFilename(IEntity entity)
        {
            return string.Format("{0}.{1}", entity.BirthMark, entity.GetType().Name);
        }
    }
}

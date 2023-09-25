﻿using NetMudCore.DataStructure.Architectural;
using NetMudCore.Utility;
using System;
using System.IO;

namespace NetMudCore.DataAccess.FileSystem
{
    public class ConfigData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                //Base dir is weird for config as it might be anywhere (but not outside the base dir)
                return null;//HostingEnvironment.MapPath(base.BaseDirectory);
            }
        }

        /// <summary>
        /// The default directory name for when files are rolled over or archived
        /// </summary>
        public override string DatedBackupDirectory
        {
            get
            {
                return string.Format("{0}{1}{2}{3}_{4}{5}/",
                                        ArchiveDirectoryName
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute);
            }
        }

        public static IConfigData ReadEntity(FileInfo file, Type entityType)
        {
            byte[] fileData = ReadFile(file);
            IConfigData blankEntity = Activator.CreateInstance(entityType) as IConfigData;

            return blankEntity.FromBytes(fileData) as IConfigData;
        }

        /// <summary>
        /// Write one backing data entity out
        /// </summary>
        /// <param name="entity">the thing to write out to current</param>
        public void WriteEntity(IConfigData entity)
        {
            try
            {
                string dirName = GetCurrentDirectoryForEntity(entity);

                if (!VerifyDirectory(dirName))
                {
                    throw new Exception("Unable to locate or create base backing data directory.");
                }

                string entityFileName = GetEntityFilename(entity);

                if (string.IsNullOrWhiteSpace(entityFileName))
                {
                    return;
                }

                string fullFileName = dirName + entityFileName;

                ArchiveEntity(entity);
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Archive a backing data entity
        /// </summary>
        /// <param name="entity">the thing to archive</param>
        public void ArchiveEntity(IConfigData entity)
        {
            string dirName = GetCurrentDirectoryForEntity(entity);

            if (!VerifyDirectory(dirName))
            {
                throw new Exception("Unable to locate or create base live data directory.");
            }

            string entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dirName + entityFileName;
            string archiveFileDirectory = GetArchiveDirectoryForEntity(entity);

            CullDirectoryCount(GetBaseBackupDirectoryForEntity(entity));

            try
            {
                RollingArchiveFile(fullFileName, archiveFileDirectory + entityFileName, archiveFileDirectory);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Removes a entity from the system and files
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool RemoveEntity(IConfigData entity)
        {
            string fileName = GetEntityFilename(entity);
            return ArchiveFile(fileName, fileName);
        }

        public string GetCurrentDirectoryForEntity(IConfigData entity)
        {
            string dirName = BaseDirectory;

            dirName += entity.Type switch
            {
                ConfigDataType.Player => "Players/" + entity.Name,
                _ => entity.Type.ToString(),
            };

            dirName += "/" + CurrentDirectoryName;

            return dirName;
        }

        public string GetCurrentDirectoryForType(Type entityType)
        {
            IConfigData entityThing = Activator.CreateInstance(entityType) as IConfigData;

            return GetCurrentDirectoryForEntity(entityThing);
        }

        private string GetBaseBackupDirectoryForEntity(IConfigData entity)
        {
            string dirName = BaseDirectory;

            dirName += entity.Type switch
            {
                ConfigDataType.Player => "Players/" + entity.Name,
                _ => entity.Type.ToString(),
            };
            dirName += "/" + ArchiveDirectoryName;

            return dirName;
        }

        private string GetArchiveDirectoryForEntity(IConfigData entity)
        {
            string dirName = BaseDirectory;

            dirName += entity.Type switch
            {
                ConfigDataType.Player => "Players/" + entity.Name,
                _ => entity.Type.ToString(),
            };
            dirName += "/" + DatedBackupDirectory;

            return dirName;
        }

        /// <summary>
        /// Archives everything
        /// </summary>
        public void ArchiveFull(ConfigDataType type, string backupName = "")
        {
            string dirName = BaseDirectory;

            switch (type)
            {
                default:
                    dirName += type.ToString();
                    break;
                case ConfigDataType.Player:
                    return; //we don't do players here, ever
            }

            string currentDirName = dirName + "/" + CurrentDirectoryName;
            string archivedDirName = dirName + "/" + ArchiveDirectoryName;

            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(currentDirName, false) && VerifyDirectory(archivedDirName))
            {
                CullDirectoryCount(archivedDirName);

                DirectoryInfo currentRoot = new(currentDirName);

                string backupDir = archivedDirName + DatedBackupDirectory;
                if (!string.IsNullOrWhiteSpace(backupName))
                {
                    backupDir = string.Format("{0}{1}/", archivedDirName, backupName);
                }

                currentRoot.CopyTo(backupDir);
            }

            //something very wrong is happening, it'll get logged
            if (!VerifyDirectory(currentDirName))
            {
                throw new Exception("Can not locate or verify current data directory.");
            }
        }

        /// <summary>
        /// Creates rolling files since backing data is dated by minute
        /// </summary>
        /// <param name="currentFile">full path of current file name</param>
        /// <param name="archiveFile">full path of archive file name</param>
        /// <param name="archiveDirectory">archive directory</param>
        private void RollingArchiveFile(string currentFile, string archiveFile, string archiveDirectory)
        {
            if (!File.Exists(currentFile))
            {
                return;
            }

            VerifyDirectory(archiveDirectory);

            if (File.Exists(archiveFile))
            {
                File.Delete(archiveFile);
            }

            File.Copy(currentFile, archiveFile, true);
        }

        /// <summary>
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        public static string GetEntityFilename(IConfigData entity)
        {
            return string.Format("{0}.{1}", entity.UniqueKey, entity.GetType().Name);
        }
    }
}

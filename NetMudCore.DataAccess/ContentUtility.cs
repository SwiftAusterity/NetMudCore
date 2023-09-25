﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NetMudCore.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetMudCore.DataAccess
{
    public static class ContentUtility
    {
        private const string BaseMusicDirectoryName = "/Content/Sounds/BkgMusic/";

        #region GameClientStuff
        /// <summary>
        /// Grab the names of the music files for this zone
        /// </summary>
        /// <param name="zone">The zone in question</param>
        /// <returns>Dict of Name, Uri</returns>
        public static IDictionary<string, string> GetMusicTracksForZone(IZone zone)
        {
            //TODO: Zone stuff
            IEnumerable<string> names = Enumerable.Empty<string>();

            if (VerifyDirectory(BaseMusicDirectoryName))
            {
               // names = Directory.EnumerateFiles(HostingEnvironment.MapPath(BaseMusicDirectoryName), "*.mp3", SearchOption.TopDirectoryOnly);
            }

            Dictionary<string, string> returnValues = new();

            foreach(string uri in names)
            {
                string songName = uri.Substring(uri.LastIndexOf('\\') + 1, uri.LastIndexOf('.') - uri.LastIndexOf('\\') - 1);
                returnValues.Add(songName, "/Content/Sounds/BkgMusic/" + songName + ".mp3");
            }

            return returnValues;
        }
        #endregion

        /// <summary>
        /// Verifies the existence of or creates a new directory, also creates the base directory if necessary
        /// </summary>
        /// <param name="directoryName">the directory to create</param>
        /// <param name="createIfMissing">creates the directory if it doesn't already exist</param>
        /// <returns>success</returns>
        private static bool VerifyDirectory(string directoryName)
        {
            //string mappedName = directoryName;

            //if (!mappedName.EndsWith("/"))
            //{
            //    mappedName += "/";
            //}

            try
            {
                return false; // Directory.Exists(.MapPath(mappedName));
            }
            catch (Exception ex)
            {
                //Log any filesystem errors
                LoggingUtility.LogError(ex, false);
            }

            return false;
        }
    }
}

﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Data.Architectural.EntityBase
{
    public abstract class LocationTemplateEntityPartial : EntityTemplatePartial, ILocationData
    {
        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>
        public virtual IEnumerable<IPathwayTemplate> GetPathways(bool withReturn = false)
        {
            return TemplateCache.GetAll<IPathwayTemplate>().Where(path => path.Origin.Equals(this) || (withReturn && path.Destination.Equals(this)));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>       
        public IEnumerable<IPathwayTemplate> GetLocalePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IRoomTemplate))
                                                        && (GetType().GetInterfaces().Contains(typeof(IZoneTemplate))
                                                            || ((IRoomTemplate)path.Destination).ParentLocation.Id != ((IRoomTemplate)this).ParentLocation.Id));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>      
        public IEnumerable<IPathwayTemplate> GetZonePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IZoneTemplate)));
        }
    }
}

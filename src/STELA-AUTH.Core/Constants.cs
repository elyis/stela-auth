using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservice_module.Core
{
    public static class Constants
    {
        public static readonly string ServerUrl = Environment.GetEnvironmentVariable("ServerUrl") ?? throw new Exception("ServerUrl is not found in enviroment variables");

        public static readonly string LocalPathToStorages = @"Resources/";
        public static readonly string LocalPathToProfileIcons = $"{LocalPathToStorages}ProfileIcons/";
        public static readonly string LocalPathToMaterialImages = $"{LocalPathToStorages}MaterialImages/";
        public static readonly string LocalPathToMemorialImages = $"{LocalPathToStorages}MemorialImages/";
        public static readonly string LocalPathToPortfolioMemorialImages = $"{LocalPathToStorages}PortfolioMemorialImages/";
        public static readonly string LocalPathToAdditionalServiceImages = $"{LocalPathToStorages}AdditionalServiceImages/";

        public static readonly string WebPathToProfileIcons = $"{ServerUrl}/api/upload/profileIcon/";
        public static readonly string WebPathToMaterialImages = $"{ServerUrl}/api/upload/material/";
        public static readonly string WebPathToMemorialImages = $"{ServerUrl}/api/upload/memorial/";
        public static readonly string WebPathToPortfolioMemorialImages = $"{ServerUrl}/api/upload/portfolio-memorial/";
        public static readonly string WebPathToAdditionalServiceImages = $"{ServerUrl}/api/upload/additional-service/";
    }
}
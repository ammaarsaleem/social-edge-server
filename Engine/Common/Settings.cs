/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Common
{
    public static class Settings
    {
         public static dynamic GameSettings    =  SocialEdge.TitleContext.GetTitleDataProperty("GameSettings");
         public static dynamic Economy         =  SocialEdge.TitleContext.GetTitleDataProperty("Economy");
         public static dynamic DynamicBundles  =  SocialEdge.TitleContext.GetTitleDataProperty("DynamicBundles");
        public static dynamic CommonSettings  = GameSettings["Common"];
        public static dynamic  MetaSettings   = GameSettings["Meta"];
        public static dynamic DynamicBundleTiers    = DynamicBundles["bundleTiers"];
        public static dynamic DynamicDisplayBundles = DynamicBundles["displayBundle"];
        public static dynamic DynamicGemSpotBundles = DynamicBundles["gemSpotBundle"];
        public static dynamic DynamicPurchaseTiers  = DynamicBundles["purchaseTiers"];
      
    }
}
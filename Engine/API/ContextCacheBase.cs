/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SocialEdgeSDK.Server.Context
{
    public class ContextCacheBase
    {
        protected enum ContextType
        {
            FUNCTION_EXECUTION_API,
            PLAYER_PLAYSTREAM,
            ENTITY_PLAYSTREAM,
            SCHEDULED_TASK
        }

        protected ContextType _contextType;
        protected dynamic _context;

        protected ulong _fillMask;
        protected ulong _dirtyMask;
        protected delegate bool CacheFnType();
        protected Dictionary<ulong, CacheFnType> _fillMap;
        protected Dictionary<ulong, CacheFnType> _writeMap;

        public bool SetDirtyBit(ulong dirtyMask)
        {
            if ((dirtyMask & CachePlayerDataSegments.READONLY) != 0)
            {
                SocialEdge.Log.LogInformation("Error: This Cache segment is readonly!");
            }

            _dirtyMask |= dirtyMask;
            return true;
        }

        protected bool CacheFillNone()
        {
            SocialEdge.Log.LogInformation("Initialize empty cache");
            return true;
        }

        protected bool CacheWriteNone()
        {
            SocialEdge.Log.LogInformation("Ignore cache write");
            return true;
        }

        protected bool CacheWriteReadOnlyError()
        {
            SocialEdge.Log.LogInformation("ERROR: Attempt to write read only cache data!");
            return true;
        }        

        public bool CacheFillSegment(ulong fetchMask)
        {
            return (bool)_fillMap[fetchMask]?.Invoke();
        }

        public bool CacheFill(ulong fetchMask)
        {
            if (fetchMask == 0)
                return true;

            ulong bit = 0x1;
            while (bit != (CachePlayerDataSegments.MAX << 1))
            {
                if ((bit & fetchMask) != 0)
                    _fillMap[bit]?.Invoke();
                
                bit <<= 1;
            }
            return true;
        }

        public bool CacheFlush()
        {
            if (_dirtyMask == 0)
                return true;

            ulong bit = 0x1;
            while (bit != (CachePlayerDataSegments.MAX << 1))
            {
                if ((bit & _dirtyMask) != 0)
                    _writeMap[bit]?.Invoke();
                
                bit <<= 1;
            }

            return true;
        }
    }

}
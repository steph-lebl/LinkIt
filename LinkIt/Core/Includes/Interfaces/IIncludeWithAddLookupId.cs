﻿#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using LinkIt.TopologicalSorting;

namespace LinkIt.Core.Includes.Interfaces
{
    internal interface IIncludeWithAddLookupId<TLink> : IInclude
    {
        Type ReferenceType { get; }
        void AddLookupId(TLink link, LookupIdContext lookupIdContext);
        void AddDependency(string linkTargetId, Dependency predecessor, LoadLinkProtocol loadLinkProtocol);
    }
}
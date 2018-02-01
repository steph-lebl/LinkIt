#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using LinkIt.TopologicalSorting;

namespace LinkIt.Core.Includes.Interfaces
{
    internal interface IIncludeWithCreateNestedLinkedSourceFromModel<TLinkedSource, TAbstractChildLinkedSource, TLink> : IInclude
    {
        TAbstractChildLinkedSource CreateNestedLinkedSourceFromModel(
            TLink link,
            LoadedReferenceContext loadedReferenceContext,
            TLinkedSource linkedSource,
            int referenceIndex,
            LoadLinkProtocol loadLinkProtocol
        );

        void AddDependenciesForAllLinkTargets(Dependency predecessor, LoadLinkProtocol loadLinkProtocol);
    }
}
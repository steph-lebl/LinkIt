﻿#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using LinkIt.Core.Includes;
using LinkIt.Core.Includes.Interfaces;
using LinkIt.Core.Interfaces;
using LinkIt.LinkTargets.Interfaces;
using LinkIt.Shared;
using LinkIt.TopologicalSorting;

namespace LinkIt.Core
{
    //See ILoadLinkExpression
    internal class LoadLinkExpressionImpl<TLinkedSource, TAbstractLinkTarget, TLink, TDiscriminant> : ILoadLinkExpression
    {
        private readonly Func<TLinkedSource, IEnumerable<TLink>> _getLinks;
        private readonly IncludeSet<TLinkedSource, TAbstractLinkTarget, TLink, TDiscriminant> _includeSet;
        private readonly ILinkTarget<TLinkedSource, TAbstractLinkTarget> _linkTarget;

        internal LoadLinkExpressionImpl(
            Func<TLinkedSource, IEnumerable<TLink>> getLinks,
            ILinkTarget<TLinkedSource, TAbstractLinkTarget> linkTarget,
            IncludeSet<TLinkedSource, TAbstractLinkTarget, TLink, TDiscriminant> includeset)
        {
            _linkTarget = linkTarget;
            _getLinks = getLinks;
            _includeSet = includeset;
            LinkedSourceType = typeof(TLinkedSource);

            ReferenceTypes = _includeSet.GetIncludesWithAddLookupId()
                .Select(include => include.ReferenceType)
                .ToList();

            ChildLinkedSourceTypes = _includeSet.GetIncludesWithChildLinkedSource()
                .Select(include => include.ChildLinkedSourceType)
                .ToList();
        }

        public List<Type> ChildLinkedSourceTypes { get; }

        public string LinkTargetId => _linkTarget.Id;
        public Type LinkedSourceType { get; }
        public List<Type> ReferenceTypes { get; }

        public void AddLookupIds(object linkedSource, LookupIdContext lookupIdContext, Type referenceTypeToBeLoaded)
        {
            AssumeLinkedSourceIsOfTLinkedSource(linkedSource);
            AssumeIsOfReferenceType(this, referenceTypeToBeLoaded);

            var notNullLinks = GetLinks((TLinkedSource) linkedSource)
                .Where(link => link != null)
                .ToList();

            foreach (var link in notNullLinks)
            {
                var include = _includeSet.GetIncludeWithAddLookupId(link);

                if (include != null && include.ReferenceType == referenceTypeToBeLoaded) include.AddLookupId(link, lookupIdContext);
            }
        }

        public void LinkNestedLinkedSourceFromModel(object linkedSource, LoadedReferenceContext loadedReferenceContext, LoadLinkProtocol loadLinkProtocol)
        {
            SetLinkTargetValues(
                linkedSource,
                _includeSet.GetIncludeWithCreateNestedLinkedSourceFromModel,
                (link, include, linkIndex) => include.CreateNestedLinkedSourceFromModel(
                    link,
                    loadedReferenceContext,
                    (TLinkedSource) linkedSource,
                    linkIndex,
                    loadLinkProtocol
                )
            );
        }

        public void LinkReference(object linkedSource, LoadedReferenceContext loadedReferenceContext)
        {
            SetLinkTargetValues(
                linkedSource,
                _includeSet.GetIncludeWithGetReference,
                (link, include, linkIndex) => include.GetReference(link, loadedReferenceContext)
            );
        }

        public void LinkNestedLinkedSourceById(
            object linkedSource,
            LoadedReferenceContext loadedReferenceContext,
            Type referenceTypeToBeLinked,
            LoadLinkProtocol loadLinkProtocol
        )
        {
            SetLinkTargetValues(
                linkedSource,
                link => _includeSet.GetIncludeWithCreateNestedLinkedSourceByIdForReferenceType(link, referenceTypeToBeLinked),
                (link, include, linkIndex) =>
                    include.CreateNestedLinkedSourceById(
                        link,
                        loadedReferenceContext,
                        (TLinkedSource) linkedSource,
                        linkIndex,
                        loadLinkProtocol
                    )
            );
        }

        public void FilterOutNullValues(object linkedSource)
        {
            AssumeLinkedSourceIsOfTLinkedSource(linkedSource);

            _linkTarget.FilterOutNullValues((TLinkedSource) linkedSource);
        }

        public void AddDependencyForEachInclude(Dependency predecessor, LoadLinkProtocol loadLinkProtocol)
        {
            AddDependenciesForAllReferences(predecessor, loadLinkProtocol);
            AddDependenciesForAllNestedLinkedSources(predecessor, loadLinkProtocol);
        }

        private void SetLinkTargetValues<TInclude>(
            object linkedSource,
            Func<TLink, TInclude> getInclude,
            Func<TLink, TInclude, int, TAbstractLinkTarget> getLinkTargetValue)
        {
            AssumeLinkedSourceIsOfTLinkedSource(linkedSource);

            SetLinkTargetValues(
                (TLinkedSource) linkedSource,
                getInclude,
                getLinkTargetValue
            );
        }


        private void SetLinkTargetValues<TInclude>(
            TLinkedSource linkedSource,
            Func<TLink, TInclude> getInclude,
            Func<TLink, TInclude, int, TAbstractLinkTarget> getLinkTargetValue)
        {
            var links = GetLinks(linkedSource);
            _linkTarget.LazyInit(linkedSource, links.Count);

            for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
            {
                var link = links[linkIndex];
                if (link == null) continue;

                var include = getInclude(link);
                if (include == null) continue;

                _linkTarget.SetLinkTargetValue(
                    linkedSource,
                    getLinkTargetValue(link, include, linkIndex),
                    linkIndex
                );
            }
        }

        private List<TLink> GetLinks(TLinkedSource linkedSource)
        {
            var links = _getLinks(linkedSource);

            return links?.ToList() ?? new List<TLink>();
        }

        private static void AssumeLinkedSourceIsOfTLinkedSource(object linkedSource)
        {
            if (!(linkedSource is TLinkedSource))
                throw new AssumptionFailed(
                    $"Cannot invoke load-link expression for {typeof(TLinkedSource)} with linked source of type {linkedSource?.GetType()}"
                );
        }

        private static void AssumeIsOfReferenceType(ILoadLinkExpression loadLinkExpression, Type referenceType)
        {
            if (!loadLinkExpression.ReferenceTypes.Contains(referenceType))
                throw new AssumptionFailed(
                    $"Cannot invoke this load link expression for reference type {referenceType}. Supported reference types are {string.Join(",", loadLinkExpression.ReferenceTypes)}. This load link expression is for {loadLinkExpression.LinkedSourceType}."
                );
        }

        private void AddDependenciesForAllNestedLinkedSources(Dependency predecessor, LoadLinkProtocol loadLinkProtocol)
        {
            foreach (var include in _includeSet.GetIncludes<IIncludeWithCreateNestedLinkedSourceFromModel<TLinkedSource, TAbstractLinkTarget, TLink>>())
            {
                include.AddDependenciesForAllLinkTargets(predecessor, loadLinkProtocol);
            }
        }

        private void AddDependenciesForAllReferences(Dependency predecessor, LoadLinkProtocol loadLinkProtocol)
        {
            foreach (var include in _includeSet.GetIncludes<IIncludeWithAddLookupId<TLink>>())
            {
                include.AddDependency(LinkTargetId, predecessor, loadLinkProtocol);
            }
        }
    }
}
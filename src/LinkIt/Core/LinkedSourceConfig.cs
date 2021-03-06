// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using LinkIt.Core.Includes;
using LinkIt.Core.Includes.Interfaces;
using LinkIt.LinkTargets.Interfaces;
using LinkIt.PublicApi;
using LinkIt.Shared;

namespace LinkIt.Core
{
    /// <inheritdoc />
    internal class LinkedSourceConfig<TLinkedSource, TLinkedSourceModel> : IGenericLinkedSourceConfig<TLinkedSource>
        where TLinkedSource : class, ILinkedSource<TLinkedSourceModel>, new()
    {
        public LinkedSourceConfig()
        {
            LinkedSourceType = typeof(TLinkedSource);
            LinkedSourceModelType = typeof(TLinkedSourceModel);
        }

        public Type LinkedSourceType { get; }
        public Type LinkedSourceModelType { get; }

        public ILoadLinker<TLinkedSource> CreateLoadLinker(Func<IReferenceLoader> createReferenceLoader,
            List<List<Type>> referenceTypeToBeLoadedForEachLoadingLevel,
            LoadLinkProtocol loadLinkProtocol)
        {
            return new LoadLinkerWrapper<TLinkedSource, TLinkedSourceModel>(createReferenceLoader, referenceTypeToBeLoadedForEachLoadingLevel, loadLinkProtocol);
        }


        public IInclude CreateIncludeNestedLinkedSourceById<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TId>(
            Func<TLink, TId> getLookupId,
            Action<TLinkTargetOwner, int, TLinkedSource> initChildLinkedSource)
        {
            EnsureIsAssignableFrom<TAbstractChildLinkedSource, TLinkedSource>();

            return new IncludeNestedLinkedSourceById<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TLinkedSource, TLinkedSourceModel, TId>(
                getLookupId,
                initChildLinkedSource
            );
        }

        public IInclude CreateIncludeNestedLinkedSourceById<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TId>(
            Func<TLink, TId> getLookupId,
            Action<TLink, TLinkedSource> initChildLinkedSource)
        {
            EnsureIsAssignableFrom<TAbstractChildLinkedSource, TLinkedSource>();

            return new IncludeNestedLinkedSourceById<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TLinkedSource, TLinkedSourceModel, TId>(
                getLookupId,
                initChildLinkedSource
            );
        }

        public IInclude CreateIncludeNestedLinkedSourceFromModel<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TChildLinkedSourceModel>(
            Func<TLink, TChildLinkedSourceModel> getNestedLinkedSourceModel,
            ILinkTarget linkTarget,
            Action<TLinkTargetOwner, int, TLinkedSource> initChildLinkedSource)
        {
            EnsureGetNestedLinkedSourceModelReturnsTheExpectedType<TChildLinkedSourceModel>(linkTarget);
            EnsureIsAssignableFrom<TAbstractChildLinkedSource, TLinkedSource>();

            return new IncludeNestedLinkedSourceFromModel<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TLinkedSource, TLinkedSourceModel>(
                WrapGetNestedLinkedSourceModel(getNestedLinkedSourceModel),
                initChildLinkedSource
            );
        }

        public IInclude CreateIncludeNestedLinkedSourceFromModel<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TChildLinkedSourceModel>(
            Func<TLink, TChildLinkedSourceModel> getNestedLinkedSourceModel,
            ILinkTarget linkTarget,
            Action<TLink, TLinkedSource> initChildLinkedSource)
        {
            EnsureGetNestedLinkedSourceModelReturnsTheExpectedType<TChildLinkedSourceModel>(linkTarget);
            EnsureIsAssignableFrom<TAbstractChildLinkedSource, TLinkedSource>();

            return new IncludeNestedLinkedSourceFromModel<TLinkTargetOwner, TAbstractChildLinkedSource, TLink, TLinkedSource, TLinkedSourceModel>(
                WrapGetNestedLinkedSourceModel(getNestedLinkedSourceModel),
                initChildLinkedSource
            );
        }

        private Func<TLink, TLinkedSourceModel> WrapGetNestedLinkedSourceModel<TLink, TChildLinkedSourceModel>(
            Func<TLink, TChildLinkedSourceModel> getNestedLinkedSourceModel)
        {
            return link => (TLinkedSourceModel) (object) getNestedLinkedSourceModel(link);
        }

        private void EnsureIsAssignableFrom<TAbstract, TConcrete>()
        {
            var abstractType = typeof(TAbstract);
            var concreteType = typeof(TConcrete);

            if (!abstractType.IsAssignableFrom(concreteType))
                throw new LinkItException(
                    $"{abstractType} is not assignable from {concreteType}."
                );
        }

        private void EnsureGetNestedLinkedSourceModelReturnsTheExpectedType<TChildLinkedSourceModel>(ILinkTarget linkTarget)
        {
            if (!LinkedSourceModelType.IsAssignableFrom(typeof(TChildLinkedSourceModel)))
                throw new ArgumentException(
                    $"{linkTarget.Id}: getNestedLinkedSourceModel returns an invalid type. {LinkedSourceModelType} is not assignable from {typeof(TChildLinkedSourceModel)}."
                );
        }
    }
}
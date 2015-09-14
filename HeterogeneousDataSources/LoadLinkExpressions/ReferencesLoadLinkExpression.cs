﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace HeterogeneousDataSources.LoadLinkExpressions {
    public class ReferencesLoadLinkExpression<TLinkedSource, TReference, TId>
        : LoadLinkExpression<TLinkedSource, TReference, TId>, ILoadLinkExpression
    {
        private readonly Func<TLinkedSource, List<TId>> _getLookupIdsFunc;
        private readonly LinkTarget<TLinkedSource, TReference> _linkTarget;
        private readonly Action<TLinkedSource, List<TReference>> _linkAction;

        public ReferencesLoadLinkExpression(
            string linkTargetId,
            Func<TLinkedSource, List<TId>> getLookupIdsFunc,
            Action<TLinkedSource, List<TReference>> linkAction) 
        {
            EnsureGenericParameterCannotBeList<TLinkedSource>(linkTargetId, "TLinkedSource");
            EnsureGenericParameterCannotBeList<TReference>(linkTargetId, "TReference");
            EnsureGenericParameterCannotBeList<TId>(linkTargetId, "TId");

            _getLookupIdsFunc = getLookupIdsFunc;
            _linkAction = linkAction;
            ModelType = typeof(TReference);
        }

        //stle: delete
        [Obsolete]
        public ReferencesLoadLinkExpression(
            Func<TLinkedSource, List<TId>> getLookupIdsFunc,
            Action<TLinkedSource, List<TReference>> linkAction)
            : this("obsolete",getLookupIdsFunc,linkAction) 
        { }

        protected override List<TId> GetLookupIdsTemplate(TLinkedSource linkedSource)
        {
            return _getLookupIdsFunc(linkedSource);
        }

        protected override void LinkAction(TLinkedSource linkedSource, List<TReference> references, LoadedReferenceContext loadedReferenceContext)
        {
            _linkAction(linkedSource, references);
        }

        public override LoadLinkExpressionType LoadLinkExpressionType {
            get { return LoadLinkExpressionType.Reference; }
        }

        public Type ModelType { get; private set; }

        private static void EnsureGenericParameterCannotBeList<T>(string context, string genericParameterName)
        {
            if (typeof(List<>).IsAssignableFrom(typeof(T))){
                throw new ArgumentException(
                    string.Format("{0}: {1} cannot be a list.", context, genericParameterName),
                    genericParameterName
                );
            }
        }

    }
}

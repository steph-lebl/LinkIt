﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HeterogeneousDataSources.LoadLinkExpressions {
    public class NestedLinkedSourceLoadLinkExpression<TLinkedSource, TNestedLinkedSource, TNestedLinkedSourceModel, TId>
        : LoadLinkExpression<TLinkedSource, TNestedLinkedSourceModel, TId>, ILoadLinkExpression
        where TNestedLinkedSource : class, ILinkedSource<TNestedLinkedSourceModel>, new() 
    {
        private readonly Func<TLinkedSource, TId> _getLookupIdFunc;
        private readonly Action<TLinkedSource, TNestedLinkedSource> _linkAction;

        public NestedLinkedSourceLoadLinkExpression(Func<TLinkedSource, TId> getLookupIdFunc, Action<TLinkedSource, TNestedLinkedSource> linkAction)
        {
            _getLookupIdFunc = getLookupIdFunc;
            _linkAction = linkAction;
        }

        protected override List<TId> GetLookupIdsTemplate(TLinkedSource linkedSource)
        {
            //stle: hey you and your inheritance crap! Try a functional approach
            return new List<TId>{
                _getLookupIdFunc(linkedSource)
            };
        }

        protected override void LinkAction(TLinkedSource linkedSource, List<TNestedLinkedSourceModel> references, LoadedReferenceContext loadedReferenceContext)
        {
            var nestedLinkedSource = LoadLinkExpressionUtil.CreateLinkedSources<TNestedLinkedSource, TNestedLinkedSourceModel>(references);
            loadedReferenceContext.AddLinkedSourcesToBeBuilt(nestedLinkedSource);
            _linkAction(linkedSource, nestedLinkedSource.SingleOrDefault());
        }

        public override LoadLinkExpressionType LoadLinkExpressionType {
            get { return LoadLinkExpressionType.NestedLinkedSource; }
        }

    }
}
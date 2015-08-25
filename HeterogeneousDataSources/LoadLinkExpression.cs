﻿using System;
using System.Collections.Generic;

namespace HeterogeneousDataSources {
    public class LoadLinkExpression<TLinkedSource, TReference, TId> : ILoadLinkExpression<TId>, ILoadLinkExpression
    {
        public LoadLinkExpression(Func<TLinkedSource, TId> getLookupIdFunc, Action<TLinkedSource, TReference> linkAction)
        {
            GetLookupIdFunc = getLookupIdFunc;
            LinkAction = linkAction;
        }

        #region Load
        public List<TId> GetLookupIds(object linkedSource) {
            if (!(linkedSource is TLinkedSource)) { return new List<TId>(); }

            return GetLookupIds((TLinkedSource) linkedSource);
        }

        private List<TId> GetLookupIds(TLinkedSource linkedSource) {
            return new List<TId> { GetLookupIdFunc(linkedSource) };
        }
        private Func<TLinkedSource, TId> GetLookupIdFunc { get; set; }


        public Type ReferenceType { get { return typeof(TReference); } }

        #endregion

        #region Link

        public void Link(object linkedSource, LoadedReferenceContext loadedReferenceContext) {
            if (!(linkedSource is TLinkedSource)) { return; }

            Link((TLinkedSource)linkedSource, loadedReferenceContext);
        }

        private void Link(TLinkedSource linkedSource, LoadedReferenceContext loadedReferenceContext) {
            var id = GetLookupIdFunc(linkedSource);
            var reference = loadedReferenceContext.GetOptionalReference<TReference, TId>(id);
            LinkAction(linkedSource, reference);
        }

        private Action<TLinkedSource, TReference> LinkAction { get; set; }
 
	    #endregion
    }
}

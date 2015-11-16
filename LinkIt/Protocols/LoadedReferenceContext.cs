﻿using System;
using System.Collections.Generic;
using System.Linq;
using LinkIt.LinkedSources.Interfaces;

namespace LinkIt.Protocols {
    public class LoadedReferenceContext {
        private readonly List<object> _linkedSourcesToBeBuilt = new List<object>();
        private readonly Dictionary<Type, object> _referenceDictionaryByReferenceType= new Dictionary<Type, object>();

        public void AddReferences<TReference, TId>(List<TReference> references, Func<TReference,TId> getReferenceId)
        {
            if (references == null) { throw new ArgumentNullException("references"); }
            if (getReferenceId == null) { throw new ArgumentNullException("getReferenceId"); }

            var referenceDictionary = references.ToDictionary(
                getReferenceId,
                reference => reference
            );

            AddReferences(referenceDictionary);
        }

        public void AddReferences<TReference, TId>(IDictionary<TId,TReference> referencesById) {
            if (referencesById == null) { throw new ArgumentNullException("referencesById"); }

            var tReference = typeof(TReference);
            if (_referenceDictionaryByReferenceType.ContainsKey(tReference)){
                throw new InvalidOperationException(
                    string.Format(
                        "All references of the same type ({0}) must be loaded at the same time.",
                        tReference.Name)
                );
            }

            var referenceDictionary = referencesById.ToDictionary(
                referenceById => referenceById.Key,
                referenceById => referenceById.Value
            );

            _referenceDictionaryByReferenceType.Add(tReference, referenceDictionary);

        }

        public List<object> LinkedSourcesToBeBuilt{
            get { return _linkedSourcesToBeBuilt.ToList(); }
        }

        private Dictionary<TId, TReference> GetReferenceDictionary<TReference, TId>() {
            var tReference = typeof (TReference);
            if (!_referenceDictionaryByReferenceType.ContainsKey(tReference)) {
                throw new InvalidOperationException(
                    string.Format(
                        "References of type {0} were not loaded. Note that the implementation of IReferenceLoader must invoke LoadedReferenceContext.AddReferences with an empty set if none of the ids provided in the LookupIdContext for a specific reference type can be loaded.",
                        tReference.Name)
                );
            }

            return (Dictionary<TId, TReference>)_referenceDictionaryByReferenceType[tReference];
        }

        public TReference GetOptionalReference<TReference, TId>(TId lookupId) {
            if (lookupId == null) { return default(TReference); }

            var referenceDictionnary = GetReferenceDictionary<TReference, TId>();

            if (!referenceDictionnary.ContainsKey(lookupId)) { return default(TReference); }

            return referenceDictionnary[lookupId];
        }

        public List<TReference> GetOptionalReferences<TReference, TId>(List<TId> lookupIds) {
            return lookupIds
                .Select(GetOptionalReference<TReference, TId>)
                .ToList();
        }

        //stle: TLinkedSourceModel not required, check this pattern everywhere
        public TLinkedSource CreatePartiallyBuiltLinkedSource<TLinkedSource, TLinkedSourceModel>(TLinkedSourceModel model, LoadLinkProtocol loadLinkProtocol, Action<TLinkedSource> init)
            where TLinkedSource : class, ILinkedSource<TLinkedSourceModel>, new()
        {
            if (model == null) { return null; }

            var linkedSource = new TLinkedSource { Model = model };
            if (init != null){
                init(linkedSource);
            }
            LinkNestedLinkedSourcesFromModel(linkedSource, loadLinkProtocol);

            _linkedSourcesToBeBuilt.Add(linkedSource);
            return linkedSource;
        }

        private void LinkNestedLinkedSourcesFromModel(object linkedSource, LoadLinkProtocol loadLinkProtocol)
        {
            var loadLinkExpressions = loadLinkProtocol.GetLoadLinkExpressions(linkedSource);
            foreach (var loadLinkExpression in loadLinkExpressions){
                loadLinkExpression.LinkNestedLinkedSourceFromModel(
                    linkedSource,
                    this,
                    loadLinkProtocol
                );
            }
        }

    }
}
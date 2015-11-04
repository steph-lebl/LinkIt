﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HeterogeneousDataSources {
    public class LoadedReferenceContext {
        private readonly List<object> _linkedSourcesToBeBuilt = new List<object>();
        private readonly List<object> _linkedSourcesWhereSubLinkedSourceAreLinked = new List<object>();
        private readonly Dictionary<Type, object> _referenceDictionaryByReferenceType= new Dictionary<Type, object>();

        public void AddReferences<TReference, TId>(List<TReference> references, Func<TReference,TId> getReferenceIdFunc){
            var tReference = typeof (TReference);
            if (_referenceDictionaryByReferenceType.ContainsKey(tReference)) {
                throw new InvalidOperationException(
                    string.Format(
                        "All references of the same type ({0}) must be loaded at the same time.",
                        tReference.Name
                    )
                );
            }

            var referenceDictionnary = references.ToDictionary(
                keySelector: getReferenceIdFunc,
                elementSelector: reference => reference 
            );

            _referenceDictionaryByReferenceType[tReference] = referenceDictionnary;
        }

        //stle: temporary work around for image, fix the root cause by solving the query vs id problem
        public void AddReference<TReference, TId>(TReference reference, TId id) {
            var tReference = typeof(TReference);
            if (!_referenceDictionaryByReferenceType.ContainsKey(tReference)) {
                _referenceDictionaryByReferenceType.Add(tReference, new Dictionary<TId, TReference>());
            }

            var referenceDictionnary = (Dictionary<TId, TReference>)
                _referenceDictionaryByReferenceType[tReference];

            referenceDictionnary.Add(id,reference);
        }

        public List<object> LinkedSourcesToBeBuilt{
            get { return _linkedSourcesToBeBuilt.ToList(); }
        }

        public void AddLinkedSourceWhereSubLinkedSourceAreLinked(object linkedSource){
            //assume linkedSource cannot be null
            _linkedSourcesWhereSubLinkedSourceAreLinked.Add(linkedSource);
        }

        public List<object> GetLinkedSourceWithSubLinkedSourceToLink()
        {
            return _linkedSourcesToBeBuilt
                .Except(_linkedSourcesWhereSubLinkedSourceAreLinked)
                .ToList();
        }

        private Dictionary<TId, TReference> GetReferenceDictionary<TReference, TId>() {
            var tReference = typeof (TReference);
            if (!_referenceDictionaryByReferenceType.ContainsKey(tReference)) {
                throw new InvalidOperationException(
                    string.Format(
                        "References of type {0} were not loaded.",
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
        public TLinkedSource CreatePartiallyBuiltLinkedSource<TLinkedSource, TLinkedSourceModel>(TLinkedSourceModel model)
            where TLinkedSource : class, ILinkedSource<TLinkedSourceModel>, new()
        {
            if (model == null) { return null; }

            var linkedSource = new TLinkedSource { Model = model };
            _linkedSourcesToBeBuilt.Add(linkedSource);

            return linkedSource;
        }
    }
}
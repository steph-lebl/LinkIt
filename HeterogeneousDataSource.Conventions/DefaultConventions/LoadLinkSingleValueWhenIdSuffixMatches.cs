﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using HeterogeneousDataSource.Conventions.Interfaces;
using HeterogeneousDataSources;
using HeterogeneousDataSources.ConfigBuilders;
using HeterogeneousDataSources.LinkedSources;

namespace HeterogeneousDataSource.Conventions.DefaultConventions {
    public class LoadLinkSingleValueWhenIdSuffixMatches : ISingleValueConvention {
        public string Name {
            get { return "Load link single value when id suffix matches"; }
        }

        public bool DoesApply(
            PropertyInfo linkTargetProperty,
            PropertyInfo linkedSourceModelProperty) 
        {
            return linkTargetProperty.MatchLinkedSourceModelPropertyName(linkedSourceModelProperty, "Id");
        }

        public void Apply<TLinkedSource, TLinkTargetProperty, TLinkedSourceModelProperty>(
            LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource> loadLinkProtocolForLinkedSourceBuilder, 
            Expression<Func<TLinkedSource, TLinkTargetProperty>> getLinkTargetProperty,
            Func<TLinkedSource, TLinkedSourceModelProperty> getLinkedSourceModelProperty,
            PropertyInfo linkTargetProperty, 
            PropertyInfo linkedSourceModelProperty)
        {
            if (LinkedSourceConfigs.DoesImplementILinkedSourceOnceAndOnlyOnce(typeof(TLinkTargetProperty))) {
                loadLinkProtocolForLinkedSourceBuilder.LoadLinkNestedLinkedSourceById(
                    getLinkedSourceModelProperty,
                    getLinkTargetProperty
                );
            }
            else{
                loadLinkProtocolForLinkedSourceBuilder.LoadLinkReferenceById(
                    getLinkedSourceModelProperty,
                    getLinkTargetProperty
                );
            }
        }
    }
}

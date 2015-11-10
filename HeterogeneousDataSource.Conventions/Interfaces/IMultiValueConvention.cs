﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HeterogeneousDataSources;
using HeterogeneousDataSources.ConfigBuilders;

namespace HeterogeneousDataSource.Conventions.Interfaces
{
    public interface IMultiValueConvention:ILoadLinkExpressionConvention
    {
        void Apply<TLinkedSource, TLinkTargetProperty, TLinkedSourceModelProperty>(
            LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource> loadLinkProtocolForLinkedSourceBuilder,
            Expression<Func<TLinkedSource, List<TLinkTargetProperty>>> getLinkTargetProperty,
            Func<TLinkedSource, List<TLinkedSourceModelProperty>> getLinkedSourceModelProperty,
            PropertyInfo linkTargetProperty,
            PropertyInfo linkedSourceModelProperty
        );
    }
}
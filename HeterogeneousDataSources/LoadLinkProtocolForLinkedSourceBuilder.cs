﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HeterogeneousDataSources.LoadLinkExpressions;

namespace HeterogeneousDataSources
{

    //stle: enhance that: TChildLinkedSourceModel could be infered by reflection
    //stle: enhance that: TId could dispear after query are supported
    public class LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource>
    {
        private readonly Action<ILoadLinkExpression> _addLoadLinkExpressionAction;

        public LoadLinkProtocolForLinkedSourceBuilder(Action<ILoadLinkExpression> addLoadLinkExpressionAction)
        {
            _addLoadLinkExpressionAction = addLoadLinkExpressionAction;
        }

        public LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource> LoadLinkReference<TReference, TId>(
            Func<TLinkedSource, TId> getLookupIdFunc,
            Expression<Func<TLinkedSource, TReference>> linkTargetFunc) 
        {
            var linkTarget = LinkTargetFactory.Create(linkTargetFunc);

            var loadLinkExpression = new ReferencesLoadLinkExpression<TLinkedSource, TReference, TId>(
                linkTarget.Id,
                linkedSource => 
                    new List<TId>{ getLookupIdFunc(linkedSource)},
                (linkedSource, references) => 
                    linkTarget.SetTargetProperty(linkedSource, references.SingleOrDefault())
            );

            _addLoadLinkExpressionAction(loadLinkExpression);

            return this;
        }

        public LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource> LoadLinkReference<TReference, TId>(
            Func<TLinkedSource, List<TId>> getLookupIdFunc,
            Expression<Func<TLinkedSource, List<TReference>>> linkTargetFunc) 
        {
            var linkTarget = LinkTargetFactory.Create(linkTargetFunc);

            var loadLinkExpression = new ReferencesLoadLinkExpression<TLinkedSource, TReference, TId>(
                linkTarget.Id,
                getLookupIdFunc,
                linkTarget.SetTargetProperty
            );

            _addLoadLinkExpressionAction(loadLinkExpression);

            return this;
        }


        public LoadLinkProtocolForLinkedSourceBuilder<TLinkedSource> IsRoot<TId>()
        {
            var loadLinkExpression = CreateRootLoadLinkExpression<TId>();

            _addLoadLinkExpressionAction(loadLinkExpression);

            return this;
        }

        private ILoadLinkExpression CreateRootLoadLinkExpression<TId>()
        {
            Type rootLoadLinkExpressionGenericType = typeof(RootLoadLinkExpression<,,>);
            Type[] typeArgs = { typeof(TLinkedSource), GetLinkedSourceModelType(), typeof(TId) };
            Type rootLoadLinkExpressionSpecificType = rootLoadLinkExpressionGenericType.MakeGenericType(typeArgs);

            return (ILoadLinkExpression)Activator.CreateInstance(rootLoadLinkExpressionSpecificType);
        }

        private Type GetLinkedSourceModelType()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var iLinkedSourceTypes = linkedSourceType.GetInterfaces()
                .Where(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(ILinkedSource<>))
                .ToList();

            EnsureILinkedSourceIsImplementedOnceAndOnlyOnce(iLinkedSourceTypes);

            var iLinkedSourceType = iLinkedSourceTypes.Single();
            return iLinkedSourceType.GenericTypeArguments.Single();
        }

        private static void EnsureILinkedSourceIsImplementedOnceAndOnlyOnce(List<Type> iLinkedSourceTypes)
        {
            if (!iLinkedSourceTypes.Any()){
                throw new ArgumentException(string.Format("{0} must implement ILinkedSource<> at least once."), "TLinkedSource");
            }

            if (iLinkedSourceTypes.Count > 1){
                throw new ArgumentException(string.Format("{0} must implement ILinkedSource<> only once."), "TLinkedSource");
            }
        }
    }
}
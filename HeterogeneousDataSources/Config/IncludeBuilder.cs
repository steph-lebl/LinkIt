using System;
using System.Linq;
using HeterogeneousDataSources.LoadLinkExpressions.Includes;

namespace HeterogeneousDataSources
{
    public class IncludeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant, TTargetConcreteType>
        where TTargetConcreteType : TIChildLinkedSource
    {
        private readonly IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> _includeTargetConcreteTypeBuilder;

        public IncludeBuilder(IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> includeTargetConcreteTypeBuilder)
        {
            _includeTargetConcreteTypeBuilder = includeTargetConcreteTypeBuilder;
        }

        public IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> AsNestedLinkedSource<TId>(
            TDiscriminant discriminantValue,
            Func<TLink, TId> getLookupIdFunc,
            Action<TLinkedSource, int, TTargetConcreteType> initChildLinkedSourceAction = null
            ) 
        {
            _includeTargetConcreteTypeBuilder.AddInclude(
                discriminantValue,
                CreatePolymorphicNestedLinkedSourceInclude(getLookupIdFunc, initChildLinkedSourceAction)
            );

            return _includeTargetConcreteTypeBuilder;
        }

        //stle: can we avoid reflection here?
        private IInclude CreatePolymorphicNestedLinkedSourceInclude<TId>(
            Func<TLink, TId> getLookupIdFunc,
            Action<TLinkedSource, int, TTargetConcreteType> initChildLinkedSourceAction) 
        {
            Type ctorGenericType = typeof(NestedLinkedSourceInclude<,,,,,>);

            var targetConcreteTypeType = typeof(TTargetConcreteType);
            Type[] typeArgs ={
                typeof(TLinkedSource),
                typeof(TIChildLinkedSource), 
                typeof(TLink),
                targetConcreteTypeType, 
                LoadLinkProtocolForLinkedSourceBuilder<string>.GetLinkedSourceModelType(targetConcreteTypeType),
                typeof(TId)
            };

            Type ctorSpecificType = ctorGenericType.MakeGenericType(typeArgs);

            //stle: change to single once obsolete constructor is deleted
            var ctor = ctorSpecificType.GetConstructors().First();

            return (IInclude)ctor.Invoke(
                new object[]{
                    getLookupIdFunc,
                    initChildLinkedSourceAction
                }
            );
        }

        public IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> AsSubLinkedSource(
            TDiscriminant discriminantValue)
        {
            //stle: should not have to know that TLink = TChildLinkedSourceModel if getSubLinkedSourceModel is null
            return AsSubLinkedSource<TLink>(discriminantValue, null);
        }

        //stle:dry
        public IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> AsSubLinkedSource<TChildLinkedSourceModel>(
            TDiscriminant discriminantValue,
            Func<TLink, TChildLinkedSourceModel> getSubLinkedSourceModel) 
        {
            _includeTargetConcreteTypeBuilder.AddInclude(
                discriminantValue,
                CreatePolymorphicSubLinkedSourceInclude(
                    getSubLinkedSourceModel
                )
            );

            return _includeTargetConcreteTypeBuilder;
        }

        //stle: can we avoid reflection here?
        private IInclude CreatePolymorphicSubLinkedSourceInclude<TChildLinkedSourceModel>(
            Func<TLink, TChildLinkedSourceModel> getSubLinkedSourceModel) 
        {
            Type ctorGenericType = typeof(SubLinkedSourceInclude<,,,>);

            //stle: test this 
            //stle: better error msg
            //Ensure TChildLinkedSourceModel == LoadLinkProtocolForLinkedSourceBuilder<string>.GetLinkedSourceModelType(TTargetConcreteType)

            var targetConcreteTypeType = typeof(TTargetConcreteType);
            Type[] typeArgs ={
                typeof(TIChildLinkedSource), 
                typeof(TLink),
                targetConcreteTypeType, 
                LoadLinkProtocolForLinkedSourceBuilder<string>.GetLinkedSourceModelType(targetConcreteTypeType),
            };

            Type ctorSpecificType = ctorGenericType.MakeGenericType(typeArgs);

            var ctor = ctorSpecificType.GetConstructors().First();

            return (IInclude)ctor.Invoke(
                new object[]{
                    getSubLinkedSourceModel
                }
            );
        }

        public IncludeTargetConcreteTypeBuilder<TLinkedSource, TIChildLinkedSource, TLink, TDiscriminant> AsReference<TId>(
            TDiscriminant discriminantValue,
            Func<TLink, TId> getLookupIdFunc
        )
        {
            _includeTargetConcreteTypeBuilder.AddInclude(
                discriminantValue,
                new ReferenceInclude<TIChildLinkedSource, TLink, TTargetConcreteType, TId>(
                    getLookupIdFunc
                )
            );

            return _includeTargetConcreteTypeBuilder;
        }
    }
}
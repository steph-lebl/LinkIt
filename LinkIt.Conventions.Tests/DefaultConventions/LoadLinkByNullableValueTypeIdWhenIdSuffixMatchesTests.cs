﻿#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using System.Collections.Generic;
using LinkIt.ConfigBuilders;
using LinkIt.Conventions.DefaultConventions;
using LinkIt.Conventions.Interfaces;
using LinkIt.PublicApi;
using LinkIt.TestHelpers;
using Xunit;

namespace LinkIt.Conventions.Tests.DefaultConventions
{
    public class LoadLinkByNullableValueTypeIdWhenIdSuffixMatchesTests
    {
        [Fact]
        public void GetLinkedSourceTypes()
        {
            var model = new Model
            {
                Id = "One",
                MediaReferenceId = 1,
                MediaNestedLinkedSourceId = 2
            };
            var sut = BuildLoadLinkProtocol();

            var actual = sut.LoadLink<LinkedSource>().FromModel(model);

            Assert.Same(model, actual.Model);
            Assert.Equal(model.MediaNestedLinkedSourceId, actual.MediaNestedLinkedSource.Id);
            Assert.Equal(model.MediaReferenceId, actual.MediaReference.Id);
        }

        private static ILoadLinkProtocol BuildLoadLinkProtocol()
        {
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
            loadLinkProtocolBuilder.ApplyConventions(
                new List<Type> { typeof(LinkedSource) },
                new List<ILoadLinkExpressionConvention> { new LoadLinkByNullableValueTypeIdWhenIdSuffixMatches() }
            );
            return loadLinkProtocolBuilder.Build(() => new ReferenceLoaderStub());
        }

        private class LinkedSource : ILinkedSource<Model>
        {
            public Media MediaReference { get; set; }
            public Media MediaNestedLinkedSource { get; set; }
            public Model Model { get; set; }
        }

        private class Model
        {
            public string Id { get; set; }
            public int? MediaReferenceId { get; set; }
            public int? MediaNestedLinkedSourceId { get; set; }
        }
    }
}
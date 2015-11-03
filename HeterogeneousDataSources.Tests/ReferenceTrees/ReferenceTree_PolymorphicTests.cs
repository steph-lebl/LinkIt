﻿using System.Collections.Generic;
using ApprovalTests.Reporters;
using HeterogeneousDataSources.Tests.Shared;
using NUnit.Framework;
using RC.Testing;

namespace HeterogeneousDataSources.Tests {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ReferenceTree_PolymorphicTests
    {
        [Test]
        public void CreateRootReferenceTree() {
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
            loadLinkProtocolBuilder.For<LinkedSource>()
                .PolymorphicLoadLink(
                    linkedSource => linkedSource.Model.PolyRef,
                    linkedSource => linkedSource.Poly,
                    link => link.Kind,
                    includes => includes
                        .Include<PersonLinkedSource>().AsNestedLinkedSource(
                            "person-nested",
                            link => (string)link.Value
                        )
                        .Include<PersonLinkedSource>().AsSubLinkedSource(
                            "person-sub",
                            link => (Person)link.Value
                        )
                        .Include<Image>().AsReference(
                            "img",
                            link=>(string)link.Value
                        )
                );
            loadLinkProtocolBuilder.For<PersonLinkedSource>()
                .LoadLinkReference(
                    linkedSource => linkedSource.Model.SummaryImageId,
                    linkedSource => linkedSource.SummaryImage
                );
            var sut = new LoadLinkConfig(loadLinkProtocolBuilder.GetLoadLinkExpressions());

            var actual = sut.CreateRootReferenceTree<LinkedSource>();

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        public class LinkedSource : ILinkedSource<Model> {
            public Model Model { get; set; }
            public object Poly { get; set; }
        }

        public class Model {
            public int Id { get; set; }
            public PolymorphicRef PolyRef { get; set; }
        }

        public class PolymorphicRef
        {
            public string Kind { get; set; }
            public object Value { get; set; }
        }
    }
}
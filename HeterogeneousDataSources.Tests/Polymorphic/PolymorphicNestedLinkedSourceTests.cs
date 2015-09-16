﻿using System.Collections.Generic;
using System.Linq;
using ApprovalTests.Reporters;
using HeterogeneousDataSources.Tests.Shared;
using NUnit.Framework;
using RC.Testing;

namespace HeterogeneousDataSources.Tests.Polymorphic {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class PolymorphicNestedLinkedSourceTests {
        private FakeReferenceLoader<WithNestedPolymorphicContent, string> _fakeReferenceLoader;
        private LoadLinkProtocol _sut;

        [SetUp]
        public void SetUp() {
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
            loadLinkProtocolBuilder.For<WithNestedPolymorphicContentLinkedSource>()
                .IsRoot<string>()
                .LoadLinkNestedLinkedSource(
                    linkedSource => linkedSource.Model.ContentContextualization,
                    linkedSource => linkedSource.Content,
                    reference => reference.ContentType,
                    includes => includes
                        .When<PolymorphicNestedLinkedSourcesTests.PersonWithoutContextualizationLinkedSource, string>(
                            "person",
                            reference => (string)reference.Id)
                        .When<PolymorphicNestedLinkedSourcesTests.ImageWithContextualizationLinkedSource, string>(
                            "image",
                            reference => (string)reference.Id,
                            (reference, childLinkedSource) => childLinkedSource.ContentContextualization = reference)
                );

            _fakeReferenceLoader =
                new FakeReferenceLoader<WithNestedPolymorphicContent, string>(reference => reference.Id);
            _sut = loadLinkProtocolBuilder.Build(_fakeReferenceLoader);
        }

        [Test]
        public void LoadLink_NestedPolymorphicContent() {
            _fakeReferenceLoader.FixValue(
                new WithNestedPolymorphicContent {
                    Id = "1",
                    ContentContextualization = new PolymorphicNestedLinkedSourcesTests.ContentContextualization{
                        ContentType = "person",
                        Id = "p1",
                        Title = "altered person title"
                    }
                }
            );

            var actual = _sut.LoadLink<WithNestedPolymorphicContentLinkedSource>("1");

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void LoadLink_NestedPolymorphicContentWithContextualization_ShouldInitContextualization() {
            _fakeReferenceLoader.FixValue(
                new WithNestedPolymorphicContent {
                    Id = "1",
                    ContentContextualization = new PolymorphicNestedLinkedSourcesTests.ContentContextualization {
                        ContentType = "image",
                        Id = "i1",
                        Title = "altered image title"
                    }
                }
            );

            var actual = _sut.LoadLink<WithNestedPolymorphicContentLinkedSource>("1");

            var contentAsImage =
                actual.Content as PolymorphicNestedLinkedSourcesTests.ImageWithContextualizationLinkedSource;
            Assert.That(contentAsImage.ContentContextualization.Title, Is.EqualTo("altered image title"));
        }


        public class WithNestedPolymorphicContentLinkedSource : ILinkedSource<WithNestedPolymorphicContent> {
            public WithNestedPolymorphicContent Model { get; set; }
            public PolymorphicNestedLinkedSourcesTests.INestedPolymorphicContentLinkedSource Content { get; set; }
        }

        public class WithNestedPolymorphicContent {
            public string Id { get; set; }
            public PolymorphicNestedLinkedSourcesTests.ContentContextualization ContentContextualization { get; set; }
        }
    }
}
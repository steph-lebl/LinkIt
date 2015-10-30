﻿using System.Collections.Generic;
using ApprovalTests.Reporters;
using HeterogeneousDataSources.Tests.Shared;
using NUnit.Framework;
using RC.Testing;

namespace HeterogeneousDataSources.Tests.Polymorphic {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class PolymorphicMixtedTests {
        private FakeReferenceLoader<Model, string> _fakeReferenceLoader;
        private LoadLinkProtocol _sut;

        [SetUp]
        public void SetUp() {
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();

            loadLinkProtocolBuilder.For<LinkedSource>()
                .PolymorphicLoadLink(
                    linkedSource => linkedSource.Model.TargetReference,
                    linkedSource => linkedSource.Target,
                    link => link.GetType(),
                    includes => includes
                        .Include<Person>().AsReference(
                            typeof(int),
                            link=>link.ToString()
                        )
                        .Include<PersonLinkedSource>().AsNestedLinkedSource(
                            typeof(string),
                            link => link.ToString()
                        )
                        .Include<PersonLinkedSource>().AsSubLinkedSource(
                            typeof(Person),
                            link => (Person)link
                        )
                );
            loadLinkProtocolBuilder.For<PersonLinkedSource>()
                .LoadLinkReference(
                    linkedSource => linkedSource.Model.SummaryImageId,
                    linkedSource => linkedSource.SummaryImage
                );

            _fakeReferenceLoader =
                new FakeReferenceLoader<Model, string>(reference => reference.Id);
            _sut = loadLinkProtocolBuilder.Build(_fakeReferenceLoader);
        }

        [Test]
        public void LoadLink_MixedPolymorphicAsReference() {
            _fakeReferenceLoader.FixValue(
                new Model {
                    Id = "1",
                    TargetReference = 1
                }
            );

            var actual = _sut.LoadLink<LinkedSource>().ById("1");

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void LoadLink_MixedPolymorphicAsNestedLinkedSource() {
            _fakeReferenceLoader.FixValue(
                new Model {
                    Id = "1",
                    TargetReference = "nested"
                }
            );

            var actual = _sut.LoadLink<LinkedSource>().ById("1");

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void LoadLink_MixedPolymorphicAsSubLinkedSource() {
            _fakeReferenceLoader.FixValue(
                new Model {
                    Id = "1",
                    TargetReference = new Person{
                        Id = "as-sub-linked-source",
                        Name = "The Name",
                        SummaryImageId = "the-id"
                    }
                }
            );

            var actual = _sut.LoadLink<LinkedSource>().ById("1");

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        public class LinkedSource : ILinkedSource<Model> {
            public Model Model { get; set; }
            public object Target { get; set; }
        }

        public class Model{
            public string Id { get; set; }
            public object TargetReference { get; set; }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using ApprovalTests.Reporters;
using HeterogeneousDataSource.Conventions.DefaultConventions;
using HeterogeneousDataSource.Conventions.Interfaces;
using HeterogeneousDataSources;
using HeterogeneousDataSources.ConfigBuilders;
using HeterogeneousDataSources.LinkedSources;
using HeterogeneousDataSources.Tests;
using HeterogeneousDataSources.Tests.Shared;
using NUnit.Framework;
using RC.Testing;

namespace HeterogeneousDataSource.Conventions.Tests.DefaultConventions
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class LoadLinkMultiValueSubLinkedSourceWhenNameMatchesTests {
        [Test]
        public void GetLinkedSourceTypes(){
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
            
            loadLinkProtocolBuilder.ApplyConventions(
                new List<Type> { typeof(LinkedSource) },
                new List<ILoadLinkExpressionConvention> { new LoadLinkMultiValueSubLinkedSourceWhenNameMatches() }
            );

            var fakeReferenceLoader =
                new FakeReferenceLoader<Model, string>(reference => reference.Id);
            var sut = loadLinkProtocolBuilder.Build(fakeReferenceLoader);

            var actual = sut.LoadLink<LinkedSource>().FromModel(
                new Model{
                    Id="One",
                    ListOfMedia = new List<Media>{
                        new Media {
                            Id = 1
                        },
                        new Media {
                            Id = 2
                        }
                    }
                }
            );

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        public class LinkedSource : ILinkedSource<Model> {
            public Model Model { get; set; }
            public List<MediaLinkedSource> ListOfMedia { get; set; }
        }

        public class Model{
            public string Id { get; set; }
            public List<Media> ListOfMedia { get; set; }
        }
    }
}

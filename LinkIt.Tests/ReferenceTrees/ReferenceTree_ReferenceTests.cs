﻿#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using ApprovalTests.Reporters;
using LinkIt.ConfigBuilders;
using LinkIt.Core;
using LinkIt.PublicApi;
using LinkIt.PublicApi;
using LinkIt.ReferenceTrees;
using LinkIt.Tests.TestHelpers;
using NUnit.Framework;


namespace LinkIt.Tests.ReferenceTrees {
    public class ReferenceTree_ReferenceTests
    {
        private LoadLinkProtocol _sut;

        [SetUp]
        public void SetUp()
        {
            var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
            loadLinkProtocolBuilder.For<LinkedSource>()
                .LoadLinkReferenceById(
                    linkedSource => linkedSource.Model.PersonOneId,
                    linkedSource => linkedSource.PersonOne
                )
                .LoadLinkReferenceById(
                    linkedSource => linkedSource.Model.PersonTwoId,
                    linkedSource => linkedSource.PersonTwo
                );
            _sut = (LoadLinkProtocol)loadLinkProtocolBuilder.Build(() => new ReferenceLoaderStub());
        }

        [Fact]
        public void CreateRootReferenceTree() {
            var actual = _sut.CreateRootReferenceTree(typeof(LinkedSource));

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Fact]
        public void ParseLoadingLevels() {
            var rootReferenceTree = _sut.CreateRootReferenceTree(typeof(LinkedSource));

            var actual = rootReferenceTree.ParseLoadingLevels();

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        public class LinkedSource : ILinkedSource<Model> {
            public Model Model { get; set; }
            public Person PersonOne { get; set; }
            public Person PersonTwo { get; set; }
        }

        public class Model {
            public int Id { get; set; }
            public string PersonOneId { get; set; }
            public string PersonTwoId { get; set; }
        }
    }
}

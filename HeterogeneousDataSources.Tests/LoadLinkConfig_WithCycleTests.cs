﻿using System;
using System.Collections.Generic;
using ApprovalTests.Reporters;
using HeterogeneousDataSources.LoadLinkExpressions;
using HeterogeneousDataSources.Tests.Shared;
using NUnit.Framework;
using RC.Testing;

namespace HeterogeneousDataSources.Tests {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class LoadLinkConfig_WithCycleTests
    {
        //[Test]
        //public void CreateLoadLinkConfig_WithCycleCausedByReference_ShouldThrow()
        //{
        //    var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
        //    loadLinkProtocolBuilder.For<CycleInReferenceLinkedSource>()
        //        .IsRoot<string>()
        //        .LoadLinkReference(
        //            linkedSource => linkedSource.Model.ParentId,
        //            linkedSource => linkedSource.Parent
        //        );

        //    TestDelegate act = () => new LoadLinkConfig(loadLinkProtocolBuilder.GetLoadLinkExpressions());

        //    Assert.That(
        //        act,
        //        Throws.ArgumentException.With
        //            .Message.ContainsSubstring("cycle").And
        //            .Message.ContainsSubstring("DirectCycle")
        //        );
        //}

        //[Test]
        //public void CreateLoadLinkConfig_WithCycleCausedByDirectNestedLinkedSource_ShouldThrow() {
        //    var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
        //    loadLinkProtocolBuilder.For<DirectCycleInNestedLinkedSource>()
        //        .LoadLinkNestedLinkedSource(
        //            linkedSource => linkedSource.Model.ParentId,
        //            linkedSource => linkedSource.Parent
        //        );

        //    var fakeReferenceLoader =
        //        new FakeReferenceLoader<DirectCycle, string>(reference => reference.Id);
        //    var sut = loadLinkProtocolBuilder.Build(fakeReferenceLoader);

        //    fakeReferenceLoader.FixValue(
        //        new DirectCycle{
        //            Id = "1",
        //            ParentId = "2"
        //        }
        //    );

        //    try
        //    {
        //        var actual = sut.LoadLink<DirectCycleInNestedLinkedSource>().ById("1");
        //        ApprovalsExt.VerifyPublicProperties(actual);
        //    }
        //    catch (Exception ex)
        //    {
                
        //    }

            
        //}

        //[Test]
        //public void CreateLoadLinkConfig_WithCycleCausedByIndirectNestedLinkedSource_ShouldThrow() {
        //    var loadLinkProtocolBuilder = new LoadLinkProtocolBuilder();
        //    loadLinkProtocolBuilder.For<IndirectCycleLevel0LinkedSource>()
        //        .IsRoot<string>()
        //        .LoadLinkNestedLinkedSource(
        //            linkedSource => linkedSource.Model.Level1Id,
        //            linkedSource => linkedSource.Level1
        //        );
        //    loadLinkProtocolBuilder.For<IndirectCycleLevel1LinkedSource>()
        //        .LoadLinkNestedLinkedSource(
        //            linkedSource => linkedSource.Model.Level0Id,
        //            linkedSource => linkedSource.Level0
        //        );

        //    TestDelegate act = () => new LoadLinkConfig(loadLinkProtocolBuilder.GetLoadLinkExpressions());

        //    Assert.That(
        //        act,
        //        Throws.ArgumentException.With
        //            .Message.ContainsSubstring("cycle").And
        //            .Message.ContainsSubstring("IndirectCycleLevel0")
        //        );
        //}

        public class CycleInReferenceLinkedSource: ILinkedSource<DirectCycle>
        {
            public DirectCycle Model { get; set; }
            public DirectCycle Parent { get; set; }
        }

        public class DirectCycleInNestedLinkedSource : ILinkedSource<DirectCycle> {
            public DirectCycle Model { get; set; }
            public DirectCycleInNestedLinkedSource Parent { get; set; }
        }

        public class DirectCycle {
            public string Id { get; set; }
            public string ParentId { get; set; }
        }

        public class IndirectCycleLevel0LinkedSource : ILinkedSource<IndirectCycleLevel0> {
            public IndirectCycleLevel0 Model { get; set; }
            public IndirectCycleLevel1LinkedSource Level1 { get; set; }
        }

        public class IndirectCycleLevel1LinkedSource : ILinkedSource<IndirectCycleLevel1> {
            public IndirectCycleLevel1 Model { get; set; }
            public IndirectCycleLevel0LinkedSource Level0 { get; set; }
        }

        public class IndirectCycleLevel0 {
            public string Id { get; set; }
            public string Level1Id { get; set; }
        }

        public class IndirectCycleLevel1 {
            public string Id { get; set; }
            public string Level0Id { get; set; }
        }
    }
}

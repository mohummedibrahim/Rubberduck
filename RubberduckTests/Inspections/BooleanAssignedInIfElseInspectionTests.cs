﻿using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rubberduck.Inspections.Concrete;
using RubberduckTests.Mocks;

namespace RubberduckTests.Inspections
{
    [TestClass]
    public class BooleanAssignedInIfElseInspectionTests
    {
        [TestMethod]
        [TestCategory("Inspections")]
        public void Simple()
        {
            const string inputcode =
@"Sub Foo()
    Dim d As Boolean
    If True Then
        d = True
    Else
        d = False
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void MultipleResults()
        {
            const string inputcode =
@"Sub Foo()
    Dim d As Boolean
    If True Then
        d = True
    Else
        d = False
    EndIf

    If True Then
        d = False
    Else
        d = True
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void AssignsInteger()
        {
            const string inputcode =
@"Sub Foo()
    Dim d
    If True Then
        d = 0
    Else
        d = 1
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void AssignsTheSameValue()       // worthy of an inspection in its own right
        {
            const string inputcode =
@"Sub Foo()
    Dim d
    If True Then
        d = True
    Else
        d = True
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void AssignsToDifferentVariables()
        {
            const string inputcode =
@"Sub Foo()
    Dim d1, d2
    If True Then
        d1 = True
    Else
        d2 = False
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void ConditionalContainsElseIfBlock()
        {
            const string inputcode =
@"Sub Foo()
    Dim d
    If True Then
        d = True
    ElseIf False Then
        d = True
    Else
        d = False
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void ConditionalDoesNotContainElseBlock()
        {
            const string inputcode =
@"Sub Foo()
    Dim d
    If True Then
        d = True
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void IsIgnored()
        {
            const string inputcode =
                @"Sub Foo()
    Dim d
    '@Ignore BooleanAssignedInIfElse
    If True Then
        d = True
    Else
        d = False
    EndIf
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out _);
            var state = MockParser.CreateAndParse(vbe.Object);

            var inspection = new BooleanAssignedInIfElseInspection(state);
            var inspector = InspectionsHelper.GetInspector(inspection);
            var results = inspector.FindIssuesAsync(state, CancellationToken.None).Result;

            Assert.IsFalse(results.Any());
        }
    }
}
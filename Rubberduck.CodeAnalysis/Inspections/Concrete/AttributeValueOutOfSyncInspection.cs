﻿using System.Collections.Generic;
using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Inspections.Results;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Annotations;
using Rubberduck.Parsing.Inspections;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Resources.Inspections;

namespace Rubberduck.Inspections.Concrete
{
    [CannotAnnotate]
    public sealed class AttributeValueOutOfSyncInspection : InspectionBase
    {
        public AttributeValueOutOfSyncInspection(RubberduckParserState state) 
        :base(state)
        {
        }

        protected override IEnumerable<IInspectionResult> DoGetInspectionResults()
        {
            var declarationsWithAttributeAnnotations = State.DeclarationFinder.AllUserDeclarations
                .Where(declaration => declaration.Annotations.Any(annotation => annotation.AnnotationType.HasFlag(AnnotationType.Attribute)));
            var results = new List<DeclarationInspectionResult>();
            foreach (var declaration in declarationsWithAttributeAnnotations)
            {
                foreach (var annotation in declaration.Annotations.OfType<IAttributeAnnotation>())
                {
                    if (HasDifferingAttributeValues(declaration, annotation, out var attributeValues))
                    {
                        var description = string.Format(InspectionResults.AttributeValueOutOfSyncInspection, 
                            annotation.Attribute, 
                            string.Join(", ", attributeValues), 
                            annotation.AnnotationType);

                        var result = new DeclarationInspectionResult(this, description, declaration,
                            new QualifiedContext(declaration.QualifiedModuleName, annotation.Context));
                        result.Properties.Annotation = annotation;
                        result.Properties.AttributeValues = attributeValues;

                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private static bool HasDifferingAttributeValues(Declaration declaration, IAttributeAnnotation annotation, out IReadOnlyList<string> attributeValues)
        {
            var attributeNodes = declaration.DeclarationType.HasFlag(DeclarationType.Module)
                                    ? declaration.Attributes.AttributeNodesFor(annotation).ToList()
                                    : declaration.Attributes.AttributeNodesFor(annotation, declaration.IdentifierName).ToList();

            foreach (var attributeNode in attributeNodes)
            {
                var values = attributeNode.Values;
                if (!annotation.AttributeValues.SequenceEqual(values))
                {
                    attributeValues = values;
                    return true;
                }
            }
            attributeValues = new List<string>();
            return false;
        }
    }
}
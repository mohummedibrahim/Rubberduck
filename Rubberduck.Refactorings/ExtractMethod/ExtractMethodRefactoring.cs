using System;
using Rubberduck.Parsing.Symbols;
using Rubberduck.SmartIndenter;
using Rubberduck.VBEditor;
using Rubberduck.Parsing;
using Rubberduck.Parsing.VBA;
using Rubberduck.VBEditor.Utility;
using Rubberduck.VBEditor.Extensions;
using Rubberduck.Refactorings.Exceptions;
using Rubberduck.VBEditor.ComManagement;
using System.Linq;
using System.Collections.Generic;

namespace Rubberduck.Refactorings.ExtractMethod
{
    /// <summary>
    /// A refactoring that extracts a method (procedure or function) 
    /// out of a selection in the active code pane and 
    /// replaces the selected code with a call to the extracted method.
    /// </summary>
    public class ExtractMethodRefactoring : InteractiveRefactoringBase<ExtractMethodModel>
    {
        private readonly IDeclarationFinderProvider _declarationFinderProvider;
        private readonly ISelectedDeclarationProvider _selectedDeclarationProvider;
        private readonly IRefactoringAction<ExtractMethodModel> _refactoringAction;
        private readonly ISelectionProvider _selectionProvider;
        private readonly IProjectsProvider _projectsProvider;
        private readonly IIndenter _indenter;
        public ExtractMethodSelectionValidation Validator { get; set; }

        public ExtractMethodRefactoring(
            ExtractMethodRefactoringAction refactoringAction,
            IDeclarationFinderProvider declarationFinderProvider,
            RefactoringUserInteraction<IExtractMethodPresenter, ExtractMethodModel> userInteraction,
            ISelectionProvider selectionProvider,
            ISelectedDeclarationProvider selectedDeclarationProvider,
            IProjectsProvider projectsProvider,
            IIndenter indenter)
        : base(selectionProvider, userInteraction)
        {
            _refactoringAction = refactoringAction;
            _declarationFinderProvider = declarationFinderProvider;
            _selectedDeclarationProvider = selectedDeclarationProvider;
            _selectionProvider = selectionProvider;
            _projectsProvider = projectsProvider;
            _indenter = indenter;
        }
        protected override ExtractMethodModel InitializeModel(Declaration target)
        {
            CheckWhetherValidTarget(target);
            Validator = new ExtractMethodSelectionValidation(_declarationFinderProvider.DeclarationFinder.AllUserDeclarations, _projectsProvider);
            if (!_selectionProvider.ActiveSelection().HasValue)
            {
                throw new TargetDeclarationIsNullException();
            }
            if (Validator.ValidateSelection(_selectionProvider.ActiveSelection().GetValueOrDefault()))
            {
                var model = new ExtractMethodModel(_declarationFinderProvider, Validator.SelectedContexts, _selectionProvider.ActiveSelection().GetValueOrDefault(), target, _indenter);
                model.ModuleContainsCompilationDirectives = Validator.ContainsCompilerDirectives;
                return model;
            }
            else
            {
                throw new InvalidTargetSelectionException(_selectionProvider.ActiveSelection().GetValueOrDefault());
            }
        }

        protected override void RefactorImpl(ExtractMethodModel model)
        {
            _refactoringAction.Refactor(model);
        }

        protected override Declaration FindTargetDeclaration(QualifiedSelection targetSelection)
        {
            return _selectedDeclarationProvider.SelectedDeclaration(targetSelection);
        }

        override public void Refactor()
        {
            //if (!_codeModule.GetQualifiedSelection().HasValue)
            //{
            //    OnInvalidSelection();
            //    return;
            //}

            //selection = _codeModule.GetQualifiedSelection().Value;
            
            //var model = new ExtractMethodModel(_state, selection, Validator.SelectedContexts, _indenter, _codeModule);
            ////var presenter = new ExtractMethodPresenter(view, _indenter);
            //ExtractMethodPresenter presenter = null;
            //if (presenter == null)
            //{
            //    return;
            //}

            ////model = presenter.Show(model, extractMethodProc); //TODO - restore user interface
            //model = null;
            //if (model == null)
            //{
            //    return;
            //}

            //QualifiedSelection? oldSelection;
            //if (!_codeModule.IsWrappingNullReference)
            //{
            //    oldSelection = _codeModule.GetQualifiedSelection();
            //}
            //else
            //{
            //    return;
            //}

            //if (oldSelection.HasValue)
            //{
            //    _codeModule.CodePane.Selection = oldSelection.Value.Selection;
            //}

            //model.State.OnParseRequested(this);
        }

        public override void Refactor(QualifiedSelection target)
        {
            var _declarations = _declarationFinderProvider.DeclarationFinder.AllUserDeclarations;
            var selection = target.Selection;
            var procedures = _declarations.Where(d => d.ComponentName == target.QualifiedName.ComponentName && d.IsUserDefined && (ExtractMethodSelectionValidation.ProcedureTypes.Contains(d.DeclarationType)));
            var declarations = procedures as IList<Declaration> ?? procedures.ToList();
            Declaration ProcOfLine(int sl) => declarations.FirstOrDefault(d => d.Context.Start.Line < sl && d.Context.Stop.EndLine() > sl);
            var targetProc = ProcOfLine(selection.StartLine);
            Refactor(InitializeModel(targetProc));
        }

        public override void Refactor(Declaration target)
        {
            OnInvalidSelection();
        }
        private void CheckWhetherValidTarget(Declaration target)
        {
            if (target == null)
            {
                throw new TargetDeclarationIsNullException();
            }

            if (!target.IsUserDefined)
            {
                throw new TargetDeclarationNotUserDefinedException(target);
            }
        }
        private void ExtractMethod()
        {

            #region to be put back when allow subs and functions
            /* Remove this entirely for now.
            // assumes these are declared *before* the selection...
            var offset = 0;
            foreach (var declaration in model.DeclarationsToMove.OrderBy(e => e.Selection.StartLine))
            {
                var target = new Selection(
                    declaration.Selection.StartLine - offset,
                    declaration.Selection.StartColumn,
                    declaration.Selection.EndLine - offset,
                    declaration.Selection.EndColumn);

                _codeModule.DeleteLines(target);
                offset += declaration.Selection.LineCount;
            }
            */
            #endregion

        }


        /// <summary>
        /// An event that is raised when refactoring is not possible due to an invalid selection.
        /// </summary>
        public event EventHandler InvalidSelection;
        private void OnInvalidSelection()
        {
            var handler = InvalidSelection;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

    }
}

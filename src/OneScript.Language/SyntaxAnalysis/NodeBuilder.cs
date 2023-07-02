﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    /// <summary>
    /// helper for creating ast nodes of different types
    /// </summary>
    internal static class NodeBuilder
    {
        public static BslSyntaxNode CreateNode(NodeKind kind, in Lexem startLexem)
        {
            switch (kind)
            {
                case NodeKind.Identifier:
                case NodeKind.Constant:
                case NodeKind.ExportFlag:
                case NodeKind.ByValModifier:
                case NodeKind.AnnotationParameterName:
                case NodeKind.AnnotationParameterValue:
                case NodeKind.ParameterDefaultValue:
                case NodeKind.ForEachVariable:
                case NodeKind.Unknown:
                    return new TerminalNode(kind, startLexem);
                case NodeKind.BlockEnd:
                case NodeKind.ContinueStatement:
                case NodeKind.BreakStatement:
                    return new LineMarkerNode(startLexem.Location, kind);
                default:
                    var node = MakeNonTerminal(kind, startLexem);
                    return node;
            }
        }

        private static NonTerminalNode MakeNonTerminal(NodeKind kind, in Lexem startLexem)
        {
            switch (kind)
            {
                case NodeKind.Annotation:
                case NodeKind.Import:
                    return new AnnotationNode(kind, startLexem);
                case NodeKind.AnnotationParameter:
                    return new AnnotationParameterNode();
                case NodeKind.VariableDefinition:
                    return new VariableDefinitionNode(startLexem);
                case NodeKind.Method:
                    return new MethodNode();
                case NodeKind.MethodSignature:
                    return new MethodSignatureNode(startLexem);
                case NodeKind.MethodParameter:
                    return new MethodParameterNode();
                case NodeKind.BinaryOperation:
                    return new BinaryOperationNode(startLexem);
                case NodeKind.UnaryOperation:
                    return new UnaryOperationNode(startLexem);
                case NodeKind.WhileLoop:
                    return new WhileLoopNode(startLexem);
                case NodeKind.Condition:
                    return new ConditionNode(startLexem);
                case NodeKind.CodeBatch:
                    return new CodeBatchNode(startLexem);
                case NodeKind.ForEachLoop:
                    return new ForEachLoopNode(startLexem);
                case NodeKind.ForLoop:
                    return new ForLoopNode(startLexem);
                case NodeKind.TryExcept:
                    return new TryExceptNode(startLexem);
                case NodeKind.NewObject:
                    return new NewObjectNode(startLexem);
                case NodeKind.Preprocessor:
                    return new PreprocessorDirectiveNode(startLexem);
                case NodeKind.GlobalCall:
                case NodeKind.MethodCall:
                    return new CallNode(kind, startLexem);
                case NodeKind.Module:
                    throw new InvalidOperationException();
                default:
                    return new NonTerminalNode(kind, startLexem);
            }
        }
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace scanner
{
    public class RoslynEditHelper
    {
        static private string wpfPath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2";
        private MetadataReference[] references;
        public RoslynEditHelper()
        {
            MetadataReference mscorlib =
                    MetadataReference.CreateFromFile(Path.Combine(wpfPath, "mscorlib.dll"));
            MetadataReference pc =
                    MetadataReference.CreateFromFile(Path.Combine(wpfPath, "PresentationCore.dll"));

            MetadataReference wpf =
                    MetadataReference.CreateFromFile(Path.Combine(wpfPath, "PresentationFramework.dll"));

            MetadataReference dp =
                    MetadataReference.CreateFromFile(Path.Combine(wpfPath, "WindowsBase.dll"));
            
            references = new MetadataReference[] { mscorlib, pc, wpf, dp };
        }
        public Dictionary<string, string> EditAndGetInfo(StmtNode node, string apiName, Dictionary<string, string> overridePairs)
        {
            SemanticModel model = CreateModelForWhole(node);
            var sourceTree = model.SyntaxTree;

            SyntaxNode realNode = FindRealNode(node, sourceTree.GetCompilationUnitRoot());
            InvocationExpressionSyntax invocation = FindInvocationNode(apiName, realNode);
            SeparatedSyntaxList<ArgumentSyntax> arguments;
            Dictionary<string, string> list = new Dictionary<string, string>();
            Dictionary<string, ExpressionSyntax> nodeList = new Dictionary<string, ExpressionSyntax>();


            // Not invocation but just member access
            if (invocation == null)
            {
                MemberAccessExpressionSyntax memberAccess = FindMemberAccessNode(apiName, realNode);

                if (memberAccess == null)
                {
                    ObjectCreationExpressionSyntax objectCreation = FindObjectCreationNode(apiName, realNode);
                    arguments = objectCreation.ArgumentList.Arguments;

                    foreach (var pair in overridePairs)
                    {
                        string oldCode = pair.Key;
                        string newCode = pair.Value;

                        SemanticModel tModel = CreateModel(oldCode);
                        ObjectCreationExpressionSyntax tNode = FindObjectCreationNode(apiName, tModel.SyntaxTree.GetCompilationUnitRoot());
                        ISymbol tSymbol = tModel.GetSymbolInfo(tNode).Symbol;
                        SymbolInfo s1 = model.GetSymbolInfo(objectCreation);
                        SymbolInfo s2 = tModel.GetSymbolInfo(tNode);
                        if (model.GetSymbolInfo(objectCreation).Symbol.ToString() == tSymbol.ToString())
                        {
                            var argstemplate = ((IMethodSymbol)tSymbol).Parameters.ToList();
                            for (int k = 0; k < arguments.Count; k++)
                            {
                                list.Add(argstemplate[k].Name, arguments[k].Expression.ToString());
                                nodeList.Add(argstemplate[k].Name, arguments[k].Expression);

                                if (arguments[k].Expression is ObjectCreationExpressionSyntax)
                                {
                                    if (argstemplate[k].Type.ToString() == "System.Delegate")
                                    {
                                        nodeList.Add($"D-{argstemplate[k].Name}", ((ObjectCreationExpressionSyntax)arguments[k].Expression).ArgumentList.Arguments.Single().Expression);
                                        list.Add($"D-{argstemplate[k].Name}", ((ObjectCreationExpressionSyntax)arguments[k].Expression).ArgumentList.Arguments.Single().Expression.ToString());
                                    }
                                    else
                                    {
                                        ObjectCreationExpressionSyntax tmp = (ObjectCreationExpressionSyntax)arguments[k].Expression;
                                        SymbolInfo tmpInfo = model.GetSymbolInfo(tmp);
                                        var tmptemplate = ((IMethodSymbol)tmpInfo.Symbol).Parameters.ToList();
                                        for (int l = 0; l < tmptemplate.Count; l++)
                                        {
                                            list.Add(tmptemplate[l].Name, tmp.ArgumentList.Arguments[l].Expression.ToString());
                                            nodeList.Add(tmptemplate[l].Name, tmp.ArgumentList.Arguments[l].Expression);
                                        }
                                    }

                                }
                            }

                            Regex regex = new Regex($"new\\s+(?<newApiName>[\\w_]+)\\((?<argList>.*)\\)");
                            Match match = regex.Match(newCode);
                            Trace.Assert(match.Success);

                            List<ArgumentSyntax> argList = new List<ArgumentSyntax>();
                            foreach (string arg in match.Groups["argList"].Value.Split(','))
                            {
                                if (nodeList.ContainsKey(arg.Trim()))
                                {
                                    argList.Add(Argument(nodeList[arg.Trim()]).WithoutTrivia());
                                }
                            }

                            ObjectCreationExpressionSyntax newNode = objectCreation.WithArgumentList(ArgumentList().AddArguments(argList.ToArray()));
                            realNode = realNode.ReplaceNode(objectCreation, newNode);
                            node.text = realNode.ToString();

                            break;
                        }
                    }
                    return list;

                } else
                {
                    foreach (var pair in overridePairs)
                    {
                        string oldCode = pair.Key;
                        string newCode = pair.Value;

                        SemanticModel tModel = CreateModel(oldCode);
                        MemberAccessExpressionSyntax tNode = FindMemberAccessNode(apiName, tModel.SyntaxTree.GetCompilationUnitRoot());

                        bool IsMatch = true;
                        ExpressionSyntax target = memberAccess, pattern = tNode;
                        while (target is MemberAccessExpressionSyntax && pattern is MemberAccessExpressionSyntax)
                        {
                            if (model.GetSymbolInfo(target).Symbol.ToString() != tModel.GetSymbolInfo(pattern).Symbol.ToString())
                            {
                                IsMatch = false;
                                break;
                            }
                            target = ((MemberAccessExpressionSyntax)target).Expression;
                            pattern = ((MemberAccessExpressionSyntax)pattern).Expression;
                        }
                        string s1 = model.GetTypeInfo(target).Type.ToString();
                        string s2 = tModel.GetTypeInfo(pattern).Type.ToString();

                        if (model.GetTypeInfo(target).Type.ToString() != tModel.GetTypeInfo(pattern).Type.ToString())
                        {
                            //IsMatch = false;
                        }

                        if (IsMatch)
                        {
                            string[] newCodeApis = newCode.Split('.');
                            ExpressionSyntax newNode;
                            if (pattern is MemberAccessExpressionSyntax)
                            {
                                newNode = IdentifierName(newCodeApis[0]);
                            }
                            else
                            {
                                newNode = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, memberAccess.OperatorToken, IdentifierName(newCodeApis[0]));
                            }

                            for (int i = 1; i < newCodeApis.Length; i++)
                            {
                                newNode = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, newNode, memberAccess.OperatorToken, IdentifierName(newCodeApis[i]));
                            }
                            realNode = realNode.ReplaceNode(memberAccess, newNode.WithTriviaFrom(memberAccess));
                            node.text = realNode.ToString();
                        }
                    }
                    return null;
                }
            }

            arguments = invocation.ArgumentList.Arguments;
            foreach (var pair in overridePairs)
            {
                string oldCode = pair.Key;
                string newCode = pair.Value;

                SemanticModel tModel = CreateModel(oldCode);
                InvocationExpressionSyntax tNode = FindInvocationNode(apiName, tModel.SyntaxTree.GetCompilationUnitRoot());
                ISymbol tSymbol = tModel.GetSymbolInfo(tNode).Symbol;
                SymbolInfo s1 = model.GetSymbolInfo(invocation);
                SymbolInfo s2 = tModel.GetSymbolInfo(tNode);
                if (model.GetSymbolInfo(invocation).Symbol.ToString() == tSymbol.ToString())
                {
                    var argstemplate = ((IMethodSymbol)tSymbol).Parameters.ToList();
                    for (int k = 0; k < arguments.Count; k++)
                    {
                        list.Add(argstemplate[k].Name, arguments[k].Expression.ToString());
                        nodeList.Add(argstemplate[k].Name, arguments[k].Expression);
                        if (argstemplate[k].Type.ToString() == "System.Type")
                        {
                            list.Add($"T-{argstemplate[k].Name}", node.getTypeFromContext(list[argstemplate[k].Name]));
                        }
                       

                        if (arguments[k].Expression is ObjectCreationExpressionSyntax)
                        {
                            if (argstemplate[k].Type.ToString() == "System.Delegate")
                            {
                                nodeList.Add($"D-{argstemplate[k].Name}", ((ObjectCreationExpressionSyntax)arguments[k].Expression).ArgumentList.Arguments.Single().Expression);
                                list.Add($"D-{argstemplate[k].Name}", ((ObjectCreationExpressionSyntax)arguments[k].Expression).ArgumentList.Arguments.Single().Expression.ToString());
                            } else
                            {
                                ObjectCreationExpressionSyntax tmp = (ObjectCreationExpressionSyntax)arguments[k].Expression;
                                SymbolInfo tmpInfo = model.GetSymbolInfo(tmp);
                                var tmptemplate = ((IMethodSymbol)tmpInfo.Symbol).Parameters.ToList();
                                for (int l = 0; l < tmptemplate.Count; l++)
                                {
                                    list.Add(tmptemplate[l].Name, tmp.ArgumentList.Arguments[l].Expression.ToString());
                                    nodeList.Add(tmptemplate[l].Name, tmp.ArgumentList.Arguments[l].Expression);
                                }
                            }
                            
                        }
                    }

                    Regex regex = new Regex($"(?<newApiName>[\\w_]+)(\\<(?<typeList>.*)\\>)?\\((?<argList>.*)\\)");
                    Match match = regex.Match(newCode);
                    Trace.Assert(match.Success);
                    
                    string newApiName = match.Groups["newApiName"].Value.Trim();
                    SimpleNameSyntax memberName;
                    var v = match.Groups["typeList"];
                    if (match.Groups["typeList"].Success)
                    {
                        List<TypeSyntax> typeList = new List<TypeSyntax>();
                        foreach (string type in match.Groups["typeList"].Value.Split(','))
                        {
                            string key = $"T-{type.Trim()}";
                            typeList.Add(IdentifierName(list[key]));
                        }
                        memberName = GenericName(Identifier(newApiName), TypeArgumentList().AddArguments(typeList.ToArray()));
                    } else
                    {
                        memberName = IdentifierName(newApiName);
                    }

                    List<ArgumentSyntax> argList = new List<ArgumentSyntax>();
                    foreach (string arg in match.Groups["argList"].Value.Split(','))
                    {
                        if (nodeList.ContainsKey(arg.Trim()))
                        {
                            argList.Add(Argument(nodeList[arg.Trim()]).WithoutTrivia());
                        }
                    }

                    InvocationExpressionSyntax newNode;
                    if (invocation.Expression is MemberAccessExpressionSyntax)
                    {
                        var memberAccessExp = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ((MemberAccessExpressionSyntax)invocation.Expression).Expression, memberName);
                        newNode = InvocationExpression(memberAccessExp, ArgumentList().AddArguments(argList.ToArray()));
                    } else
                    {
                        newNode = InvocationExpression(memberName, ArgumentList().AddArguments(argList.ToArray()));
                    }

                    realNode = realNode.ReplaceNode(invocation, newNode);
                    node.text = realNode.ToString();
                    
                    break;
                }
            }
            return list;
        }

        private SyntaxNode FindRealNode(StmtNode stmtNode, SyntaxNode syntaxNode) 
        {
            StmtNode parent = stmtNode.parent;
            if (parent == null) return syntaxNode;
            SyntaxNode syntaxParent = FindRealNode(parent, syntaxNode);

            if (syntaxParent is NamespaceDeclarationSyntax)
            {
                return ((NamespaceDeclarationSyntax)syntaxParent).Members.ElementAt(stmtNode.GetIndex());
            } else if (parent.type == StmtType.ClassDeclaration && syntaxParent is ClassDeclarationSyntax)
            {
                return ((ClassDeclarationSyntax)syntaxParent).Members.ElementAt(stmtNode.GetIndex());
                //return syntaxParent.ChildNodes().ElementAt(stmtNode.GetIndex()+1);
            } else if (parent.type == StmtType.FunctionDeclaration)
            {
                var stmts = StatementsFlat(((BaseMethodDeclarationSyntax)syntaxParent).Body.Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is IfStatementSyntax)
            {
                var stmts = StatementsFlat(((BlockSyntax)((IfStatementSyntax)syntaxParent).Statement).Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is ElseClauseSyntax)
            {
                var stmts = StatementsFlat(((BlockSyntax)((ElseClauseSyntax)syntaxParent).Statement).Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is WhileStatementSyntax)
            {
                var stmts = StatementsFlat(((BlockSyntax)((WhileStatementSyntax)syntaxParent).Statement).Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is DoStatementSyntax)
            {
                var stmts = StatementsFlat(((BlockSyntax)((DoStatementSyntax)syntaxParent).Statement).Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is SwitchStatementSyntax)
            {
                return ((SwitchStatementSyntax)syntaxParent).Sections.ElementAt(stmtNode.GetIndex());
            } else if (syntaxParent is SwitchSectionSyntax)
            {
                var stmts = StatementsFlat(((SwitchSectionSyntax)syntaxParent).Statements);
                return stmts.ElementAt(stmtNode.GetIndex());
            } else
            {
                return syntaxParent.ChildNodes().ElementAt(stmtNode.GetIndex());
            }           
        }
        private MemberAccessExpressionSyntax FindMemberAccessNode(string apiName, SyntaxNode root)
        {
            string[] apis = apiName.Split('.');
            var invocations = from expression in root.DescendantNodes()
                              .OfType<MemberAccessExpressionSyntax>()
                              where expression.Name.Identifier.ValueText == apis[apis.Length-1]
                              select expression;
           
            foreach (var invocation in invocations)
            {
                int index = apis.Length - 2;
                var tmpNode = invocation;
                while (index >= 0)
                {
                    if (tmpNode.Expression is MemberAccessExpressionSyntax
                        &&((MemberAccessExpressionSyntax)tmpNode.Expression).Name.Identifier.ValueText == apis[index])
                    {
                        tmpNode = (MemberAccessExpressionSyntax)tmpNode.Expression;
                        index--;
                    } else if (index == 0 && tmpNode.Expression.ToString() == apis[index])
                    {
                        index--;
                    } else
                    {
                        break;
                    }
                }
                if (index < 0)
                {
                    return invocation;
                }
            }
            return null;
        }

        private ObjectCreationExpressionSyntax FindObjectCreationNode(string apiName, SyntaxNode root)
        {
            var creations = from objectCreationExpression in root.DescendantNodes()
                              .OfType<ObjectCreationExpressionSyntax>()
                              where objectCreationExpression.Type.ToString() == apiName
                              select objectCreationExpression;

            return creations.FirstOrDefault();
        }
        private InvocationExpressionSyntax FindInvocationNode(string apiName, SyntaxNode root)
        {
            var invocations = from invocationExpression in root.DescendantNodes()
                              .OfType<InvocationExpressionSyntax>()
                              where (invocationExpression.Expression is MemberAccessExpressionSyntax && ((MemberAccessExpressionSyntax)invocationExpression.Expression).Name.Identifier.ValueText == apiName)
                              || invocationExpression.Expression.ToString() == apiName
                              select invocationExpression;

            return invocations.FirstOrDefault();
        }

        private Compilation CreateCompilation(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            SyntaxTree[] sourceTrees = { tree };

            return CSharpCompilation.Create("TransformationCS",
                sourceTrees,
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
        private SemanticModel CreateModelForWhole(StmtNode node)
        {
            string code = node.getRootAncestor().ToString();
            var tree = CSharpSyntaxTree.ParseText(code);

            List<SyntaxTree> sourceTrees = node.GetOwnerTree().otherFileCodes.Select(
                (c) => CSharpSyntaxTree.ParseText(c)).ToList();
            sourceTrees.Insert(0, tree);

            Compilation compilation = CSharpCompilation.Create("TransformationCS",
                sourceTrees,
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var sourceTree = compilation.SyntaxTrees.First();
            var model = compilation.GetSemanticModel(sourceTree);

            return model;
        }
        private SemanticModel CreateModel(string code)
        {
            Compilation compilation = CreateCompilation(code);

            var sourceTree = compilation.SyntaxTrees.First();
            var model = compilation.GetSemanticModel(sourceTree);

            return model;
        }
        static private List<SyntaxNode> StatementsFlat(SyntaxList<StatementSyntax> statements)
        {
            List<SyntaxNode> newStatements = new List<SyntaxNode>();
            foreach (var statement in statements)
            {
                newStatements.Add(statement);
                if (statement is IfStatementSyntax)
                {
                    IfStatementSyntax ifstmt = (IfStatementSyntax)statement;
                    while (ifstmt?.Else?.Statement != null)
                    {
                        newStatements.Add(ifstmt.Else);
                        ifstmt = (IfStatementSyntax)ifstmt.Else.Statement;
                    }
                    if (ifstmt?.Else != null)
                    {
                        newStatements.Add(ifstmt.Else);
                    }
                }
            }
            return newStatements;
        }
    }
}

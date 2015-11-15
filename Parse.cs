using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangSharp;
using System.IO;

namespace mx.GenImpl
{
    class Parse
    {
        public static string m_file = "";
        public static string m_outcpp = "";
        public static string m_funcname = "";
        public static bool m_getSelected = false;
        public static CXCursor m_selected;

        public static List<Tuple<string, string>> m_sparams = new List<Tuple<string, string>>();
        public static CXChildVisitResult cursorVisitorMethodCollector(CXCursor cursor, CXCursor parent, IntPtr client_data)
        {
            var tup = new Tuple<string, string>(clang.getTypeSpelling(clang.getCursorType((cursor))).ToString(), clang.getCursorDisplayName(cursor).ToString());
            m_sparams.Add(tup);

            return CXChildVisitResult.CXChildVisit_Continue;
        }
        public static CXChildVisitResult cursorVisitor(CXCursor cursor, CXCursor parent, IntPtr client_data)
        {
            m_sparams.Clear();
            
            if (clang.getCursorKind(cursor) == CXCursorKind.CXCursor_CXXMethod ||
                clang.getCursorKind(cursor) == CXCursorKind.CXCursor_Constructor ||
                clang.getCursorKind(cursor) == CXCursorKind.CXCursor_Destructor)
            {

                if (m_getSelected && !m_selected.Equals(cursor))
                {
                    Console.WriteLine(clang.getCursorSpelling(m_selected).ToString());
                    return CXChildVisitResult.CXChildVisit_Recurse;
                }

                uint isDecl = clang.isCursorDefinition(cursor);
                if (isDecl == 0)
                {
                    CXClientData xx = new CXClientData();
                    var thisTU = clang.Cursor_getTranslationUnit(cursor);

                    var cloc = clang.getCursorLocation(cursor);
                    CXFile file;
                    uint line, col, offset;
                    clang.getFileLocation(cloc, out file, out line, out col, out offset);
                    string thisFile = clang.getFileName(file).ToString();

                    if (thisFile != m_file)
                    {
                        return CXChildVisitResult.CXChildVisit_Recurse;
                    }
                    clang.visitChildren(cursor, cursorVisitorMethodCollector, xx);

                    string methodName = clang.getCursorSpelling(cursor).ToString();
                    var classCursor = clang.getCursorSemanticParent(cursor);
                    string classType = clang.getCursorType(classCursor).ToString();
                    string retType = clang.getCursorResultType(cursor).ToString();

                    string methodSig = "";
                    if (clang.getCursorKind(cursor) == CXCursorKind.CXCursor_CXXMethod)
                    {
                        if (classType.Length > 0)
                            methodSig = retType + " " + classType + "::" + methodName + "(";
                        else
                            methodSig = retType + " " + methodName + "(";
                    }
                    else
                    {
                        if (classType.Length > 0)
                            methodSig = classType + "::" + methodName + "(";
                        else
                            methodSig = methodName + "(";
                    }
                    if (m_sparams.Count > 0)
                    {
                        for (int i = 1; i < m_sparams.Count - 1; i++)
                        {
                            methodSig += m_sparams[i].Item1 + " " + m_sparams[i].Item2 + ", ";
                        }
                        methodSig += m_sparams.Last().Item1 + " " + m_sparams.Last().Item2;
                    }
                    methodSig += ")";
                    m_outcpp += methodSig + "\n{\n\n}\n\n";
                }
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }
        public static string getCPP(string headerFilename)
        {
            var index = clang.createIndex(0, 0);
            CXUnsavedFile unsavedfile;
            m_file = headerFilename;
            m_outcpp = "#include <" + Path.GetFileName(m_file) + "> \n\n";
            string[] cargs = { "-x", "c++", "-std=c++11", "-isystem", Path.GetDirectoryName(m_file).ToString() };

            var TU = clang.parseTranslationUnit(index, m_file,
                cargs, cargs.Length, out unsavedfile, 0, (uint)CXTranslationUnit_Flags.CXTranslationUnit_None);

            m_getSelected = false;

            var cursor = clang.getTranslationUnitCursor(TU);
            CXClientData s = new CXClientData();

            clang.visitChildren(cursor, cursorVisitor, s);
            
            clang.disposeTranslationUnit(TU);
            clang.disposeIndex(index);
            return m_outcpp;
        }
        public static string getFunction(string headerFilename, uint line, uint col)
        {
            var index = clang.createIndex(0, 0);
            CXUnsavedFile unsavedfile;
            m_file = headerFilename;
            m_outcpp = "";
            string[] cargs = { "-x", "c++", "-std=c++11", "-isystem", Path.GetDirectoryName(m_file).ToString() };

            var TU = clang.parseTranslationUnit(index, m_file,
                cargs, cargs.Length, out unsavedfile, 0, (uint)CXTranslationUnit_Flags.CXTranslationUnit_None);

            var file = clang.getFile(TU, m_file);

            var sloc = clang.getLocation(TU, file, line, col);
            
            m_getSelected = true;

            m_selected = clang.getCursor(TU, sloc);
            
            var cursor = clang.getTranslationUnitCursor(TU);
            CXClientData s = new CXClientData();

            clang.visitChildren(cursor, cursorVisitor, s);

            Console.WriteLine(m_outcpp);

            clang.disposeTranslationUnit(TU);
            clang.disposeIndex(index);
            return m_outcpp;
        }
    }
}

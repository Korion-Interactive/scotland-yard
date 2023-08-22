using BitBarons.Editor.CodeGenerator.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace BitBarons.Editor.CodeGenerator
{
    
    public class CSharpFile
    {
        const string SHIFT = "    ";

        public CSharpCodeCollection Content { get; private set; }
        string filePath;

        public CSharpFile(string filePath)
        {
            this.filePath = filePath;
            Content = new CSharpCodeCollection();

            Reload();
        }

        public void Reload()
        {
            Content.Clear();

            // ensure directory
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using(FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    CSharpCodeCollection currentContent = Content;
                    Stack<CSharpBlock> blockHierachy = new Stack<CSharpBlock>();

                    string line = GetNextTrimmedLine(reader);
                    string nextLine = GetNextTrimmedLine(reader);
                    while (line != null)
                    {
                        if(nextLine == "{")         // START OF BLOCK
                        {
                            CSharpBlock block;
                            if(NameSpaceBlock.TryParseHeader(line, out block)
                            || DataTypeBlock.TryParseHeader(line, out block)
                            || MethodBlock.TryParseHeader(line, out block))
                            {
                                blockHierachy.Push(block);
                            }
                            else
                            {
                                block = new CSharpBlock();
                                blockHierachy.Push(block);
                            }
                            currentContent.Add(block);
                            currentContent = block.Content;
                        }
                        else if(line == "}") // END OF BLOCK
                        {
                            blockHierachy.Pop();

                            if (blockHierachy.Count > 0)
                            {
                                CSharpBlock block = blockHierachy.Peek();
                                currentContent = block.Content;
                            }
                            else
                            {
                                currentContent = this.Content;
                            }
                        }
                        else if (line != "{" && line != "}")
                        {
                            currentContent.Add(new Instruction(line));
                        }

                        line = nextLine;
                        nextLine = GetNextTrimmedLine(reader);
                    }

                    reader.Close();
                }
                stream.Close();
            }
        }

        private static string GetNextTrimmedLine(StreamReader reader)
        {
            string line = reader.ReadLine();
            if(line != null)
                line = line.Trim();

            return line;
        }

        public void Save(bool refreshAssetDatabase)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    int level = 0;
                    WriteContent(writer, Content, level);
                    
                    writer.Flush();

                    writer.Close();
                }
                stream.Close();
            }

            if(refreshAssetDatabase)
                Reload();
        }

        static void WriteContent(StreamWriter writer, CSharpCodeCollection content, int level)
        {
            string shifting = "";
            for (int i = 0; i < level; i++)
                shifting += SHIFT;

            foreach (TextContent tc in content)
            {
                switch (tc.ContentType)
                {
                    case ContentType.Instruction:

                        writer.WriteLine(shifting + (tc as Instruction));
                        break;

                    case ContentType.Block:

                        CSharpBlock block = (tc as CSharpBlock);

                        writer.WriteLine(shifting + block.GetCompleteHeader());
                        writer.WriteLine(shifting + "{");

                        WriteContent(writer, block.Content, level + 1);

                        writer.WriteLine(shifting + "}");
                        break;
                }
            }
        }
    }
}

using BitBarons.Editor.CodeGenerator.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator
{
    public class CSharpCodeCollection : IList<TextContent>
    {
        List<TextContent> list = new List<TextContent>();


        public TextContent AddIfNotExists(TextContent item)
        {
            var itm = list.FirstOrDefault((o) => o.Equals(item));
            if (itm != null)
            {
                return itm;
            }
            else
            {
                list.Add(item);
                return item;
            }

        }
        public CSharpBlock FindBlock(CSharpBlock block)
        {
            return FindBlock(block.GetCompleteHeader());
        }
        public CSharpBlock FindBlock(string header)
        {
            foreach(var c in list)
            {
                if(c.ContentType == ContentType.Block)
                {
                    var block = c as CSharpBlock;
                    if(block.GetCompleteHeader() == header)
                    {
                        return block;
                    }
                    else
                    {
                        CSharpBlock subBlock = block.Content.FindBlock(header);
                        if (subBlock != null)
                            return subBlock;
                    }
                }
            }

            return null;
        }

        #region IList members
        public int IndexOf(TextContent item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, TextContent item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public TextContent this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(TextContent item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            foreach (CSharpBlock block in list.Where((o) => o is CSharpBlock))
                block.Content.Clear();

            list.Clear();
        }

        public bool Contains(TextContent item)
        {
            return list.Contains(item);
        }

        public void CopyTo(TextContent[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TextContent item)
        {
            return list.Remove(item);
        }

        IEnumerator<TextContent> IEnumerable<TextContent>.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        #endregion
    }
}
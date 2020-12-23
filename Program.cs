using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace html_split
{
    class Program
    {
        static void Main(string[] args)
        {
            // From File
            var doc = new HtmlDocument();

            var book = @"C:\git\book\build\html\book.html";
            doc.Load(book);

            var nav = doc.DocumentNode.SelectSingleNode("//nav");
            var chapters = nav.SelectNodes("./ul/li");

            for (var i = 0; i < chapters.Count; ++i)
            {
                var chapter = chapters[i];
                var name = chapter.SelectSingleNode("./a").Attributes[0].Value.Substring(1);
                var file = Path.Combine(Path.GetDirectoryName(book), $"{name}.html");
                File.Copy(book, file, true);

                Clean(file, name, i < chapters.Count - 1 ? chapters[i + 1].SelectSingleNode("./a").Attributes[0].Value.Substring(1) : null);
            }
        }

        private static void Clean(string file, string id, string idNext)
        {
            var doc = new HtmlDocument();
            doc.Load(file);
            var header = doc.DocumentNode.SelectSingleNode("//header");
            var nav = doc.DocumentNode.SelectSingleNode("//nav");

            RefreshTocLinks(nav);
            
            header.Remove();
            nav.Remove();

            var body = doc.DocumentNode.SelectSingleNode("//body");

            var removed = new List<HtmlNode>();
            var i = 0; 
            for(; i < body.ChildNodes.Count; ++i)
            {
                var node = body.ChildNodes[i];
                if (node.Id != id) removed.Add(node);
                else break;
            }

            if (!string.IsNullOrEmpty(idNext))
            {
                ++i;
                for (; i < body.ChildNodes.Count; ++i)
                {
                    var node = body.ChildNodes[i];
                    if (node.Id == idNext) break;
                }
                
                for (; i < body.ChildNodes.Count; ++i)
                {
                    var node = body.ChildNodes[i];
                    removed.Add(node);
                }
            }
            
            removed.ForEach(o => o.Remove());

            var root = HtmlNode.CreateNode("<div style=\"display: flex\"></div>");
            var left = HtmlNode.CreateNode("<div style=\"flex: 0 0 auto; background: #EEE;padding: 1em; max-width: 24em\"></div>");
            var right = HtmlNode.CreateNode("<div style=\"flex: 1 1 auto; padding: 1em;\"></div>");

            left.AppendChild(header);
            left.AppendChild(nav);
            
            var contents = body.ChildNodes;
            contents.ToList().ForEach(o =>
            {
                o.Remove();
                right.AppendChild(o);
            });

            root.AppendChild(left);
            root.AppendChild(right);
            body.AppendChild(root);

            doc.Save(file);
        }

        private static void RefreshTocLinks(HtmlNode nav)
        {
            var chapters = nav.SelectNodes("./ul/li");

            for (var i = 0; i < chapters.Count; ++i)
            {
                var chapter = chapters[i];
                var href = chapter.SelectSingleNode("./a").Attributes["href"].Value;
                href = href.Substring(1) + ".html";

                chapter.SelectSingleNode("./a").Attributes["href"].Value = href;

                foreach (var link2 in chapter.SelectNodes("./ul/li/a") ?? new HtmlNodeCollection(null))
                {
                    var href2 = link2.Attributes["href"].Value;
                    link2.Attributes["href"].Value = $"{href}{href2}";
                }
            }
        }
    }
}
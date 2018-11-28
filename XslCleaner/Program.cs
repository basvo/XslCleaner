using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;

namespace XslCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 2)
            {
                Console.WriteLine("Usage: XslCleaner.exe [input.xsl] [output.xsl]");
            }
            else
            {
                try
                {
                    string contents = File.ReadAllText(args[0]);
                    contents = contents.Replace("&quot;", "'");

                    // get all variables
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsMgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");

                    xmlDoc.LoadXml(contents);

                    XmlNodeList varNodes = xmlDoc.SelectNodes("//xsl:variable", nsMgr);

                    var variables = new System.Collections.Hashtable();

                    foreach (XmlNode node in varNodes)
                    {
                        string name = node.Attributes["name"].Value;
                        string val = node.Attributes["select"].Value;

                        if (val.Contains("$var"))
                        {
                            string pattern = @"\$var:v\d+";
                            var match = Regex.Match(val, pattern);
                            while (match.Success)
                            {
                                var varName = match.Value;
                                var varVal = variables[varName.Substring(1, varName.Length - 1)].ToString();

                                val = val.Replace(varName, varVal);

                                match = match.NextMatch();
                            }
                        }

                        variables.Add(name, val);

                        //remove the variable
                        node.ParentNode.RemoveChild(node);
                    }

                    // replace all the variables in the text
                    contents = xmlDoc.OuterXml;

                    foreach (DictionaryEntry variable in variables)
                    {
                        var var = "$" + variable.Key.ToString();

                        contents = contents.Replace("(" + var + ")", "(" + variable.Value.ToString() + ")");
                        contents = contents.Replace("\"" + var + "\"", "\"" + variable.Value.ToString() + "\"");
                    }

                    File.WriteAllText(args[1], contents);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}

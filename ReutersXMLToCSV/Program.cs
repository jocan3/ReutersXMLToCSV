using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace ReutersXMLToCSV
{
    class Program
    {
        /*
         * Parses a Reuter dataset file in XML format to a CSV file containing 2 columns: a text attribure and a class attribute
         * @author Jose Andres Mena Arias
         * @param inputFile - Full path of the input file to be parsed
         * @param outputFile - Full path of the expected output file
         * @param class - Name of the XML Tag containing the categoric attribute 
         * @param allowMissingValues - TRUE or FALSE to include or exclude missing values from output file respectively
         * @param allowMultipleClasses - TRUE to include text where there are more than 1 class
         */
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                throw new Exception("Usage: inputFilePath outputFilePath className allowMissingValues allowMultipleClasses");
            }

            string inputFile = args[0];
            string outputFile = args[1];
            string className = args[2];
            bool allowMissingValues = (args[3] == "TRUE");
            bool allowMultipleClasses = (args[4] == "TRUE");

            Regex nonAlphanumeric = new Regex(@"\W|_"); //Matches all non-alphanumeric characters
            Regex multipleSpaces = new Regex("[ ]{2,}");


            XmlReaderSettings rs = new XmlReaderSettings();
            rs.DtdProcessing = DtdProcessing.Ignore;
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StreamReader(inputFile), rs))
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    bool readClassValues = false;
                    bool readyToReadTextValues = false;
                    bool readTextValues = false;

                    List<string> tempClassValues = new List<string>();
                    List<string> tempTextValues = new List<string>();
                    string tempText = "";

                    writer.WriteLine("\"" + "class" + "\",\"" + "text" + "\"");

                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        if (readClassValues)
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Text:
                                    tempClassValues.Add(reader.Value);
                                    break;
                                case XmlNodeType.EndElement:
                                    if (reader.Name == className)
                                    {
                                        readClassValues = false;
                                        readyToReadTextValues = true;
                                    }
                                    break;
                            }
                        }
                        else if (readyToReadTextValues)
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "TEXT")
                            {
                                readyToReadTextValues = false;
                                readTextValues = true;
                                tempText = "";
                            }
                        }
                        else if (readTextValues)
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Text:
                                    tempText += reader.Value;
                                    break;
                                case XmlNodeType.EndElement:
                                    if (reader.Name == "TEXT")
                                    {
                                        readTextValues = false;
                                     
                                        tempText = nonAlphanumeric.Replace(tempText, " ");
                                        tempText = multipleSpaces.Replace(tempText, " ");

                                        if (tempClassValues.Count() == 1 || (tempClassValues.Count() > 1 && allowMultipleClasses)) {
                                            foreach (var c in tempClassValues)
                                            {
                                                writer.WriteLine("\"" + c + "\",\"" + tempText + "\"");
                                            }
                                        }

                                        if ((tempClassValues.Count() == 0 && allowMissingValues)) {
                                            writer.WriteLine("\"" + "" + "\",\"" + tempText + "\"");
                                        }

                                        tempClassValues.Clear();
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == className)
                            {
                                readClassValues = true;
                                tempClassValues.Clear();
                            }

                        }

                    }
                }
            }

        }
    }
}

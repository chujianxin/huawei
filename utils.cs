using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Xml;
using Microsoft.Win32;
using System.Text.RegularExpressions;
namespace AWClient 
{
    public class utils
    {
        public static string awjson = "";
        public static string awFileName = "";
        //public static List<string> map_name ;
        //public static List<string> map_desc;
        //public static List<string> map_url;
        public static List<XmlDocument> xml=new List<XmlDocument>();

        //public static string mapjson = "";
        public static XmlDocument xmlDoc = null;
        public static string CreateAW()
        {
            string tmpUrl = "";
            List<AW> list = new List<AW>();
            AW aw = new AW();
            List<Record> record = new List<Record>();

            List<Element> element = new List<Element>();

            for (int i = 0; i < MainWindow.lbox.Items.Count; i++)
            {
                ListBoxItem item = (ListBoxItem)MainWindow.lbox.Items[i];
                string jsonData = item.Tag.ToString();
                var AWItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(jsonData);

                if (AWItem[0].record[0].fullUrl != tmpUrl)
                {
                    tmpUrl = AWItem[0].record[0].fullUrl;
                    aw = AWItem[0];
                    record.Add(AWItem[0].record[0]);
                }
                else
                {
                    tmpUrl = AWItem[0].record[0].fullUrl;
                    record[record.Count - 1].elements.Add(AWItem[0].record[0].elements[0]);
                }
                aw.record = record;
                
            }
            list.Add(aw);
            awjson = JsonConvert.SerializeObject(list).Replace(":null", ":\"\"");

            return ConvertJsonString(awjson);
        }
        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);


                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
        public static void SaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            //sfd.InitialDirectory = "c:\\";
            sfd.RestoreDirectory = true;
            sfd.Filter = "AW文件(*.aw)|*.aw";

            if (sfd.ShowDialog() == true)
            {
                string filename = sfd.FileName;

                if (!isRightName(sfd.SafeFileName.Replace(".aw", "")))
               {
                   MessageBox.Show("文件名中仅可包含大小写和下划线且不能以数字开头！");
                   return;
               }
               string dir = System.IO.Path.GetDirectoryName(filename) + "\\aw\\ui\\";
               if (!Directory.Exists(dir))
               {
                   Directory.CreateDirectory(dir);
               }
               if (File.Exists(dir + sfd.SafeFileName))
               {
                   MessageBox.Show("相同的AW已存在，请换名保存！");
                   return;
               }
               
                //try
                //{
                    
                    var element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(utils.awjson);

                    foreach (var record in element[0].record)
                    {
                        //element[0].name = sfd.SafeFileName.Split('.')[0];

                        foreach (var a in record.elements)
                        {
                            if (a.name == "")
                            {
                                MessageBox.Show("有元素的name属性为空!");
                                return;
                            }
                        }
                    }
                    FileStream fs = new FileStream(dir + sfd.SafeFileName, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    string outstr = ConvertJsonString(utils.awjson);

                    outstr = outstr.Replace("\"name\": \"\",", "\"name\": \"" + sfd.SafeFileName.Split('.')[0]+"\"");
                    sw.Write(outstr.Substring(1).Substring(0, outstr.ToString().Length - 2));
                 
                    sw.Flush();
              
                    sw.Close();
                    fs.Close();
                //}

                for (int i = 0; i < xml.Count; i++)
                {
                    FileStream fsmap = null;
                    StreamWriter swmap = null;
                    try
                    {
                        string fn = System.IO.Path.GetDirectoryName(filename) + "\\map\\" + xml[i].SelectSingleNode("map").Attributes["name"].Value ;
                        if (fn.IndexOf(".map") == -1)
                            fn = fn + ".map";
                        if (fn.Length > 240)
                        {
                            MessageBox.Show("Map Filename lenth is too long.");
                            return;
                        }
                        else
                        {
                          
                            if (xml[i].SelectSingleNode("map").Attributes["name"].Value == ".map")
                            {
                                MessageBox.Show("Map文件名不能为空！");
                                return;
                            }
                            else
                            {
                                if (!Directory.Exists(System.IO.Path.GetDirectoryName(fn) ))
                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fn));
                                fsmap = new FileStream(fn, FileMode.Create);
                                swmap = new StreamWriter(fsmap);
                                xml[i].SelectSingleNode("map").Attributes["name"].Value = xml[i].SelectSingleNode("map").Attributes["name"].Value.Replace(".map","");
                                swmap.Write(FormatXml(xml[i].InnerXml.ToString()));
                                swmap.Flush();

                                swmap.Close();
                                fsmap.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                    finally
                    {
                    }
                }
            }
        }
        public  static bool isRightName(string fn)
        {
             Regex reg = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

             return reg.IsMatch(fn);
        }
        public static void CreateMap()
        {
            try
            {
                int xmlNodeIndex = 0;
                xml.Clear();
                var AWItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(awjson);
                bool isIn = false;
                //foreach (var record in AWItem[0].record)
                for (int i = 0; i < AWItem[0].record.Count; i++)
                {
                    Record record = AWItem[0].record[i];
           
                    for (int j = 0; j < i; j++)
                    {
                        if (AWItem[0].record[j].map != record.map || i == 0)
                        {
                            isIn = false;
                        }
                        else
                        { 
                            for(int k=0;k<xml.Count;k++)
                            {
                                if (xml[k].GetElementsByTagName("map")[0].Attributes["name"].Value == AWItem[0].record[j].map)
                                {
                                    isIn = true;
                                    xmlNodeIndex = k; 
                                }
                            }
                            break;
                        }
                    }
                    if (!isIn)
                    {
                        xmlDoc = new XmlDocument();
                        //创建类型声明节点    
                        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "");
                        xmlDoc.AppendChild(node);
                        //创建根节点    
                        XmlNode root = xmlDoc.CreateElement("map");
                        root.Attributes.Append(CreateAttribute(root, "name", record.map));
                        xmlDoc.AppendChild(root);
                        //info
                        XmlNode info = CreateNode(xmlDoc, root, "info", "");
                        XmlNode locator = CreateNode(xmlDoc, info, "locator", ""); 
                        
                        CreateNode(xmlDoc, info, "attributes", "");
                        CreateNode(xmlDoc, info, "description", record.map.Replace(".map",""));
                        CreateNode(xmlDoc, info, "domain", record.website);
                        XmlNode parents = CreateNode(xmlDoc, info, "parents", "");

                        if (record.map.Replace(".map", "").EndsWith("_iframe"))
                        {
                            locator.Attributes.Append(CreateAttribute(locator, "selected", "iframe"));
                            XmlNode parent =CreateNode(xmlDoc, parents, "parent", "");
                            parent.Attributes.Append(CreateAttribute(parent, "depth", "1"));
                            parent.Attributes.Append(CreateAttribute(parent, "locator", "id"));

                            XmlNode attributes = CreateNode(xmlDoc, parent, "attributes", "");
                            attributes.Attributes.Append(CreateAttribute(attributes, "id", "iframe"));
                            attributes.Attributes.Append(CreateAttribute(attributes, "src",record.iframesrc));
                        }
                        else
                        {
                            locator.Attributes.Append(CreateAttribute(locator, "selected", ""));
                        }

                        //elements
                        XmlNode elements = CreateNode(xmlDoc, root, "elements", "");
                        List<Element> nonDuplicateElements = new List<Element>();
                        foreach (var element in record.elements)
                        {
                            if (nonDuplicateElements.Exists(x => x.xpath == element.xpath) == false)
                            {
                                nonDuplicateElements.Add(element);
                            }
                        }

                        foreach (var element in nonDuplicateElements)
                        {
                            XmlNode xmlelement = CreateNode(xmlDoc, elements, "element", "");
                            //if (element.id != "")
                            //    xmlelement.Attributes.Append(CreateAttribute(xmlelement, "name", element.id));
                            //else
                            xmlelement.Attributes.Append(CreateAttribute(xmlelement, "name", element.name));
                            xmlelement.Attributes.Append(CreateAttribute(xmlelement, "type", element.action.ToLower()));

                            XmlNode locator2 = CreateNode(xmlDoc, xmlelement, "locator", "");
                            if (element.id != "")
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "id"));
                            else if (element.name != null)
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "name"));
                            else
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "xpath"));

                            XmlNode att = CreateNode(xmlDoc, xmlelement, "attributes", "");
                            att.Attributes.Append(CreateAttribute(att, "class", element._class));
                            att.Attributes.Append(CreateAttribute(att, "id", element.id));
                            string loc = "";
                            if (element.id != "")
                            {
                                if (element.name != null)
                                {
                                    loc = "id,name,xpath";
                                }
                                else
                                {
                                    loc = "id,xpath";
                                }
                            }
                            else
                            {
                                if (element.name != null)
                                {
                                    loc = "name,xpath";
                                }
                                else
                                {
                                    loc = "xpath";
                                }
                            }
                            att.Attributes.Append(CreateAttribute(att, "locators", loc));
                            att.Attributes.Append(CreateAttribute(att, "name", element.name));
                            att.Attributes.Append(CreateAttribute(att, "noattrxpath", element.noattrxpath));
                            att.Attributes.Append(CreateAttribute(att, "type", element.type.ToLower()));
                            att.Attributes.Append(CreateAttribute(att, "xpath", element.xpath));

                            CreateNode(xmlDoc, xmlelement, "description", "");
                        }
                        xml.Add(xmlDoc);
                       
                    }
                    else
                    {

                        XmlNode elements = xml[xmlNodeIndex].GetElementsByTagName("elements")[0];
                        List<Element> nonDuplicateElements = new List<Element>();
                        foreach (var element in record.elements)
                        {
                            bool isPathE = nonDuplicateElements.Exists(x => x.xpath == element.xpath);
                            bool isNameE = nonDuplicateElements.Exists(x => x.name == element.name);

                            if (isPathE == false && isNameE == false)
                            {
                                for (int ii = 0; ii < i; ii++)
                                {//haoduo elements
                                    bool isPathE1 = AWItem[0].record[ii].elements.Exists(x => x.xpath == element.xpath);
                                    bool isNameE1 = AWItem[0].record[ii].elements.Exists(x => x.name == element.name);

                                    if (!isPathE1 && !isNameE1 && AWItem[0].record[i].map != record.map)
                                    {
                                        nonDuplicateElements.Add(element);
                                        break;
                                    }
                                }

                            }
                        }

                        foreach (var element in nonDuplicateElements)
                        {
                            XmlNode xmlelement = CreateNode(xml[xmlNodeIndex], elements, "element", "");
                            if (element.id != "")
                                xmlelement.Attributes.Append(CreateAttribute(xmlelement, "name", element.id));
                            else
                                xmlelement.Attributes.Append(CreateAttribute(xmlelement, "name", element.name));
                            xmlelement.Attributes.Append(CreateAttribute(xmlelement, "type", element.action.ToLower()));

                            XmlNode locator2 = CreateNode(xml[xmlNodeIndex], xmlelement, "locator", "");
                            if (element.id != "")
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "id"));
                            else if (element.name != null)
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "name"));
                            else
                                locator2.Attributes.Append(CreateAttribute(locator2, "selected", "xpath"));

                            XmlNode att = CreateNode(xml[xmlNodeIndex], xmlelement, "attributes", "");
                            att.Attributes.Append(CreateAttribute(att, "class", element._class));
                            att.Attributes.Append(CreateAttribute(att, "id", element.id));
                            string loc = "";
                            if (element.id != "")
                            {
                                if (element.name != null)
                                {
                                    loc = "id,name,xpath";
                                }
                                else
                                {
                                    loc = "id,xpath";
                                }
                            }
                            else
                            {
                                if (element.name != null)
                                {
                                    loc = "name,xpath";
                                }
                                else
                                {
                                    loc = "xpath";
                                }
                            }
                            att.Attributes.Append(CreateAttribute(att, "locators", loc));
                            att.Attributes.Append(CreateAttribute(att, "name", element.name));
                            att.Attributes.Append(CreateAttribute(att, "noattrxpath", element.noattrxpath));
                            att.Attributes.Append(CreateAttribute(att, "type", element.type.ToLower()));
                            att.Attributes.Append(CreateAttribute(att, "xpath", element.xpath));
                        }
                      
                    }
                }

                
                RefreshMapListBox();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
         
        }
        public static void RefreshMapListBox()
        {
            MainWindow.lboxmap.Items.Clear();
            //显示maps
            for (int i = 0; i < utils.xml.Count; i++)
            {
                ListBoxItem mapitem = new ListBoxItem();
          
                XmlDocument xml = utils.xml[i];
                mapitem.FontSize = 16;
                mapitem.Content = (i + 1).ToString() + ": " + xml.GetElementsByTagName("map")[0].Attributes["name"].Value;
                mapitem.Tag = xml;

                MainWindow.lboxmap.Items.Add(mapitem);
            }
        }
        public static XmlNode CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            return parentNode.AppendChild(node);
        }
        public static XmlAttribute CreateAttribute(XmlNode node, string attributeName, string value)
        {
            try
            {
                XmlDocument doc = node.OwnerDocument;
                XmlAttribute attr = null;
                attr = doc.CreateAttribute(attributeName);
                attr.Value = value;
                node.Attributes.SetNamedItem(attr);
                return attr;
            }
            catch (Exception err)
            {
                string desc = err.Message;
                return null;
            }
        }
        public static void CreateTempXml()
        {
            xmlDoc = new XmlDocument();
            //创建类型声明节点    
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "");
            xmlDoc.AppendChild(node);
            //创建根节点    
            XmlNode root = xmlDoc.CreateElement("XmlData");
            xmlDoc.AppendChild(root);
            //info
            if (!Directory.Exists("c:\\Data"))
                Directory.CreateDirectory("c:\\Data");
            int i = 1;
            foreach (ListBoxItem item in MainWindow.lbox.Items)
            {
                XmlNode nodes = CreateNode(xmlDoc, root, "Nodes", "");
                XmlNode elements = CreateNode(xmlDoc, nodes, "Element", "");
                var element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(item.Tag.ToString()); 

                CreateNode(xmlDoc, elements, "Number", i.ToString());
                CreateNode(xmlDoc, elements, "CssPath", element[0].record[0].elements[0].csspath);
                CreateNode(xmlDoc, elements, "Url", element[0].record[0].fullUrl); 

                xmlDoc.Save("c:\\Data\\Read.xml");
                i++;
            }
        }
        private static string FormatXml(string sUnformattedXml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = System.Xml.Formatting.Indented;
                xtw.Indentation = 1;
                xtw.IndentChar = '\t';
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }  
    }
}

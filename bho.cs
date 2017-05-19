
using mshtml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace COM.HPE
{
    public partial class BHO : IObjectWithSite
    {
        #region EventHandle
        public void onClick(IHTMLEventObj evo)
        {
            try
            {
                evo = document.parentWindow.@event;
                evo.returnValue = true;
                evo.cancelBubble = true;
               
                IEnumerable<AW> aw = GetElement(evo);
                string json = JsonConvert.SerializeObject(aw);
                MainInfo(json);
                
                return;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            } 
        }
        public void onScroll(IHTMLEventObj evo)
        {
            try
            {
                
                return;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        public IEnumerable<AW> GetElement(IHTMLEventObj evo)
        {
            string fullurl = document.url.ToString();
           
            List<AW> list = new List<AW>();

            AW aw = new AW();
            List<Record> records = new List<Record>();
            Record record = new Record();
            List<Element> elements = new List<Element>();
            Element element = new Element();

            record.website = fullurl.Split('/')[0] + "//" + fullurl.Split('/')[2] + "/";
            if (fullurl.EndsWith("/"))
            {
                char[] MyChar = { '_','/'};
                record.fullUrl = fullurl.TrimEnd(MyChar);
                record.map = ReplaceStr(fullurl.Replace(record.website, "").Split('?')[0]).TrimEnd(MyChar) + ".map";
                record.page = fullurl.Replace(record.website, "").TrimEnd(MyChar);
                record.url = fullurl.Replace(record.website, "").TrimEnd(MyChar);
            }
            else
            {
                record.fullUrl = fullurl;
                record.map = ReplaceStr(fullurl.Replace(record.website, "").Split('?')[0]) + ".map";
                record.page = fullurl.Replace(record.website, "");
                record.url = fullurl.Replace(record.website, "");
            }
            record.page = fullurl.Replace(record.website, "");
            record.url = fullurl.Replace(record.website, "");

            aw.comment = "";
          
            element.action = "CLICK";
            element.id = evo.srcElement.id;
            element._class = evo.srcElement.className;
            if (!(evo.srcElement.getAttribute("name") is DBNull))
            {
                element.name = evo.srcElement.getAttribute("name");
                object val = evo.srcElement.getAttribute("name");
                if (val != null && element.id==null)
                    element.name = val.ToString();
                else
                    element.name = element.id;
            }
            else
            {
                element.name = element.id;
            }
            if (!(evo.srcElement.getAttribute("value") is DBNull))
            {
                object val = evo.srcElement.getAttribute("value");
                if(val!=null)
                    element.value = val.ToString();
            }
            element.csspath = FindCssPath(evo.srcElement);
            element.xpath = FindXPath(evo.srcElement);
            element.noattrxpath = FindFullXPath(evo.srcElement);
            if (evo.srcElement.innerText != null)
            {
                if (evo.srcElement.innerText.Length > 100)
                    element.comment = evo.srcElement.innerText.Substring(0, 100);
                else
                    element.comment = evo.srcElement.innerText;
            }
         
            if (!(evo.srcElement.getAttribute("type") is DBNull))
            {
                element.type = evo.srcElement.getAttribute("type");
                
            }
            switch (evo.srcElement.tagName.ToUpper())
            {
                case "INPUT":
                    
                    element.comment = evo.srcElement.getAttribute("value");
                    if (evo.srcElement.getAttribute("type") == "text")
                    {
                        element.action = "INPUT";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    else if (evo.srcElement.getAttribute("type") == "radio")
                    {
                        element.action = "CLICK";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    else if (evo.srcElement.getAttribute("type") == "password")
                    {
                        element.action = "INPUT";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    break;
                case "A":
                    //row1["type"] = "Link";
                    break;
                case "IMG":
                    //row1["type"] = "Image";
                    break;
                case "DIV":
                   // row1["type"] = "div";
                    break;
                case "SELECT":
                   // row1["type"] = "select";
                    break;
                case "TD":
                    //row1["type"] = "table";
                    break;
                case "TH":
                   // row1["type"] = "table";
                    break;
                case "TR":
                   // row1["type"] = "table";
                    break;

                default:
                    //row1["type"] = evo.srcElement.tagName.ToUpper();
                    break;
            }
            elements.Add(element);
            record.elements = elements;
            records.Add(record);
            aw.record = records;

            list.Add(aw);

            return list;
        }
        public IEnumerable<AW> GetIFrameElement(IHTMLEventObj evo)
        {
            
            HTMLDocument doc = evo.srcElement.document; 
            
            string fullurl = doc.url.ToString();
            List<AW> list = new List<AW>();

            AW aw = new AW();
            List<Record> records = new List<Record>();
            Record record = new Record();
            List<Element> elements = new List<Element>();
            Element element = new Element();

            record.website = fullurl.Split('/')[0] + "//" + fullurl.Split('/')[2] + "/";
            record.fullUrl = fullurl;
            //System.Windows.Forms.MessageBox.Show(GetIframeName(evo));
            record.map = ReplaceStr(fullurl.Replace(record.website, "").Split('?')[0]) + "_" + GetIframeName(evo) + "_iframe.map";
            record.page = fullurl.Replace(record.website, "");
            record.url = fullurl.Replace(record.website, "");
            record.iframesrc = GetIframeSrc(evo);
            aw.comment = "";
            element.action = "CLICK";
            element.id = evo.srcElement.id;
            element._class = evo.srcElement.className;
         
            if (!(evo.srcElement.getAttribute("name") is DBNull))
            { 
                element.name = evo.srcElement.getAttribute("name");
                object val = evo.srcElement.getAttribute("name");
				if (val != null && element.id == null)
					element.name = val.ToString();
                else
                    element.name = element.id;
            }
            else
            {
                element.name = element.id;
            }
            if (!(evo.srcElement.getAttribute("value") is DBNull))
            {
                object val = evo.srcElement.getAttribute("value");
                if (val != null)
                    element.value = val.ToString();

            }
            element.csspath = FindCssPath(evo.srcElement);
            element.xpath = FindXPath(evo.srcElement);
            element.noattrxpath = FindFullXPath(evo.srcElement);
            if (evo.srcElement.innerText != null)
            {
                if (evo.srcElement.innerText.Length > 100)
                    element.comment = evo.srcElement.innerText.Substring(0, 100);
                else
                    element.comment = evo.srcElement.innerText;
            }
    
            if (!(evo.srcElement.getAttribute("type") is DBNull))
            {
                element.type = evo.srcElement.getAttribute("type");
            }
           
            switch (evo.srcElement.tagName.ToUpper())
            {
                case "INPUT":

                    element.comment = evo.srcElement.getAttribute("value");
                    if (evo.srcElement.getAttribute("type") == "text")
                    {
                        element.action = "INPUT";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    else if (evo.srcElement.getAttribute("type") == "radio")
                    {
                        element.action = "CLICK";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    else if (evo.srcElement.getAttribute("type") == "password")
                    {
                        element.action = "INPUT";
                        element.comment = evo.srcElement.getAttribute("name");
                    }
                    break;
                case "A":
                    //row1["type"] = "Link";
                    break;
                case "IMG":
                    //row1["type"] = "Image";
                    break;
                case "DIV":
                    // row1["type"] = "div";
                    break;
                case "SELECT":
                    // row1["type"] = "select";
                    break;
                case "TD":
                    //row1["type"] = "table";
                    break;
                case "TH":
                    // row1["type"] = "table";
                    break;
                case "TR":
                    // row1["type"] = "table";
                    break;

                default:
                    //row1["type"] = evo.srcElement.tagName.ToUpper();
                    break;
            }
            elements.Add(element);
            record.elements = elements;
            records.Add(record);
            aw.record = records;

            list.Add(aw);

            return list;
        }
        public string GetIframeSrc(IHTMLEventObj evo)
        { 
            IHTMLWindow2 x =evo.srcElement.document.parentWindow;
          
            return x.location.href;
        }
        public string GetIframeName(IHTMLEventObj evo)
        {
            IHTMLWindow2 x = evo.srcElement.document.parentWindow;

            return x.name;
        }
        public void onIframeClick(IHTMLEventObj evo)
        {
            try
            {
                evo.returnValue = true;
                evo.cancelBubble = true;
                 
                IEnumerable<AW> aw = GetIFrameElement(evo);

                string json = JsonConvert.SerializeObject(aw);
                MainInfo(json);

                return;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        public void OnMouseOver(IHTMLEventObj evo)
        {
            evo.returnValue = true;
            evo.cancelBubble = true; 
        }
        public void OnMouseOut(IHTMLEventObj evo)
        {
            evo.returnValue = true;
            evo.cancelBubble = true; 
        }

        public static string ReplaceStr(string str)
        { 
            str = str.Replace("'", "_");
            str = str.Replace("-", "_");
            str = str.Replace(";", "_");
            str = str.Replace(":", "_");
            str = str.Replace("/", "_");
            str = str.Replace("?", "_");
            str = str.Replace("<", "_");
            str = str.Replace(">", "_");
            str = str.Replace(".", "_");
            str = str.Replace("#", "_");
            str = str.Replace("%", "_");
            str = str.Replace("=", "_");
            str = str.Replace("^", "_");
            str = str.Replace("//", "_");
            str = str.Replace("@", "_");
            str = str.Replace("(", "_");
            str = str.Replace(")", "_");
            str = str.Replace("*", "_");
            str = str.Replace("~", "_");
            str = str.Replace("`", "_");
            str = str.Replace("$", "_");

            return str;
        }  
        #endregion
    }
}

using Kingdee.BOS.Orm.DataEntity;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Frh.K3.FastExtension.ExtensionUI
{
    class ExtensionPresenter : IExtensionPresenter
    {
        private readonly IExtensionView view;
        private List<ExtensionItem> extensions;


        public ExtensionPresenter(IExtensionView view)
        {
            this.view = view;
            view.SetPresenter(this);//把自己发给View持有
        }


        /// <summary>
        /// 通过json生成界面,如果是上有botp推过来,则在json带有INIT标记，如果配置了需要InitValue，则从推过来的字符中取数
        /// 
        /// </summary>
        /// <param name="json"></param>
        public void GenerateByJson(string json)
        {
            extensions = JsonConvert.DeserializeObject<List<ExtensionItem>>(json);


            foreach (ExtensionItem extension in extensions)
            {

                switch (extension.Type)
                {
                    case "Text": view.BuildText(extension); break;
                    case "F8": view.BuildF8(extension); break;
                    case "Decimal": view.BuildDecimal(extension); break;
                    case "Assistant": view.BuildAssistant(extension); break;
                    default: throw new Exception("没有定义");
                }
            }
        }

        public void SetValueOnInit(string jsonValue)
        {

            if (!string.IsNullOrWhiteSpace(jsonValue))
            {
                bool isInit = false;
                if (jsonValue.StartsWith("INIT"))
                {
                    isInit = true;
                    jsonValue = jsonValue.Substring(4, jsonValue.Length - 4);
                }

                Dictionary<string, string> extensionValueDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonValue);
                foreach (ExtensionItem extension in extensions)
                {
                    if (extensionValueDictionary.TryGetValue(extension.Key, out string extensionValue))
                    {
                        if (isInit)
                        {
                            if (extension.InitValue)
                            {
                                view.SetValue(extension.Key, extensionValue);
                            }
                        }
                        else
                        {
                            view.SetValue(extension.Key, extensionValue);
                        }
                    }
                }
            }
        }

        public void LayoutByExtensons()
        {
            foreach (ExtensionItem extension in extensions)
            {
                int i = extensions.IndexOf(extension);
                int columnIndex = i / 3;//在第几列
                int top = columnIndex * 25;
                int rowIndex = i % 3;
                int left = rowIndex * 300;

                this.view.SetAppLayout(top, left, extension.Key, extension.Name, extension.Type, i);
                ;
            }
        }

        /// <summary>
        /// 通过界面值更新json值
        /// </summary>
        public string GenerateToJson()
        {
            Dictionary<string, string> extensionValueDictionary = new Dictionary<string, string>(extensions.Count);
            foreach (ExtensionItem extension in extensions)
            {

                object v = view.GetValue(extension.Key);
                string value = null;
                if (v != null)
                {
                    switch (extension.Type)
                    {
                        case "F8":
                            value = ((DynamicObject)v)["id"].ToString(); break;
                        case "Text":
                            value = v.ToString(); break;
                        case "Decimal":
                            value = v.ToString(); break;
                        case "Assistant":
                            value = ((DynamicObject)v)["id"].ToString(); break;
                        default:
                            throw new Exception("没有找到type");
                    }
                }

                extensionValueDictionary.Add(extension.Key, value);
            }
            return JsonConvert.SerializeObject(extensionValueDictionary);

        }
    }
}

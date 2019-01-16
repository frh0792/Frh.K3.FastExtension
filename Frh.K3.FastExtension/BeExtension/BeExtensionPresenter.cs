using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

 
using static Frh.K3.FastExtension.Constant;

namespace Frh.K3.FastExtension.BeExtension
{
    class BeExtensionPresenter : IBeExtensionPresenter
    {
        private readonly IBeExtensionView view;
        private readonly BeExtensionRepository repository;//持有Repository数据库访问类，不是必须

        public BeExtensionPresenter(IBeExtensionView baseExtensionView, BeExtensionRepository baseExtensionRepository)
        {
            this.view = baseExtensionView;
            this.repository = baseExtensionRepository;
            baseExtensionView.SetPresenter(this);//把自己发给View持有
        }

        /// <summary>
        /// 通过简单扩展字段名，取出json，动态生成界面
        /// 字段名为FExtension_0001，则0001为“简单扩展”
        // H标记代表表头，E标记代表分录，因为BeforeF7Select的事件里面没办法知道是表头还是分录点击的，
        //e.Row为0既可以是表头字段也可以是分录的第一行，太混淆，只能用标记

        /// </summary>
        /// <param name="fieldKey"></param>
        public void ShowBillExtension(string fieldKey, String pageId, int row)
        {
            if (fieldKey.StartsWith(Constant.FExtension))
            {

                string jsonValue = null;
                bool isFieldInHead = IsFieldInHead(fieldKey);
                if (isFieldInHead)
                {
                    jsonValue = view.GetHeadValue(fieldKey) as string;
                }
                else
                {
                    jsonValue = view.GetEntryValue(fieldKey, row) as string;
                }



                string simpleExtensionNumber = fieldKey.Substring(fieldKey.LastIndexOf("_") + 1, fieldKey.Length - 1 - fieldKey.LastIndexOf("_"));
                var simpleExtension = repository.GetSimpleExtensionByNumber(simpleExtensionNumber);

                DynamicFormShowParameter showParam = new DynamicFormShowParameter
                {
                    FormId = FormID.WJ_ExtensionUI,
                    ParentPageId = pageId
                };
                showParam.CustomComplexParams.Add("simpleExtension", simpleExtension);

                showParam.CustomParams.Add("jsonValue", jsonValue);
                showParam.OpenStyle.ShowType = ShowType.Modal;

                //返回编辑好的json
                this.view.ShowForm(showParam,
                    new Action<FormResult>((formResult) =>
                    {
                        if (formResult != null && formResult.ReturnData != null)
                        {
                            string returnjson = (string)formResult.ReturnData;

                            if (isFieldInHead)
                            {
                                view.SetHeadValue(fieldKey, returnjson);
                            }
                            else
                            {
                                view.SetEntryValue(fieldKey, returnjson, row);
                            }

                        }
                    }
                    ));
            }
        }
        /// <summary>
        /// 打印时将扩展属性加入到打印数据源中，在套打中使用动态字段
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="fields"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DynamicObject[] PrintExtensionData(string dataSourceId, List<Field> fields, ref DynamicObjectType dt, DynamicObject[] oldObjs)
        {
            DynamicObject[] newObjs = new DynamicObject[oldObjs.Length];

            bool isEmpty = true;
            foreach (Field field in fields)
            {
                string entryKey = field.Entity.Key;
                string fieldKey = field.Key;
                if (!(dataSourceId.Equals(entryKey) && fieldKey.StartsWith(Constant.FExtension)))
                {
                    continue;
                }

                RegisterSimpleProperty(ref dt, fieldKey);

                for (int i = 0; i < oldObjs.Length; i++)
                {
                    // 使用新的模型，创建新的数据包
                    DynamicObject newObj = new DynamicObject(dt);
                    foreach (var prop in dt.Properties)
                    {
                        prop.SetValue(newObj, prop.GetValue(oldObjs[i]));
                    }
                    string jsonValue = GetJsonValueByKey(fieldKey, i);

                    Dictionary<string, string> jsonValueDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonValue);

                    foreach (var item in jsonValueDictionary)
                    {
                        newObj[item.Key] = item.Value;
                    }

                    newObjs[i] = newObj;

                }
                isEmpty = false;

            }
            if (isEmpty)
            {
                return oldObjs;
            }
            return newObjs;
        }

        private string GetJsonValueByKey(string fieldKey, int row)
        {
            string jsonValue = null;
            if (IsFieldInHead(fieldKey))
            {
                jsonValue = this.view.GetHeadValue(fieldKey) as String;
            }
            else
            {
                jsonValue = this.view.GetEntryValue(fieldKey, row) as String;
            }
            return jsonValue;
        }

        private void RegisterSimpleProperty(ref DynamicObjectType dt, string fieldKey)
        {
            string jsonValue = GetJsonValueByKey(fieldKey, 0);


            Dictionary<string, string> extensionValueDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonValue);
            foreach (string key in extensionValueDictionary.Keys)
            {
                dt.RegisterSimpleProperty(key, typeof(string));
            }
        }

        /// <summary>
        /// 判断字段是在表头还是分录，如果和编码字段的entity一样 ，则在表头
        /// </summary>
        /// <param name="fieldKey"></param>
        /// <returns></returns>
        private bool IsFieldInHead(string fieldKey)
        {
            var numberField = this.view.GetBillNoField();
            var field = this.view.GetField(fieldKey);
            var fieldEntryName = field.Entity.EntryName;
            var numberEntryName = numberField.Entity.EntryName;
            return fieldEntryName.Equals(numberEntryName);
        }

        private bool IsHeadEntity(string dataSourceId)
        {
            var numberField = this.view.GetBillNoField();
            var numberEntryKey = numberField.Entity.Key;
            return dataSourceId.Equals(numberEntryKey);
        }
    }
}

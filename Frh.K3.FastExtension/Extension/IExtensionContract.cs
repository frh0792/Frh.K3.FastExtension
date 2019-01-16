using Kingdee.BOS.Core.DynamicForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Frh.K3.FastExtension.Extension
{
    /// <summary>
    /// 定义Presenter接口，用于解耦
    /// </summary>
    public interface IExtensionPresenter : IBasePresenter
    {
        void GenerateByJson(string json);
        string GenerateToJson();
        void LayoutByExtensons();
        void SetValueOnInit(string jsonValue);
    }


    /// <summary>
    /// 定义View接口
    /// </summary>
    public interface IExtensionView : IBaseView<IExtensionPresenter>
    {

        object GetValue(string field);
        void BuildText(ExtensionItem extension);
        void BuildF8(ExtensionItem extension);
        void BuildDecimal(ExtensionItem extension);
        void BuildAssistant(ExtensionItem extension);
        void SetValue(string field, object value);
        void SetAppLayout(int top, int left, string key, string name, string type, int tableIdx);
    }
}

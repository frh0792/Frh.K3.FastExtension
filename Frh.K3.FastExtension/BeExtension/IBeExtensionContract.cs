using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;


namespace Frh.K3.FastExtension.BeExtension
{

    public interface IBeExtensionView : IBaseView<IBeExtensionPresenter>
    {
        void ShowForm(DynamicFormShowParameter param, Action<FormResult> action);
        object GetHeadValue(string field);
        void SetHeadValue(string field, object value);
        object GetEntryValue(string field, int row);
        void SetEntryValue(string field, object value, int row);
        Field GetField(string field);
        Field GetBillNoField();
    }

    public interface IBeExtensionPresenter : IBasePresenter
    {
        void ShowBillExtension(string fieldKey, String pageId, int row);
        DynamicObject[] PrintExtensionData(string dataSourceId, List<Field> fields, ref DynamicObjectType dt, DynamicObject[] oldObjs);
    }
}

using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frh.K3.FastExtension.BeExtension
{
    /// <summary>
    /// 表单编辑插件，实现View，并持有Presenter的引用,插件可以被所有实现了IBaseExtensionView的替换
    /// 所有PlugIn必须写Description，注册插件的时候才能知道给谁用
    /// </summary>
    [Description("被扩展单据")]
    public class BeExtensionBillPlugIn : AbstractBillPlugIn, IBeExtensionView
    {
        private IBeExtensionPresenter presenter;

        /// <summary>
        /// 如果业务修改，只要修改此一行代码，另作实现类即可
        /// 例如：因业务变更，原有的BaseExtensionPresenter不符合需求，添加新的类 XXXBaseExtensionPresenter,
        /// 则将new BaseExtensionPresenter 改为 new XXXBaseExtensionPresenter，其他原有代码不得修改
        /// </summary> 
        public override void OnBillInitialize(BillInitializeEventArgs e)
        {
            base.OnBillInitialize(e);
            new BeExtensionPresenter(this, new BeExtensionRepository(this.Context));
        }
        /// <summary>
        /// 这个不用修改，presenter会把自己的引用设置过来
        /// </summary>
        /// <param name="presenter"></param>
        public void SetPresenter(IBeExtensionPresenter presenter)
        {
            this.presenter = presenter;
        }


        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);

            presenter.ShowBillExtension(e.FieldKey, this.View.PageId, e.Row);//View里面调用Presenter

        }
        public Field GetField(string field)
        {
            return this.View.BillBusinessInfo.GetField(field);
        }
        public Field GetBillNoField()
        {
            return this.View.BillBusinessInfo.GetBillNoField();
        }

        public object GetHeadValue(string field)
        {
            return this.View.Model.GetValue(field);
        }

        public void SetHeadValue(string field, object value)
        {
            this.View.Model.SetValue(field, value);
        }
        public object GetEntryValue(string field, int row)
        {
            return this.View.Model.GetValue(field, row);
        }

        public void SetEntryValue(string field, object value, int row)
        {
            this.View.Model.SetValue(field, value, row);
        }
        /// <summary>
        /// 调用view的弹出界面
        /// </summary>
        /// <param name="param"></param>

        public void ShowForm(DynamicFormShowParameter param, Action<FormResult> action)
        {
            this.View.ShowForm(param, action);
        }

        /// <summary>
        /// 为打印动态属性修改数据源，使用套打动态属性赋值
        /// </summary>
        /// <param name="e"></param>
        public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
        {


            base.OnPrepareNotePrintData(e);
            string dataSourceId = e.DataSourceId;

            List<Field> fields = GetFieldByEntity(dataSourceId);
            DynamicObjectType dt = e.DynamicObjectType;

            e.DataObjects = presenter.PrintExtensionData(dataSourceId, fields, ref dt, e.DataObjects);

        }

        private List<Field> GetFieldByEntity(string entityKey)
        {
            return this.View.BillBusinessInfo.GetEntity(entityKey).Fields;
        }
    }
}

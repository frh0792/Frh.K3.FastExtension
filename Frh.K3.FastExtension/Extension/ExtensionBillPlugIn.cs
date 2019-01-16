using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;

namespace Frh.K3.FastExtension.Extension
{
    public class ExtensionBillPlugIn : AbstractDynamicFormPlugIn, IExtensionView
    {
        private IExtensionPresenter presenter;

        private BusinessInfo _currBusinessInfo;
        private LayoutInfo _currLayoutInfo;
        private Field modelTextField;
        private FieldAppearance modelTextFieldApp;

        private Field modelDecimalField;
        private DecimalFieldAppearance modelDecimalFieldApp;

        private Field modelF8Field;
        private BaseDataFieldAppearance modelF8FieldApp;

        private Field modelAssistantField;
        private AssistantFieldAppearance modelAssistantFieldApp;


        private string currEntityKey;
        private Entity currEntity;
        private LayoutInfo panelLayout;


        public override void OnSetBusinessInfo(SetBusinessInfoArgs e)
        {
            new ExtensionPresenter(this);

            base.OnSetBusinessInfo(e);
            // 创建当前单据元数据副本，避免直接修改原始元数据，并发时串账
            FormMetadata currmetadata = (FormMetadata)ObjectUtils.CreateCopy(
            this.View.OpenParameter.FormMetaData);
            _currBusinessInfo = currmetadata.BusinessInfo;
            _currLayoutInfo = currmetadata.GetLayoutInfo();

            // 取单据头的元数据模型
            currEntityKey = "FBillHead";
            currEntity = _currBusinessInfo.GetEntity(currEntityKey);

            // 取配置字段的模板，动态添加的字段，是基于模板字段的一个分身
            modelTextField = _currBusinessInfo.GetField("FModelText");//文本模板
            modelDecimalField = _currBusinessInfo.GetField("FModelDecimal");//小数模板
            modelF8Field = _currBusinessInfo.GetField("FModelF8");//F8模板
            modelAssistantField = _currBusinessInfo.GetField("FModelAssistant");//辅助资料模板


            var simpleExtension = this.View.OpenParameter.GetCustomParameter("simpleExtension") as DynamicObject;

            presenter.GenerateByJson(simpleExtension["FExtension"] as string);

            _currBusinessInfo.GetDynamicObjectType(true);
            e.BusinessInfo = _currBusinessInfo;
            e.BillBusinessInfo = _currBusinessInfo;
        }


        public override void OnSetLayoutInfo(SetLayoutInfoArgs e)
        {
            // 用来放置动态创建字段的容器面板：需提前在单据上设计好此面板
            Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel.Container parentPanel = this.View.GetControl<Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel.Container>("F_WJ_Panel");
            // 布局对象：需要把新字段加入到布局对象中，然后再把布局对象添加到面板中
            panelLayout = new LayoutInfo();
            // 取模板字段的外观布局对象，仿照外观
            modelTextFieldApp = this.View.LayoutInfo.GetFieldAppearance("FModelText");
            modelF8FieldApp = this.View.LayoutInfo.GetFieldAppearance("FModelF8") as BaseDataFieldAppearance;
            modelDecimalFieldApp = this.View.LayoutInfo.GetFieldAppearance("FModelDecimal") as DecimalFieldAppearance;
            modelAssistantFieldApp = this.View.LayoutInfo.GetFieldAppearance("FModelAssistant") as AssistantFieldAppearance;
            presenter.LayoutByExtensons();

            // 把layout加入到容器面板中
            parentPanel.AddControls(panelLayout);

            e.LayoutInfo = _currLayoutInfo;
            e.BillLayoutInfo = _currLayoutInfo;
            this.View.SendDynamicFormAction(this.View);
            base.OnSetLayoutInfo(e);
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string jsonValue = this.View.OpenParameter.GetCustomParameter("jsonValue") as string;
            presenter.SetValueOnInit(jsonValue);
        }
        public void SetAppLayout(int top, int left, string key, string name, string type, int tableIdx)
        {

            int lcid = this.Context.UserLocale.LCID;
            // 基于模板字段的外观，产生出新字段的外观
            FieldAppearance newFldApp = null;
            switch (type)
            {
                case "F8":
                    newFldApp = (BaseDataFieldAppearance)ObjectUtils.CreateCopy(modelF8FieldApp);
                    break;
                case "Text":
                    newFldApp = (TextFieldAppearance)ObjectUtils.CreateCopy(modelTextFieldApp); break;
                case "Decimal":
                    newFldApp = (DecimalFieldAppearance)ObjectUtils.CreateCopy(modelDecimalFieldApp); break;
                case "Assistant":
                    newFldApp = (AssistantFieldAppearance)ObjectUtils.CreateCopy(modelAssistantFieldApp); break;
                default:
                    throw new Exception("没有找到type");
            }
            newFldApp.Key = key;
            newFldApp.EntityKey = "FBillHead";
            newFldApp.Caption = new LocaleValue(name);
            newFldApp.Field = this.View.BusinessInfo.GetField(key);
            newFldApp.Top = new LocaleValue(top.ToString(), lcid);
            newFldApp.Left = new LocaleValue(left.ToString(), lcid);
            //newFldApp.Tabindex = modelTextFieldApp.Tabindex + tableIdx + 1;
            // 必须清除字段的容器属性，否则生成的控件，不能挂到指定的容器面板上
            newFldApp.Container = "";
            _currLayoutInfo.Add(newFldApp);
            // 把新字段外观，同步加入到容器面板的布局对象中
            panelLayout.Add(newFldApp);

        }


        /// <summary>
        /// 如果业务修改，只要修改此一行代码，另作实现类即可
        /// 例如：因业务变更，原有的BaseExtensionPresenter不符合需求，添加新的类 XXXBaseExtensionPresenter,
        /// 则将new BaseExtensionPresenter 改为 new XXXBaseExtensionPresenter，其他原有代码不得修改
        ///
        /// </summary>
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            //new BillExtensionPresenter(this);
        }

        /// <summary>
        /// 这个不用修改，presenter会把自己的引用设置过来
        /// </summary>
        /// <param name="presenter"></param>
        public void SetPresenter(IExtensionPresenter presenter)
        {
            this.presenter = presenter;
        }



        public override void ButtonClick(ButtonClickEventArgs e)
        {

            if (e.Key == "FBTNOK")
            {
                string json = presenter.GenerateToJson();
                this.View.ReturnToParentWindow(new FormResult(json));
                this.View.Close();
            }

        }

        public object GetValue(string field)
        {
            return this.View.Model.GetValue(field);
        }

        public void BuildText(ExtensionItem extension)
        {

            Field propField = (Field)ObjectUtils.CreateCopy(modelTextField);
            propField.DynamicProperty = null;
            propField.ChildrenFields.Clear();
            propField.EntityKey = currEntityKey;
            propField.Entity = currEntity;
            if (propField.UpdateActions != null)
                propField.UpdateActions.Clear();

            // 必改属性，涉及到数据的加载
            propField.Key = extension.Key;
            propField.FieldName = extension.Key.ToUpperInvariant();
            propField.PropertyName = extension.Key;
            propField.Name = new LocaleValue(extension.Name);

            _currBusinessInfo.Add(propField);

        }

        public void SetValue(string field, object value)
        {
            this.View.Model.SetValue(field, value);
        }

        public void BuildF8(ExtensionItem extension)
        {
            BaseDataField propField = (BaseDataField)ObjectUtils.CreateCopy(modelF8Field);
            propField.DynamicProperty = null;
            propField.ChildrenFields.Clear();
            propField.EntityKey = currEntityKey;
            propField.Entity = currEntity;
            if (propField.UpdateActions != null)
                propField.UpdateActions.Clear();

            // 必改属性，涉及到数据的加载
            propField.Key = extension.Key;
            propField.FieldName = extension.Key.ToUpperInvariant();
            propField.PropertyName = extension.Key;
            propField.Name = new LocaleValue(extension.Name);
            propField.LookUpObject.FormId = extension.F8FormId;
            propField.LookUpObject.TableName = extension.F8TableName;

            FormMetadata materialMetada =
               MetaDataServiceHelper.Load(this.Context, extension.F8FormId) as FormMetadata;


            propField.RefFormDynamicObjectType = materialMetada.BusinessInfo.GetDynamicObjectType();
            propField.TableName = extension.F8TableName;
            //  propField.RefFormDynamicObjectType = new DynamicObjectType(extension.F8FormId);

            _currBusinessInfo.Add(propField);
        }
        public void BuildAssistant(ExtensionItem extension)
        {
            AssistantField propField = (AssistantField)ObjectUtils.CreateCopy(modelAssistantField);
            propField.DynamicProperty = null;
            propField.ChildrenFields.Clear();
            propField.EntityKey = currEntityKey;
            propField.Entity = currEntity;
            if (propField.UpdateActions != null)
                propField.UpdateActions.Clear();

            // 必改属性，涉及到数据的加载
            propField.Key = extension.Key;
            propField.FieldName = extension.Key.ToUpperInvariant();
            propField.PropertyName = extension.Key;
            propField.Name = new LocaleValue(extension.Name);

            propField.LookUpObjectID = extension.LookupObjectId;

            _currBusinessInfo.Add(propField);
        }
        public void BuildDecimal(ExtensionItem extension)
        {
            DecimalField propField = (DecimalField)ObjectUtils.CreateCopy(modelDecimalField);
            propField.DynamicProperty = null;
            propField.ChildrenFields.Clear();
            propField.EntityKey = currEntityKey;
            propField.Entity = currEntity;
            if (propField.UpdateActions != null)
                propField.UpdateActions.Clear();

            // 必改属性，涉及到数据的加载
            propField.Key = extension.Key;
            propField.FieldName = extension.Key.ToUpperInvariant();
            propField.PropertyName = extension.Key;
            propField.Name = new LocaleValue(extension.Name);

            _currBusinessInfo.Add(propField);
        }


    }
}

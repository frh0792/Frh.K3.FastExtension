using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Linq;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.ServiceHelper;

namespace Frh.K3.FastExtension.BeExtension
{
    /// <summary>
    /// 做为model层，专门访问数据库操作
    /// </summary>
    class BeExtensionRepository
    {
        private readonly Context context;
        public BeExtensionRepository(Context context)
        {
            this.context = context;
        }
        public DynamicObject GetSimpleExtensionByNumber(String number)
        {
            FormMetadata meta = MetaDataServiceHelper.Load(context, Constant.FormID.WJ_Extension) as FormMetadata;

            // 构建查询参数，设置过滤条件
            QueryBuilderParemeter queryParam = new QueryBuilderParemeter
            {
                FormId = Constant.FormID.WJ_Extension,
                BusinessInfo = meta.BusinessInfo,

                FilterClauseWihtKey = string.Format(" {0} = '{1}' ",
                meta.BusinessInfo.GetForm().NumberFieldKey,
                number)
            };

            var bdObjs = BusinessDataServiceHelper.Load(context,
                meta.BusinessInfo.GetDynamicObjectType(),
                queryParam);
            if (bdObjs.Length == 1)
            {
                return bdObjs.FirstOrDefault();
            }
            else if (bdObjs.Length == 0)
            {
                return null;
            }
            else
            {
                throw new Exception("编码 " + number + " formId " + Constant.FormID.WJ_Extension + "找到多个数据");
            }


        }
    }
}

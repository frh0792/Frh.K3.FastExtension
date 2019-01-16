using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frh.K3.FastExtension
{
    public interface IBaseView<T>
    {
        void SetPresenter(T presenter);
    }
}

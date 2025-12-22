
using UI.Models;

namespace UI.Menu
{
    public abstract class BaseNonContextMenu
    {
        protected BL.Factory _f;
        
        protected List<MenuItem> _lis;

        public BaseNonContextMenu(BL.Factory f)
        {
            _lis = new List<MenuItem>();
            _f = f;
        }

        public List<MenuItem> GetItems()
        {
            return _lis;
        }


        public  MenuItem AMI(string strName, string strUrl, string icon = null, string strParentID = null, string strID = null, string strTarget = null)
        {
            var c = new MenuItem() { Name = _f.tra(strName), Url = strUrl, Target = strTarget, ID = strID, ParentID = strParentID, Icon = icon };
            _lis.Add(c);
            return c;
        }
        public void AMI_NOTRA(string strName, string strUrl, string icon = null, string strParentID = null, string strID = null, string strTarget = null)
        {
            _lis.Add(new MenuItem() { Name = strName, Url = strUrl, Target = strTarget, ID = strID, ParentID = strParentID, Icon = icon });
        }
        public void DIV(string strName = null, string strParentID = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(strName, 30), ParentID = strParentID });
        }
        public void DIV_TRANS(string strName = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(_f.tra(strName), 30) });
        }
        
    }
}

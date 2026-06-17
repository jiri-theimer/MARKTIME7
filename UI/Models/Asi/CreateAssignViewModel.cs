using System.Linq;

namespace UI.Models
{
    public class CreateAssignViewModel
    {
        public int j08RecordPid { get; set; }
        public string j08RecordEntity { get; set; }
        public string j02IDs { get; set; }
        public string Persons { get; set; }

        public string j11IDs { get; set; }
        public string Teams { get; set; }

        public string j04IDs { get; set; }
        public string Roles { get; set; }
        public bool j08IsAllUsers { get; set; }

        public void SetInitValues(BL.Factory f,string recprefix, int recpid)
        {
            var lisJ08 = f.FBL.GetListJ08(recprefix, recpid);
            if (lisJ08.Count() == 0) return;

            if (lisJ08.Any(p => p.j08IsAllUsers))
            {
                this.j08IsAllUsers = true;
                return;
            }
            if (lisJ08.Any(p => p.j11ID>0))
            {
                var lisJ11 = f.j11TeamBL.GetList(new BO.myQueryJ11() { pids= lisJ08.Where(p=>p.j11ID>0).Select(p=>p.j11ID).ToList() });
                this.j11IDs = String.Join(",", lisJ11.Select(p => p.pid));
                this.Teams = String.Join(",", lisJ11.Select(p => p.j11Name));
            }
            if (lisJ08.Any(p => p.j02ID > 0))
            {
                var lisJ02 = f.j02UserBL.GetList(new BO.myQueryJ02() { pids = lisJ08.Where(p => p.j02ID > 0).Select(p => p.j02ID).ToList() });
                this.j02IDs = String.Join(",", lisJ02.Select(p => p.pid));
                this.Persons = String.Join(",", lisJ02.Select(p => p.FullnameDesc));
            }
            if (lisJ08.Any(p => p.j04ID > 0))
            {
                var lisJ04 = f.j04UserRoleBL.GetList(new BO.myQueryJ04() { pids = lisJ08.Where(p => p.j04ID > 0).Select(p => p.j04ID).ToList() });
                this.j04IDs = String.Join(",", lisJ04.Select(p => p.pid));
                this.Roles = String.Join(",", lisJ04.Select(p => p.j04Name));
            }

        }
        public List<BO.j08CreatePermission> getList4Save(BL.Factory f)
        {
            var ret = new List<BO.j08CreatePermission>();

            if (this.j08IsAllUsers)
            {
                ret.Add(new BO.j08CreatePermission() { j08IsAllUsers = true });
            }
            if (!string.IsNullOrEmpty(this.j02IDs))
            {
                foreach(var intJ02ID in BO.Code.Bas.ConvertString2ListInt(this.j02IDs)){
                    ret.Add(new BO.j08CreatePermission() { j02ID = intJ02ID });
                }

            }
            if (!string.IsNullOrEmpty(this.j11IDs))
            {
                foreach (var intJ11ID in BO.Code.Bas.ConvertString2ListInt(this.j11IDs))
                {
                    ret.Add(new BO.j08CreatePermission() { j11ID = intJ11ID });
                }

            }
            if (!string.IsNullOrEmpty(this.j04IDs))
            {
                foreach (var intJ04ID in BO.Code.Bas.ConvertString2ListInt(this.j04IDs))
                {
                    ret.Add(new BO.j08CreatePermission() { j04ID = intJ04ID });
                }

            }

            return ret;
        }
    }
}

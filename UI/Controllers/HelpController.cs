using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class HelpController : Controller
    {
        private BL.Factory _f;
        public HelpController(BL.Factory f)
        {
            _f = f;
        }
        public IActionResult Index(string file)
        {
            var v = new HelpViewModel() { SelectedFile=file };

            RefreshState(v);


            if (v.SelectedFile != null)
            {
                var c = v.lisFiles.FirstOrDefault(p => p == v.SelectedFile);
                if (c != null)
                {
                    var arr = c.Split("|");
                    v.SelectedFile = arr[1];
                   
                    var strPath = $"{_f.App.RootUploadFolder}\\_distribution\\help";
                    
                    if (!string.IsNullOrEmpty(arr[0]))
                    {
                        strPath = $"{strPath}\\{arr[0]}";
                    }
                    strPath = $"{strPath}\\{arr[1]}";
                    if (System.IO.File.Exists(strPath))
                    {
                        v.Header = System.IO.Path.GetFileNameWithoutExtension(strPath);
                        v.SelectedHtml = System.IO.File.ReadAllText(strPath);
                    }
                    
                }
            }

            return View(v);
        }

        private void RefreshState(HelpViewModel v)
        {
            var lisflat = new List<UI.Models.Asi.TreeNode>();

            var strPath = $"{_f.App.RootUploadFolder}\\_distribution\\help\\dirs.pdw";
            var dirs = BO.Code.Bas.ConvertString2List(System.IO.File.ReadAllText(strPath), "\n");
            strPath = $"{_f.App.RootUploadFolder}\\_distribution\\help\\files.pdw";
            v.lisFiles = BO.Code.Bas.ConvertString2List(System.IO.File.ReadAllText(strPath), "\n");

            int y = 100000;
            for (int x = 0; x < dirs.Count(); x++)
            {
                lisflat.Add(new UI.Models.Asi.TreeNode() { Id = x + 1, IdParent = 0, Name = dirs[x] });
                foreach (var s in v.lisFiles)
                {
                    var arr = s.Split("|");
                    if (arr[0] == dirs[x])
                    {
                        var n = new UI.Models.Asi.TreeNode() { Id = y, IdParent = x + 1, Name = arr[1].Replace(".html", "") };
                        n.Url = $"/Help/Index?file={s}";
                        lisflat.Add(n);
                        y += 1;

                        if (v.SelectedFile != null && v.SelectedFile == s)
                        {
                            v.SelectedId = n.Id;
                        }
                    }
                }
            }

            v.treeNodes = UI.Code.basTree.BuildTree(lisflat);
        }
    }
}

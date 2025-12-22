using UI.Menu;

namespace UI.Menu
{
    public class workflowContextMenu:BaseContextMenu
    {
        public workflowContextMenu(BL.Factory f,int pid, string prefix, string source) : base(f, pid)
        {

            
            HEADER("Workflow");

            AMI_Workflow(prefix, pid,0,0);

            

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FilmOrganizer
{
    public partial class Form1 : Form
    {
        private FilmOrganizer.Settings settingsForm;
        private ToolTip TurnOnToolTip;
       
        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            settingsForm.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            this.settingsForm = new FilmOrganizer.Settings();
            this.settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.FilmLibrary.AfterSelect += FilmLibrary_NodeMouseCLick;
            this.Films.NodeMouseDoubleClick += toolStripButton1_Click;
            this.Films.NodeMouseHover += Films_MouseHover;
            this.Films.MouseLeave += control_MouseLeave;

            ImageList il = new ImageList();
            il.ImageSize = new Size(25, 25);
            il.Images.Add(Properties.Resources.FolderIcon);
            il.Images.Add(Properties.Resources.redFIlm);
            il.Images.Add(Properties.Resources.green_film);
            Films.ImageList = il;

            TurnOnToolTip = new ToolTip();

            LoadLibraries();
        }
        private void Films_MouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            
            string str = "";
            if (e.Node.ImageIndex == 1)
                str = "Not watched";
            else if (e.Node.ImageIndex == 2)
                str = "Watched";
            else
            {
                var control = (Control)sender;
                TurnOnToolTip.Hide(control);
                return;
            }

            var relativePoint = this.PointToClient(Cursor.Position); ;
            int x = this.Location.X + relativePoint.X + e.Node.Bounds.Width;
            int y = this.Location.Y + relativePoint.Y + e.Node.Bounds.Height;

            if (!string.IsNullOrEmpty(str))
                TurnOnToolTip.Show(str, Films, Films.PointToClient(new Point(x,y)));
        }
        private void control_MouseLeave(object sender, EventArgs e)
        {
            var control = (Control)sender;
            TurnOnToolTip.Hide(control);
        }
        private void FilmLibrary_NodeMouseCLick(object sender,EventArgs e)
        {
            TreeNode Node = FilmLibrary.SelectedNode;
            if (Node == null || Node.Parent == null ) return;

            this.Films.SuspendLayout();
            this.Films.Nodes.Clear();

            if (Directory.Exists((string)Node.Tag))
            {
                TreeNode parent = new TreeNode(Node.Text);
                parent.Tag = Node.Tag;
                var l = DigForFiles((string)parent.Tag);

                foreach(string str in l)
                {
                    TreeNode n = new TreeNode(GetName(str));
                    n.Tag = str;
                    parent.Nodes.Add(n);
                }

                this.Films.Nodes.Add(parent);
                parent.ExpandAll();
            }

            RefreshWatched();
            this.Films.ResumeLayout();
        }
        private void LoadLibraries()
        {
            this.FilmLibrary.SuspendLayout();
            Properties.Settings set = new Properties.Settings();
            this.FilmLibrary.Nodes.Clear();
            foreach(string str in set.LibraryList)
            {
                AddDirectory(str);
            }

            this.FilmLibrary.ResumeLayout();
        }
        private void AddDirectory(string dir)
        {
            if (!Directory.Exists(dir)) return;

            TreeNode parent = new TreeNode(GetName(dir));
            parent.Tag = dir;

            this.FilmLibrary.Nodes.Add(parent);

            foreach(string str in GetDirectories(dir))
            {
                TreeNode n = new TreeNode(GetName(str));
                n.Tag = str;
                parent.Nodes.Add(n);
            }
            parent.ExpandAll();
        }
        private string GetName(string str)
        {
            return str.Substring(str.LastIndexOf("\\") + 1, str.Length - str.LastIndexOf("\\") - 1);
        }
        private List<string> GetDirectories(string dir)
        {
            List<string> result = new List<string>();

            DirectoryInfo di = new DirectoryInfo(dir);

            foreach (var fi in di.GetDirectories())
                result.Add(fi.FullName);

            di = null;

            return result;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Add film directory";
            
            // OK button was pressed.
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string str = fbd.SelectedPath;
                Properties.Settings set = new Properties.Settings();
                if (set.LibraryList.Contains(str)) return;

                set.LibraryList.Add(str);
                set.Save();

                AddDirectory(str);

                Films.Nodes.Clear();
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (this.FilmLibrary.SelectedNode == null || this.FilmLibrary.SelectedNode.Parent!=null) return;

            TreeNode n = this.FilmLibrary.SelectedNode;
            this.FilmLibrary.Nodes.Remove(n);
            Properties.Settings set = new Properties.Settings();
            set.LibraryList.Remove((string)n.Tag);
            set.Save();

            Films.Nodes.Clear();
        }
        private List<string> DigForFiles(string dir)
        {
            List<string> result = new List<string>();
            List<string> dirs = new List<string>() { dir };
            List<string> temp;

            while (dirs.Count > 0)
            {
                temp = new List<string>();

                foreach (string str in dirs)
                {
                    foreach (string name in GetFiles(str))
                        result.Add(name);

                    foreach (string name in GetDirectories(str))
                        temp.Add(name);
                }

                dirs = temp;
            }

            dirs = null;
            temp = null;
            return result;
        }
        private List<string> GetFiles(string dir)
        {
            List<string> result = new List<string>();

            DirectoryInfo di = new DirectoryInfo(dir);
            Properties.Settings set = new Properties.Settings();
            foreach (var fi in di.GetFiles())
                if(set.ExtensionsList.Contains(fi.Extension) && fi.Extension!="@")
                    result.Add(fi.FullName);

            di = null;

            return result;
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (FilmLibrary.SelectedNode == null || !Directory.Exists((string)FilmLibrary.SelectedNode.Tag)) return;

            string dir = (string)FilmLibrary.SelectedNode.Tag;
            System.Diagnostics.Process.Start("explorer.exe", dir);
            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Properties.Settings set = new Properties.Settings();
            if (Films.SelectedNode == null || Films.SelectedNode.Parent == null 
                || !File.Exists((string)Films.SelectedNode.Tag ) ||
                !File.Exists(set.Dir)) return;
           
            string dir = (string)Films.SelectedNode.Tag;
            System.Diagnostics.Process.Start(set.Dir, dir);
            
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (Films.SelectedNode == null || Films.SelectedNode.Parent == null
                || !File.Exists((string)Films.SelectedNode.Tag) ) return;

            string dir = (string)Films.SelectedNode.Tag;
            string parent = (string)Films.SelectedNode.Parent.Tag;

            parent += "\\Info.nfo";

            List<string> watched;
            if (File.Exists(parent))
                watched = File.ReadAllLines(parent).ToList();
            else
            {
                watched = new List<string>();
            }

            if (!watched.Contains(dir))
            {
                watched.Add(dir);
                File.WriteAllLines(parent, watched);
            }

            watched = null;
            RefreshWatched();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (Films.SelectedNode == null || Films.SelectedNode.Parent == null
                || !File.Exists((string)Films.SelectedNode.Tag)) return;

            string dir = (string)Films.SelectedNode.Tag;
            string parent = (string)Films.SelectedNode.Parent.Tag;

            parent += "\\Info.nfo";

            List<string> watched;
            if (File.Exists(parent))
                watched = File.ReadAllLines(parent).ToList();
            else
                return;

            if (watched.Contains(dir))
                watched.Remove(dir);

            File.WriteAllLines(parent, watched);

            watched = null;
            RefreshWatched();
        }
        private void RefreshWatched()
        {
            if (Films.Nodes.Count == 0) return;
            Films.Nodes[0].ImageIndex = 0;
            Films.Nodes[0].SelectedImageIndex = 0;

            string parent = (string)Films.Nodes[0].Tag;

            parent +=  "\\Info.nfo";

            List<string> watched;
            if (File.Exists(parent))
                watched = File.ReadAllLines(parent).ToList();
            else
                watched=new List<string>();

            foreach (TreeNode n in Films.Nodes[0].Nodes)
                if (!watched.Contains((string)n.Tag))
                {
                    n.ImageIndex = 1;
                    n.SelectedImageIndex = 1;
                }
                else
                {
                    n.ImageIndex = 2;
                    n.SelectedImageIndex = 2;
                }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            LoadLibraries();
            Films.Nodes.Clear();
        }
    }
}

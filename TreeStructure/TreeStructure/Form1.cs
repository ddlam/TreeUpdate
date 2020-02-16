using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeStructure
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            try {
                foreach (var directory in directoryInfo.GetDirectories())
                    directoryNode.Nodes.Add(CreateDirectoryNode(directory));
                foreach (var file in directoryInfo.GetFiles()
                    .Where(name => !name.FullName.EndsWith(".av", StringComparison.OrdinalIgnoreCase))
                    .Where(name => !name.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                    )
                {
                    if (file.LastWriteTime > DateTime.Now.AddDays(Convert.ToInt32(txtDay.Text) * -1))
                    {
                        directoryNode.Nodes.Add(new TreeNode(file.Name));
                    }
                }
            } 
            catch (AggregateException ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }


            return directoryNode;
        }
        //
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Nodes[0].Text == "..." && e.Node.Nodes[0].Tag == null)
                {
                    e.Node.Nodes.Clear();


                    string[] dirs = Directory.GetDirectories(e.Node.Tag.ToString());

                    foreach (string dir in dirs)
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        TreeNode node = new TreeNode(di.Name, 0, 1) { Checked = true };

                        try
                        {
                            node.Tag = dir;


                            if (di.GetDirectories().Count() > 0)
                                node.Nodes.Add(null, "...", 0, 0).Checked = node.Checked;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            node.ImageIndex = 12;
                            node.SelectedImageIndex = 12;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Tree_to_Grid", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        }
                        finally
                        {
                            node.Checked = e.Node.Checked;
                            e.Node.Nodes.Add(node);
                        }
                    }
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            btnView.Enabled = false;
            TreeNode node = e.Node;
            bool is_checked = node.Checked;
            foreach (TreeNode childNode in e.Node.Nodes)
            {
                childNode.Checked = e.Node.Checked;
            }
            treeView1.SelectedNode = node;
        }

        public void CheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = true;
                CheckChildren(node, true);
            }
        }

        public void UncheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                CheckChildren(node, false);
            }
        }

        private void CheckChildren(TreeNode rootNode, bool isChecked)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                CheckChildren(node, isChecked);
                node.Checked = isChecked;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DirectoryCopy(txtPath.Text, "C:\\AcquireUpdate", true);
        }
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);


                if (file.LastWriteTime > DateTime.Now.AddDays(Convert.ToInt32(txtDay.Text) * -1)
                        && file.FullName.EndsWith(".av", StringComparison.OrdinalIgnoreCase)
                        && file.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)

                    )
                {

                    file.CopyTo(temppath, false);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void BtnView_Click(object sender, EventArgs e)
        {

            ListDirectory(treeView1,txtPath.Text);
            CheckAllNodes(treeView1.Nodes);
        }
    }
}

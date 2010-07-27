using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MenuCreator
{
    public partial class Form1 : Form
    {
        private Int32 CurrentNodeID = 0;

        public Form1()
        {
            InitializeComponent();
            ClearNodes();
        }

        private void ClearNodes()
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("Items");
            treeView1.Nodes[0].Checked = true;
            treeView1.Nodes[0].Tag = new NodeTag();
        }
        private void AddNode(TreeNode Parent, String Label, Int32 ItemID, Boolean Enabled, Boolean CSROnly)
        {
            treeView1.BeginUpdate();

            TreeNode node = new TreeNode(Label);
            
            node.Checked = Enabled;
            node.Tag = new NodeTag(0, 0, ItemID, CSROnly);

            if (Parent == null)
            {
                treeView1.Nodes[0].Nodes.Add(node);
            }
            else
            {
                Parent.Nodes.Add(node);
            }
            treeView1.EndUpdate();
            treeView1.Refresh();
        }
        public static TreeNode GetNode(TreeNodeCollection c, int ID)
        {
            if (ID == 0)
                return null;

            foreach (TreeNode n in c)
            {
                NodeTag nttemp = (NodeTag)n.Tag;
                if (nttemp == null) continue;

                if (nttemp.ID == ID)
                    return n;

                else if (n.Nodes.Count > 0)
                {
                    TreeNode temp = GetNode(n.Nodes, ID);
                    if (temp != null) return temp;
                }
                
            }
            return null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;

            if(node != null)
            {
                AddNode(node, textBox1.Text, Convert.ToInt32(textBox2.Text), checkBox1.Enabled, checkBox2.Enabled);
            } else {
                AddNode(null, textBox1.Text, Convert.ToInt32(textBox2.Text), checkBox1.Enabled, checkBox2.Enabled);
            }
        }
        private void WriteToFile(String Filename)
        {
            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(Filename);
            }
            catch (Exception ex) { MessageBox.Show("Failed to open file: \n"+ex.Message); return; }
            CurrentNodeID = 0;
            WriteNodes(sw, treeView1.Nodes, 0);

            sw.Flush();
            sw.Close();
        }
        private void WriteNodes(StreamWriter sw, TreeNodeCollection treeNodeCollection, Int32 ParentID)
        {
            foreach (TreeNode n in treeNodeCollection)
            {
                CurrentNodeID++;
                NodeTag ntold = (NodeTag)n.Tag;
                NodeTag nt = new NodeTag(CurrentNodeID, ParentID, ntold.ItemID, ntold.CSROnly);
                StringBuilder sb = new StringBuilder();
                sb.Append(nt.ID);
                sb.Append(",");
                sb.Append(nt.ParentID);
                sb.Append(",");
                sb.Append(n.Text);
                sb.Append(",");
                sb.Append(nt.ItemID);
                sb.Append(",");
                sb.Append(n.Checked ? 1:0);
                sb.Append(",");
                sb.Append(nt.CSROnly ? 1 : 0);
                if(CurrentNodeID>1)
                    sw.WriteLine(sb.ToString());

                if (n.Nodes.Count > 0)
                    WriteNodes(sw, n.Nodes, CurrentNodeID);
            }
        }
        private void LoadNodes(String filename)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(filename);
            }
            catch (Exception Exception){ MessageBox.Show("Problem opening file: \n" + Exception.Message); return; }

            if (sr == null) return;

            ClearNodes();

            String temp = sr.ReadToEnd();
            sr.Close();

            String[] nodes = temp.Split(new char[] { '\n' });
            foreach (string s in nodes)
            {
                String[] data = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (data.Length == 0) continue;
                if (data.Length != 6)
                {
                    MessageBox.Show("Problem parsing line: " + s);
                    continue;
                }

                NodeTag tag = new NodeTag();

                tag.ID = Convert.ToInt32(data[0]);
                tag.ParentID = Convert.ToInt32(data[1]);
                TreeNode node = new TreeNode(data[2]);
                tag.ItemID = Convert.ToInt32(data[3]);
                node.Checked = Convert.ToInt32(data[4]) == 1;
                tag.CSROnly = Convert.ToInt32(data[5]) == 1;
                
                node.Tag = tag;
                TreeNode parent = GetNode(treeView1.Nodes, tag.ParentID);
                if (parent != null)
                    parent.Nodes.Add(node);
                else
                    treeView1.Nodes[0].Nodes.Add(node);
            }
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".csv";
            ofd.Filter = "CSV File|*.csv";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofd.FileName))
                {
                    LoadNodes(ofd.FileName);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".csv";
            sfd.Filter = "CSV File|*.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if(File.Exists(sfd.FileName))
                    File.Delete(sfd.FileName);
                WriteToFile(sfd.FileName);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = treeView1.SelectedNode.Text;
            NodeTag nt = (NodeTag)treeView1.SelectedNode.Tag;
            textBox2.Text = nt.ItemID.ToString();
            checkBox1.Checked = treeView1.SelectedNode.Checked;
            checkBox2.Checked = nt.CSROnly;
        }


        private void SaveExistingNode_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn == null) return;

            NodeTag tag = (NodeTag)tn.Tag;

            tn.Text = textBox1.Text;
            tag.ItemID = Convert.ToInt32(textBox2.Text);
            tn.Checked = checkBox1.Checked;
            tag.CSROnly = checkBox2.Checked;
        }
    }
    internal class NodeTag
    {
        public NodeTag() { ;}
        public Int32 ID { get; set; }
        public Int32 ParentID { get; set; }
        public Int32 ItemID { get; set; }
        public Boolean CSROnly { get; set; }
        public NodeTag(Int32 id, Int32 parentid, Int32 itemid, Boolean csronly)
        {
            ID = id;
            ParentID = parentid;
            ItemID = itemid;
            CSROnly = csronly;
        }
    }
}

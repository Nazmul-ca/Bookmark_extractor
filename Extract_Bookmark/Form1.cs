using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using iTextSharp.text.pdf;



namespace Extract_book_mark
{
    public partial class Form1 : Form
    {

        TreeNode tn;
        string title ;
        PdfReader reader;
        string file_name;
        BackgroundWorker bgw;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Visible = false;           
        }

        public void recursive_search(IList<Dictionary<string, object>> ilist, TreeNode tnt)
        {
            foreach (Dictionary<string, object> bk in ilist)
            {
                foreach (KeyValuePair<string, object> kvr in bk)
                {
                    if (kvr.Key == "Kids" || kvr.Key == "kids")
                    {
                        IList<Dictionary<string, object>> child = (IList<Dictionary<string, object>>)kvr.Value;
                        recursive_search(child, tn);
                    }
                    else if (kvr.Key == "Title" || kvr.Key == "title")
                    {
                        tn = new System.Windows.Forms.TreeNode(kvr.Value.ToString());
                        title = kvr.Value.ToString();                     
                    }
                    else if (kvr.Key == "Page" || kvr.Key == "page")
                    {
                        tn.ToolTipText = Regex.Match(kvr.Value.ToString(), "[0-9]+").Value;
                        tnt.Nodes.Add(tn);
                        ProgressChanged(title, kvr.Value.ToString());
                    }
                }
            }
        }
        
   

        public void ProgressChanged(object sender, object value)
        {
            label1.Invoke((MethodInvoker)(() => label1.Text = "Extracting " + sender + "   page number " + value));
        }

        void bgw_DoWork(string reader_name)
        {
            reader = new iTextSharp.text.pdf.PdfReader(reader_name);
           
            IList<Dictionary<string, object>> book_mark = SimpleBookmark.GetBookmark(reader);

            foreach (Dictionary<string, object> bk in book_mark)
            {

                foreach (KeyValuePair<string, object> kvr in bk)
                {
                    if (kvr.Key == "Kids" || kvr.Key == "kids")
                    {
                        IList<Dictionary<string, object>> child = (IList<Dictionary<string, object>>)kvr.Value;
                        treeView1.Invoke((MethodInvoker)(() => recursive_search(child, tn)));
                    }

                    else if (kvr.Key == "Title" || kvr.Key == "title")
                    {
                        tn = new System.Windows.Forms.TreeNode(kvr.Value.ToString());
                        title = kvr.Value.ToString();

                    }
                    else if (kvr.Key == "Page" || kvr.Key == "page")
                    {
                        //saves page number
                        tn.ToolTipText = Regex.Match(kvr.Value.ToString(), "[0-9]+").Value;
                        treeView1.Invoke((MethodInvoker)(() => treeView1.Nodes.Add(tn)));
                        ProgressChanged(title, kvr.Value.ToString());
                    }

                }
            }

            label1.Invoke((MethodInvoker)(() => label1.Text = "book_mark Extraction completed"));
            show_book_mark.Invoke((MethodInvoker)(() => show_book_mark.Enabled = true));
            progressBar1.Invoke((MethodInvoker)(() => progressBar1.Visible = false));
        }

        private void show_book_mark_Click(object sender, EventArgs e)
        {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    file_name = ofd.FileName;
                }
                else
                    return;

              label1.Text = "book_mark Extraction started";
              show_book_mark.Enabled = false;
              progressBar1.Visible = true;
              bgw = new BackgroundWorker();

              //backgroundworker initialization
              bgw= new BackgroundWorker();
              bgw.DoWork += ((obj, ep) => bgw_DoWork(file_name));
              bgw.ProgressChanged += ((obj, ep) => ProgressChanged(obj, ep));
              bgw.WorkerReportsProgress = true;
              bgw.WorkerSupportsCancellation = true;
              bgw.RunWorkerAsync();
            
        }
 
    }
}
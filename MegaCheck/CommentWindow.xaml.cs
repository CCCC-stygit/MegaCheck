using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MegaCheck
{
    /// <summary>
    /// Interaction logic for CommentWindow.xaml
    /// </summary>
    public partial class CommentWindow : Window
    {
        //private MainWindow.CheckItem check;

        public bool commentChanged { get; set; }
        public string comment { get; set; }


        public CommentWindow(MainWindow.CheckItem check)
        {
            InitializeComponent();

            if (check.checkComment == null)
                txt_title.Text = $"Add comment to check item: {check.checkName}";
            else
                txt_title.Text = $"Edit comment for check item: {check.checkName}";
            comment = check.checkComment;
            txt_comment.Text = comment;
            commentChanged = false;

            txt_comment.Focus();
        }


        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            comment = txt_comment.Text;
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txt_comment_TextChanged(object sender, TextChangedEventArgs e)
        {
            commentChanged = true;
        }
    }
}

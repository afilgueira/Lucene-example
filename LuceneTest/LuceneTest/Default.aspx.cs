namespace LuceneTest
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The default.
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {
        /// <summary>
        /// The index.
        /// </summary>
        private LuceneIndex index;

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.index == null)
            {
                this.index = new LuceneIndex();
                var file = new StreamReader(Request.PhysicalApplicationPath + "test_index.txt");
                var content = file.ReadToEnd().Replace("\n", string.Empty).Replace("\r", string.Empty).Split(new[] { '*' });
                this.index.Index(content.ToList());
            }
        }

        /// <summary>
        /// The btn_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void BtnClick(object sender, EventArgs e)
        {
            this.lblTest.Text = string.Empty;
            foreach (var s in this.index.Search(this.tbText.Text))
            {
                this.lblTest.Text += s + "<br><br>";
            }
        }
    }
}

namespace LuceneTest
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Snowball;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Store;

    /// <summary>
    /// The lucene index.
    /// </summary>
    public class LuceneIndex
    {
        /// <summary>
        /// The content field.
        /// </summary>
        private const string CONTENT = "Content";

        /// <summary>
        /// The directory.
        /// </summary>
        private Lucene.Net.Store.Directory directory;

        /// <summary>
        /// The analyzer.
        /// </summary>
        private Analyzer analyzer;

        /// <summary>
        /// The searcher.
        /// </summary>
        private IndexSearcher searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneIndex"/> class.
        /// </summary>
        public LuceneIndex()
        {
            // this.directory = FSDirectory.GetDirectory(new DirectoryInfo("d:\\index.dx"), true);
            this.directory = new RAMDirectory();
            this.analyzer = new SnowballAnalyzer("Spanish", StandardAnalyzer.STOP_WORDS);

            if (!IndexReader.IndexExists(this.directory))
            {
                var indexWriter = new IndexWriter(this.directory, this.analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                indexWriter.Close();
            }

            this.NewSearcher();
        }

        /// <summary>
        /// The new searcher.
        /// </summary>
        private void NewSearcher()
        {
            lock (this)
            {
                if (this.searcher != null)
                {
                    this.searcher.Close();
                }
                this.searcher = new IndexSearcher(this.directory, true);
            }
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        public void Index(List<string> content)
        {
            var indexWriter = new IndexWriter(this.directory, this.analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            foreach (var c in content)
            {
                var document = new Document();
                document.Add(new Field(CONTENT, c, Field.Store.YES, Field.Index.ANALYZED));
                indexWriter.AddDocument(document);
            }

            indexWriter.Close();
            this.NewSearcher();
        }

        /// <summary>
        /// The search.
        /// </summary>
        /// <param name="search">
        /// The search.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; System.String].
        /// </returns>
        public List<string> Search(string search)
        {
            var result = new List<string>();

            var phraseQuery = new PhraseQuery();
            var booleanQuery = new BooleanQuery();
            var ts = this.analyzer.TokenStream(null, new StringReader(search));
            var token = new Token();

            while ((token = ts.Next(token)) != null)
            {
                var term = new Term(CONTENT, token.Term());
                booleanQuery.Add(new TermQuery(term), BooleanClause.Occur.SHOULD);
                phraseQuery.Add(term);
            }

            var query = new BooleanQuery(true);
            query.Add(phraseQuery, BooleanClause.Occur.SHOULD);
            query.Add(booleanQuery, BooleanClause.Occur.SHOULD);

            var docs = this.searcher.Search(query, 1000);
            
            for (var i = 0; i < docs.totalHits; i++)
            {
                var text = this.searcher.Doc(docs.scoreDocs[i].doc, new MapFieldSelector(new[] { CONTENT })).Get(CONTENT);
                var score = this.searcher.Explain(query, docs.scoreDocs[i].doc).GetValue().ToString(CultureInfo.InvariantCulture);
                result.Add(text + " (" + score + ")");
            }

            return result;
        }
    }
}

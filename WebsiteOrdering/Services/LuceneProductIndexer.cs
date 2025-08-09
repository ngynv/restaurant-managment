using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.IO;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Services
{
    public class LuceneProductIndexer 
    {
        private readonly string _luceneDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "lucene_index");
        private readonly LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;

        public void CreateIndex(IEnumerable<Monan> products)
        {
            var dir = FSDirectory.Open(_luceneDir);
            var analyzer = new StandardAnalyzer(_luceneVersion);
            var indexConfig = new IndexWriterConfig(_luceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);
            writer.DeleteAll(); // Xóa chỉ mục cũ
            
            foreach (var product in products)
            {
                var doc = new Document
                {
                    new StringField("Id", product.Idmonan, Field.Store.YES),
                    new TextField("Tenmonan", product.Tenmonan ?? "", Field.Store.YES)
                };
                writer.AddDocument(doc);
            }
            writer.Flush(triggerMerge: false, applyAllDeletes: false);
            writer.Commit();
        }

        public List<string> Search(string queryText)
        {
            var dir = FSDirectory.Open(_luceneDir);
            var analyzer = new StandardAnalyzer(_luceneVersion);
            using var reader = DirectoryReader.Open(dir);
            var searcher = new IndexSearcher(reader);
            var parser = new QueryParser(_luceneVersion, "Tenmonan", analyzer);
            var query = parser.Parse(queryText);
            var hits = searcher.Search(query, 10).ScoreDocs;
            
            return hits.Select(hit =>
            {
                var foundDoc = searcher.Doc(hit.Doc);
                return foundDoc.Get("Id");
            }).ToList();
        }

        // Phương thức mới: Tìm kiếm fuzzy với độ chính xác có thể điều chỉnh
        public List<string> FuzzySearch(string queryText, int maxEdits = 2)
        {
            var dir = FSDirectory.Open(_luceneDir);
            using var reader = DirectoryReader.Open(dir);
            var searcher = new IndexSearcher(reader);
            
            // Tạo FuzzyQuery với maxEdits (số ký tự khác biệt tối đa)
            var fuzzyQuery = new FuzzyQuery(new Term("Tenmonan", queryText), maxEdits);
            var hits = searcher.Search(fuzzyQuery, 10).ScoreDocs;
            
            return hits.Select(hit =>
            {
                var foundDoc = searcher.Doc(hit.Doc);
                return foundDoc.Get("Id");
            }).ToList();
        }

        // Phương thức kết hợp: Tìm kiếm chính xác trước, sau đó fuzzy search
        public List<string> SmartSearch(string queryText, int maxResults = 10)
        {
            var dir = FSDirectory.Open(_luceneDir);
            var analyzer = new StandardAnalyzer(_luceneVersion);
            using var reader = DirectoryReader.Open(dir);
            var searcher = new IndexSearcher(reader);
            
            // Tạo BooleanQuery để kết hợp nhiều loại tìm kiếm
            var booleanQuery = new BooleanQuery();
            
            // 1. Tìm kiếm chính xác (độ ưu tiên cao nhất)
            var parser = new QueryParser(_luceneVersion, "Tenmonan", analyzer);
            var exactQuery = parser.Parse(queryText);
            booleanQuery.Add(exactQuery, Occur.SHOULD);
            
            // 2. Tìm kiếm với wildcard (ví dụ: "bun*")
            var wildcardQuery = new WildcardQuery(new Term("Tenmonan", queryText + "*"));
            booleanQuery.Add(wildcardQuery, Occur.SHOULD);
            
            // 3. Tìm kiếm fuzzy (độ ưu tiên thấp nhất)
            var fuzzyQuery = new FuzzyQuery(new Term("Tenmonan", queryText), 2);
            booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
            
            var hits = searcher.Search(booleanQuery, maxResults).ScoreDocs;
            
            return hits.Select(hit =>
            {
                var foundDoc = searcher.Doc(hit.Doc);
                return foundDoc.Get("Id");
            }).ToList();
        }

        // Phương thức trả về kết quả chi tiết với điểm số
        public List<SearchResult> SearchWithScore(string queryText, int maxResults = 10)
        {
            var dir = FSDirectory.Open(_luceneDir);
            var analyzer = new StandardAnalyzer(_luceneVersion);
            using var reader = DirectoryReader.Open(dir);
            var searcher = new IndexSearcher(reader);
            
            var booleanQuery = new BooleanQuery();
            var parser = new QueryParser(_luceneVersion, "Tenmonan", analyzer);
            var exactQuery = parser.Parse(queryText);
            booleanQuery.Add(exactQuery, Occur.SHOULD);
            
            var wildcardQuery = new WildcardQuery(new Term("Tenmonan", queryText + "*"));
            booleanQuery.Add(wildcardQuery, Occur.SHOULD);
            
            var fuzzyQuery = new FuzzyQuery(new Term("Tenmonan", queryText), 2);
            booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
            
            var hits = searcher.Search(booleanQuery, maxResults).ScoreDocs;
            
            return hits.Select(hit =>
            {
                var foundDoc = searcher.Doc(hit.Doc);
                return new SearchResult
                {
                    Id = foundDoc.Get("Id"),
                    Name = foundDoc.Get("Tenmonan"),
                    Score = hit.Score
                };
            }).ToList();
        }
    }

    // Lớp hỗ trợ để trả về kết quả tìm kiếm với điểm số
    public class SearchResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Score { get; set; }
    }
}
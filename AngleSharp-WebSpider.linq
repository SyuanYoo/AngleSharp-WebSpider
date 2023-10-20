<Query Kind="Program">
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>System.Linq</NuGetReference>
  <Namespace>AngleSharp</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//爬蟲資料來源：半夏小說
private const string MainUrl = "https://www.xbanxia.com";
//書籍代碼
private const string BookId = "";
//章節數量
private const int GetChapterCount = 5;

public class Book
{
	public string Title { get; set; }
	public string Author { get; set; }
	public List<Chapter> Chapters { get; set; }
}

public class Chapter
{
	public string Title { get; set; }
	public string Link { get; set; }
}

static async Task Main()
{
	//設定 AngleSharp 的配置
	var config = AngleSharp.Configuration.Default.WithDefaultLoader();

	//建立瀏覽器物件
	var browser = BrowsingContext.New(config);

	//取得小說作者、書名、章節資訊
	Book book = await GenerateBook(browser);
	
	//匯出檔案
	await Export(browser, book, GetChapterCount);
}

static async Task<IDocument> GetDocument(IBrowsingContext browser, string url)
{
	//使用該瀏覽器物件讀取特定的 URL
	var document = await browser.OpenAsync(new Url(url));
	
	document.Close();

	return document;
}

static async Task<Book> GenerateBook(IBrowsingContext browser)
{
	//使用Selector取的網頁元素
	//可以使用網頁F12開發人員工具>於Element右鍵>Copy>Copy Selector
	Book book = new Book();
	var document = await GetDocument(browser, $"{MainUrl}/books/{BookId}.html");
	book.Title = document.QuerySelector("#content-list > div.book-intro.clearfix > div.book-describe > h1")?.InnerHtml;
	book.Author = document.QuerySelector("#content-list > div.book-intro.clearfix > div.book-describe > p:nth-child(2) > a")?.InnerHtml;

	List<Chapter> chapters = new List<Chapter>();
	var chaptersElement = document.QuerySelectorAll("#content-list > div.book-list.clearfix > ul > li a");
	foreach (var i in chaptersElement)
	{
		chapters.Add(new Chapter
		{
			Title = i.GetAttribute("title"),
			Link = i.GetAttribute("href"),
		});
	}

	book.Chapters = chapters;
	
	return book;
}

static async Task Export(IBrowsingContext browser, Book book, int getChapterCount)
{
	foreach (var i in book.Chapters)
	{
		if (getChapterCount == 0) break;

		string filePath = $"D:\\{book.Author}\\{book.Title}\\{i.Title}";
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));

		var chapterDocument = await GetDocument(browser, $"{MainUrl}{i.Link}");
		var chapter = chapterDocument.QuerySelector("#nr1").InnerHtml.Replace("<br>", string.Empty);

		System.IO.File.WriteAllText(filePath, chapter);

		getChapterCount--;
	}
}


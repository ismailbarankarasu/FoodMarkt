using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using FoodMart.Entities;
using FoodMart.Services;
using MongoDB.Bson;
using MongoDB.Driver;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var databaseName = "FoodMart_Verification_" + Guid.NewGuid().ToString("N");
var mongo = new MongoClient("mongodb://localhost:27017");
var database = mongo.GetDatabase(databaseName);
var password = Guid.NewGuid().ToString("N") + "!aA1";
var start = new ProcessStartInfo("dotnet", "bin/Debug/net10.0/FoodMart.dll")
{
    WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true,
    RedirectStandardOutput = true, RedirectStandardError = true
};
start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
start.Environment["ASPNETCORE_URLS"] = "http://localhost:5189";
start.Environment["MongoDbSettings__ConnectionString"] = "mongodb://localhost:27017";
start.Environment["MongoDbSettings__DatabaseName"] = databaseName;
start.Environment["AdminSeed__FullName"] = "Doğrulama Yöneticisi";
start.Environment["AdminSeed__Email"] = "verification@example.test";
start.Environment["AdminSeed__Password"] = password;
start.Environment["EmailSettings__SenderEmail"] = ""; // Never send real email during verification.
using var process = Process.Start(start)!;
_ = process.StandardOutput.ReadToEndAsync();
_ = process.StandardError.ReadToEndAsync();
using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
using var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5189") };
var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAILED: " + name);
    Console.WriteLine("PASS: " + name);
    checks++;
}
async Task<string> Get(string path)
{
    var response = await http.GetAsync(path);
    Check(response.StatusCode == HttpStatusCode.OK, "GET " + path);
    return await response.Content.ReadAsStringAsync();
}
async Task<HttpResponseMessage> Post(string path, Dictionary<string, string> fields, string? formPath = null, byte[]? image = null, string filename = "test.png")
{
    var html = await Get(formPath ?? path);
    fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    if (image is null) return await http.PostAsync(path, new FormUrlEncodedContent(fields));
    using var content = new MultipartFormDataContent();
    foreach (var pair in fields) content.Add(new StringContent(pair.Value), pair.Key);
    var file = new ByteArrayContent(image);
    file.Headers.ContentType = new("image/png");
    content.Add(file, "ImageFile", filename);
    return await http.PostAsync(path, content);
}
try
{
    for (var attempt = 0; attempt < 60; attempt++)
    {
        if (process.HasExited) throw new Exception("Application exited before becoming ready.");
        try { if ((await http.GetAsync("/Admin/Account/Login")).IsSuccessStatusCode) break; }
        catch (HttpRequestException) { }
        await Task.Delay(500);
    }
    foreach (var controller in new[] { "Dashboard", "Category", "Product", "Feature", "Discount", "Sale", "Subscriber" })
    {
        var response = await http.GetAsync("/Admin/" + controller);
        Check(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location!.ToString().Contains("/Admin/Account/Login"), controller + " anonymous redirect");
    }
    var noCsrf = await http.PostAsync("/Admin/Account/Login", new FormUrlEncodedContent(new Dictionary<string,string>()));
    Check(noCsrf.StatusCode == HttpStatusCode.BadRequest, "Login CSRF protection");
    var invalid = await Post("/Admin/Account/Login", new() { ["Email"] = "bad", ["Password"] = "" });
    Check(invalid.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync()).Contains("Geçerli bir e-posta"), "Turkish login validation");
    var wrong = await Post("/Admin/Account/Login", new() { ["Email"] = "verification@example.test", ["Password"] = "wrong" });
    Check(wrong.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await wrong.Content.ReadAsStringAsync()).Contains("şifre hatalı"), "Wrong password rejected");
    var login = await Post("/Admin/Account/Login", new() { ["Email"] = "verification@example.test", ["Password"] = password });
    Check(login.StatusCode == HttpStatusCode.Redirect && login.Headers.Location!.ToString() == "/Admin", "Successful login redirects to Dashboard");
    Check(login.Headers.GetValues("Set-Cookie").Any(x => x.Contains("httponly", StringComparison.OrdinalIgnoreCase)), "HttpOnly authentication cookie");
    Check((await http.GetAsync("/Admin/Account/Login")).StatusCode == HttpStatusCode.Redirect, "Authenticated login redirects");
    var admin = await database.GetCollection<AdminUser>("AdminUsers").Find(_ => true).SingleAsync();
    Check(admin.PasswordHash != password && new Microsoft.AspNetCore.Identity.PasswordHasher<AdminUser>().VerifyHashedPassword(admin, admin.PasswordHash, password) != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, "Seed password is hashed");
    await new MongoIndexService(database).InitializeAsync();
    Check((await database.GetCollection<AdminUser>("AdminUsers").Indexes.ListAsync()).ToList().Any(x => x.GetValue("unique", false).ToBoolean()), "Unique indexes and repeat initialization");
    var badCategory = await Post("/Admin/Category/CreateCategory", new() { ["Name"] = "", ["Icon"] = "" });
    Check(badCategory.StatusCode == HttpStatusCode.OK && await database.GetCollection<Category>("Categories").CountDocumentsAsync(_ => true) == 0, "Invalid category not saved");
    Check((await Post("/Admin/Category/CreateCategory", new() { ["Name"] = "Doğrulama", ["Icon"] = "bi bi-basket" })).StatusCode == HttpStatusCode.Redirect, "Create category");
    var category = await database.GetCollection<Category>("Categories").Find(_ => true).SingleAsync();
    Check((await http.GetAsync("/Admin/Category/DeleteCategory/" + category.Id)).StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotFound, "Category deletion rejects GET");
    var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aD1sAAAAASUVORK5CYII=");
    var productFields = new Dictionary<string,string> { ["Name"] = "Test ürün", ["CategoryId"] = category.Id, ["Price"] = "10", ["Stock"] = "5", ["IsActive"] = "true", ["Description"] = "Doğrulama" };
    var invalidPrice = new Dictionary<string, string>(productFields) { ["Price"] = "-1", ["DiscountPrice"] = "20", ["Stock"] = "-1", ["ImageUrl"] = "/test.png" };
    var invalidProduct = await Post("/Admin/Product/CreateProduct", invalidPrice);
    Check(invalidProduct.StatusCode == HttpStatusCode.OK && await database.GetCollection<Product>("Products").CountDocumentsAsync(_ => true) == 0, "Invalid product price, discount and stock rejected");
    var missingCategory = new Dictionary<string, string>(productFields) { ["CategoryId"] = ObjectId.GenerateNewId().ToString(), ["ImageUrl"] = "/test.png" };
    Check((await Post("/Admin/Product/CreateProduct", missingCategory)).StatusCode == HttpStatusCode.OK && await database.GetCollection<Product>("Products").CountDocumentsAsync(_ => true) == 0, "Nonexistent category rejected");
    Check((await Post("/Admin/Product/CreateProduct", new(productFields), image: new byte[5 * 1024 * 1024 + 1])).StatusCode == HttpStatusCode.OK, "Oversized image rejected");
    Check((await Post("/Admin/Product/CreateProduct", new(productFields), image: png, filename: "test.svg")).StatusCode == HttpStatusCode.OK, "Unsupported extension rejected");
    var badFile = await Post("/Admin/Product/CreateProduct", new(productFields), image: "not an image"u8.ToArray());
    Check(badFile.StatusCode == HttpStatusCode.OK && await database.GetCollection<Product>("Products").CountDocumentsAsync(_ => true) == 0, "Invalid upload signature rejected");
    Check((await Post("/Admin/Product/CreateProduct", new(productFields), image: png, filename: "../../test.png")).StatusCode == HttpStatusCode.Redirect, "Product image upload");
    var product = await database.GetCollection<Product>("Products").Find(_ => true).SingleAsync();
    Check(product.ImageUrl.StartsWith("/uploads/images/") && (await http.GetAsync(product.ImageUrl)).StatusCode == HttpStatusCode.OK, "Generated image URL served");
    productFields["Id"] = product.Id;
    Check((await Post("/Admin/Product/UpdateProduct", new(productFields), "/Admin/Product/UpdateProduct/" + product.Id)).StatusCode == HttpStatusCode.Redirect, "Update product without replacement");
    Check((await database.GetCollection<Product>("Products").Find(x => x.Id == product.Id).SingleAsync()).ImageUrl == product.ImageUrl, "Existing image preserved");
    var oldImage = product.ImageUrl;
    Check((await Post("/Admin/Product/UpdateProduct", new(productFields), "/Admin/Product/UpdateProduct/" + product.Id, png)).StatusCode == HttpStatusCode.Redirect, "Image replacement");
    product = await database.GetCollection<Product>("Products").Find(x => x.Id == product.Id).SingleAsync();
    Check(product.ImageUrl != oldImage && !File.Exists(Path.Combine(root, "wwwroot", oldImage.TrimStart('/'))), "Replaced image cleaned up");
    await database.GetCollection<Product>("Products").UpdateOneAsync(x => x.Id == product.Id,
        Builders<Product>.Update.Set(x => x.Price, 1234.5m).Set(x => x.DiscountPrice, 1000.25m));
    foreach (var entity in new[] { "Feature", "Discount", "Sale" })
    {
        var fields = entity switch
        {
            "Feature" => new Dictionary<string,string> { ["Title"] = "Test başlık", ["Description"] = "Test", ["ButtonText"] = "İncele", ["ButtonUrl"] = "/Product", ["Order"] = "0", ["ImageUrl"] = product.ImageUrl, ["IsActive"] = "true" },
            "Discount" => new Dictionary<string,string> { ["Title"] = "Test kampanya", ["Description"] = "Test", ["ProductId"] = product.Id, ["DiscountRate"] = "20", ["StartDate"] = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"), ["EndDate"] = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"), ["ImageUrl"] = product.ImageUrl, ["IsActive"] = "true" },
            _ => new Dictionary<string,string> { ["ProductId"] = product.Id, ["Quantity"] = "2", ["UnitPrice"] = "10", ["TotalPrice"] = "999", ["SaleDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd") }
        };
        Check((await Post($"/Admin/{entity}/Create{entity}", new())).StatusCode == HttpStatusCode.OK, entity + " invalid form stays on page");
        Check((await Post($"/Admin/{entity}/Create{entity}", fields)).StatusCode == HttpStatusCode.Redirect, "Create " + entity);
        var document = await database.GetCollection<BsonDocument>(entity == "Sale" ? "Sales" : entity + "s").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        var id = document["_id"].ToString()!;
        fields["Id"] = id;
        Check((await Post($"/Admin/{entity}/Update{entity}", fields, $"/Admin/{entity}/Update{entity}/{id}")).StatusCode == HttpStatusCode.Redirect, "Update " + entity);
    }
    Check((await database.GetCollection<Sale>("Sales").Find(_ => true).SingleAsync()).TotalPrice == 20, "Sale total ignores client TotalPrice");
    foreach (var path in new[] { "/", "/Product", "/Product/Search?search=Test", "/Product/Category/" + category.Id, "/Product/Detail/" + product.Id, "/Admin/Dashboard", "/Admin/Subscriber" }) await Get(path);
    var homepage = WebUtility.HtmlDecode(await Get("/"));
    foreach (var anchor in new[] { "home", "categories", "products", "discounts", "popular-products", "contact" })
        Check(Regex.Matches(homepage, $"id=\"{anchor}\"").Count == 1, "Unique homepage anchor: " + anchor);
    foreach (var path in new[] { "/", "/Product", "/Product/Search?search=Test", "/Product/Category/" + category.Id, "/Product/Detail/" + product.Id })
    {
        var page = WebUtility.HtmlDecode(await Get(path));
        Check(page.Contains("<html lang=\"tr\">") && Regex.IsMatch(page, "</head>\\s*<body") && !page.Contains(";;") && !page.Contains("</main>;"), "Turkish layout without stray text: " + path);
        Check(new[] { "categories", "products", "discounts", "popular-products", "contact" }.All(anchor => page.Contains($"href=\"/#{anchor}\"")), "Cross-page homepage links: " + path);
        Check(!page.Contains("href=\"#\"") && !page.Contains("Sepete Ekle") && !page.Contains("Favorilere Ekle") && !page.Contains("Lorem ipsum") && !page.Contains("Free HTML Template"), "No dead controls or theme text: " + path);
        Check(page.Contains("1.234,50") && page.Contains("1.000,25"), "Turkish regular and discounted prices: " + path);
        foreach (var src in Regex.Matches(page, "<img[^>]+src=\"([^\"]+)\"").Select(x => x.Groups[1].Value).Where(x => x.StartsWith('/')).Distinct())
            Check((await http.GetAsync(src)).StatusCode == HttpStatusCode.OK, "Rendered image exists: " + src);
    }
    Check(homepage.Contains("TemplatesJungle") && homepage.Contains($"© {DateTime.Now.Year} FoodMart"), "Footer attribution and current copyright year");
    Check(homepage.Contains("id=\"mobileSearch\" name=\"search\" type=\"search\"") && homepage.Contains("action=\"/Product/Search\""), "Mobile search uses product search route");
    Check((await http.GetAsync("/Home")).StatusCode == HttpStatusCode.Redirect, "Obsolete home page redirects to storefront");
    await Get("/Home/Privacy");
    var emptyCategory = new Category { Name = "Boş kategori", Icon = "/missing-image.png" };
    await database.GetCollection<Category>("Categories").InsertOneAsync(emptyCategory);
    Check(WebUtility.HtmlDecode(await Get("/Product/Category/" + emptyCategory.Id)).Contains("Boş kategori"), "Empty category keeps its actual name");
    await database.GetCollection<Category>("Categories").DeleteOneAsync(x => x.Id == emptyCategory.Id);
    Check((await http.GetAsync("/Product/Search?categoryId=invalid")).StatusCode == HttpStatusCode.NotFound, "Malformed search category is rejected");
    Check((await http.GetAsync("/Product/Category/" + ObjectId.GenerateNewId())).StatusCode == HttpStatusCode.NotFound, "Unknown category returns 404");
    await database.GetCollection<Product>("Products").UpdateOneAsync(x => x.Id == product.Id, Builders<Product>.Update.Set(x => x.IsActive, false));
    Check((await http.GetAsync("/Product/Detail/" + product.Id)).StatusCode == HttpStatusCode.NotFound, "Inactive product detail is hidden");
    var inactiveHome = WebUtility.HtmlDecode(await Get("/"));
    Check(!inactiveHome.Contains("Test kampanya") && !inactiveHome.Contains("Test ürün") && inactiveHome.Contains("id=\"discounts\"") && inactiveHome.Contains("id=\"popular-products\""), "Inactive product and campaign hidden; empty section anchors remain");
    await database.GetCollection<Product>("Products").UpdateOneAsync(x => x.Id == product.Id, Builders<Product>.Update.Set(x => x.IsActive, true));
    var dashboard = WebUtility.HtmlDecode(await Get("/Admin/Dashboard"));
    Check(dashboard.Contains("verification@example.test") && dashboard.Contains("dashboardSalesData"), "Claims header and real dashboard JSON");
    Check((await http.GetAsync("/Product/Detail/not-an-id")).StatusCode == HttpStatusCode.NotFound, "Malformed ObjectId returns 404");
    var subscription = await Post("/Subscribe/Create", new() { ["FullName"] = "", ["Email"] = "invalid" }, "/");
    Check(subscription.StatusCode == HttpStatusCode.Redirect && await database.GetCollection<Subscriber>("Subscribers").CountDocumentsAsync(_ => true) == 0, "Invalid subscriber rejected");
    var mailFailure = await Post("/Subscribe/Create", new() { ["FullName"] = "Test", ["Email"] = "test@example.test" }, "/");
    Check(mailFailure.StatusCode == HttpStatusCode.Redirect && await database.GetCollection<Subscriber>("Subscribers").CountDocumentsAsync(_ => true) == 0, "Email failure rolls back subscription");
    Check(WebUtility.HtmlDecode(await Get("/")).Contains("gönderilirken bir sorun"), "Friendly email failure message");
    var subscriberService = new FoodMart.Services.SubscriberServices.SubscriberService(database);
    var subscriber = await subscriberService.CreateAsync(new() { FullName = "Test", Email = "TEST@example.test" });
    Check(subscriber.Email == "test@example.test" && subscriber.DiscountCode.StartsWith("FOOD25-") && subscriber.ExpiresAt > DateTime.UtcNow, "Subscriber code and normalization");
    try
    {
        await subscriberService.CreateAsync(new() { FullName = "Duplicate", Email = "test@example.test" });
        Check(false, "Unique subscriber email enforced");
    }
    catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
    { Check(true, "Unique subscriber email enforced"); }
    await Get("/Admin/Subscriber/Detail/" + subscriber.Id);
    Check((await Post("/Admin/Subscriber/DeleteSubscriber/" + subscriber.Id, new(), "/Admin/Subscriber")).StatusCode == HttpStatusCode.Redirect, "Delete subscriber");
    Check((await http.PostAsync("/Admin/Category/DeleteCategory/" + category.Id, new FormUrlEncodedContent(new Dictionary<string,string>()))).StatusCode == HttpStatusCode.BadRequest, "Delete CSRF protection");
    foreach (var entity in new[] { "Feature", "Discount", "Sale", "Product", "Category" })
    {
        var collection = entity == "Category" ? "Categories" : entity + "s";
        var record = await database.GetCollection<BsonDocument>(collection).Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Check((await Post($"/Admin/{entity}/Delete{entity}/{record["_id"]}", new(), $"/Admin/{entity}")).StatusCode == HttpStatusCode.Redirect, "Delete " + entity);
    }
    Check(!File.Exists(Path.Combine(root, "wwwroot", product.ImageUrl.TrimStart('/'))), "Unreferenced uploaded image cleaned up");
    Check((await Post("/Admin/Account/Logout", new(), "/Admin/Dashboard")).StatusCode == HttpStatusCode.Redirect, "POST logout");
    Check((await http.GetAsync("/Admin/Dashboard")).Headers.Location!.ToString().Contains("/Admin/Account/Login"), "Dashboard protected after logout");
    await process.KillAndWaitAsync();
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    using var production = Process.Start(start)!;
    _ = production.StandardOutput.ReadToEndAsync();
    _ = production.StandardError.ReadToEndAsync();
    try
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            try { if ((await http.GetAsync("/Home/Error")).IsSuccessStatusCode) break; }
            catch (HttpRequestException) { }
            await Task.Delay(500);
        }
        Check(await database.GetCollection<AdminUser>("AdminUsers").CountDocumentsAsync(_ => true) == 1, "Restart does not recreate admin");
        // Deliberately malformed data only in the disposable DB exercises the real production exception pipeline.
        await database.GetCollection<BsonDocument>("Products").InsertOneAsync(new BsonDocument { ["_id"] = ObjectId.GenerateNewId(), ["Price"] = "invalid decimal" });
        var failure = await http.GetAsync("/Product");
        var errorPage = WebUtility.HtmlDecode(await failure.Content.ReadAsStringAsync());
        Check(failure.StatusCode == HttpStatusCode.InternalServerError && errorPage.Contains("Bir sorun oluştu") && !errorPage.Contains("FormatException") && !errorPage.Contains("StackTrace"), "Production exception uses friendly standalone error page");
    }
    finally { await production.KillAndWaitAsync(); }
    Console.WriteLine($"Completed {checks} checks.");
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    Environment.ExitCode = 1;
}
finally
{
    if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(); }
    // This name is generated above and cannot target the application's configured database.
    await mongo.DropDatabaseAsync(databaseName);
}

static class ProcessExtensions
{
    public static async Task KillAndWaitAsync(this Process process)
    {
        if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(); }
    }
}

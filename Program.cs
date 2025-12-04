using System.Linq;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello Masi!");

app.MapGet("/hello", () => "Hello from /hello endpoint");

app.MapGet("/time", () => DateTime.UtcNow);

app.MapGet("/info", () => new { name = "Minimal API", version = "1.0" });

app.MapGet("/color/{favorite_color}", (string favorite_color) => $"Your favorite color is: {favorite_color}");

app.MapGet("/order/{order_number:int}", (int order_number) => $"Order number {order_number} is being processed.");

app.MapGet("/book/{isbn}", (string isbn) => new { isbn, title = "API Basics", available = true });

app.MapPost("/api/support", (string body) =>
{
    return Results.Created("/api/support", $"Support request received: {body}");
});

app.MapPost("/api/movies", (MovieDetails new_movie) =>
{
    return Results.Created($"/api/movies/{new_movie.Year}", new_movie);
});

var book_list = new List<Book>
{
    new Book { Id = 1, Title = "Harry Potter", Pages = 120 },
    new Book { Id = 2, Title = "James and Giant Beach", Pages = 255 },
    new Book { Id = 3, Title = "Design Patterns", Pages = 505 },
};

app.MapPost("/api/books", (Book book) =>
{
    var ctx = new ValidationContext(book);
    var results = new List<ValidationResult>();

    bool isValid = Validator.TryValidateObject(book, ctx, results, true);

    if (!isValid)
    {
        return Results.BadRequest(results);
    }

    book_list.Add(book);
    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapGet("/api/books/title/{title}", (string title) =>
{
    foreach (var item in book_list)
    {
        if (item.Title != null && item.Title.ToLower().Contains(title.ToLower()))
        {
            return Results.Ok(item);
        }
    }
    return Results.NotFound();
});

app.MapGet("/api/books", () => book_list);

app.MapPut("/api/books/{id}", (int id, Book updated) =>
{
    for (int index = 0; index < book_list.Count; index++)
    {
        if (book_list[index].Id == id)
        {
            book_list[index] = updated;
            return Results.Ok(updated);
        }
    }
    return Results.NotFound();
});

app.MapDelete("/api/books/{id}", (int id) =>
{
    var item_to_remove = book_list.FirstOrDefault(p => p.Id == id);
    if (item_to_remove is null)
    {
        return Results.NotFound();
    }

    book_list.Remove(item_to_remove);
    return Results.NoContent();
});

app.Run();

public class MovieDetails
{
    public string Title { get; set; }
    public int Year { get; set; }
}

public class Book
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }
    
    [Range(1, 2000)]
    public int Pages { get; set; }
}
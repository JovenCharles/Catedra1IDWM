using ebooks_dotnet7_api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ebooks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var ebooks = app.MapGroup("api/ebook");

// TODO: Add more routes
ebooks.MapPost("/", CreateEBook);
ebooks.MapGet("/", GetAllEBooks);
ebooks.MapPut("/{id}", UpdateEBook);
ebooks.MapPut("/{id}/change-availability", ChangeAvailability);
ebooks.MapPut("/{id}/increment-stock", IncrementStock);
ebooks.MapPost("/{id}", PurchaseEBook);
ebooks.MapDelete("/{id}",DeleteEBook);

app.Run();

// TODO: Add more methods
static IResult CreateEBook(CreateEBookDto createEBookDto, DataContext db)
{
    var existTitle = db.EBooks.Where(e => e.Title == createEBookDto.Title).FirstOrDefault();
    var existAuthor = db.EBooks.Where(e => e.Author == createEBookDto.Author).FirstOrDefault();
    if(existTitle != null){
        return TypedResults.BadRequest("Ya existe un EBook con ese título.");
    }
    if(existAuthor != null){
        return TypedResults.BadRequest("Ya existe un EBook con ese autor.");
    }

    var ebook = new EBook
    {
        Title = createEBookDto.Title,
        Author = createEBookDto.Author,
        Genre = createEBookDto.Genre,
        Format = createEBookDto.Format,
        Price = createEBookDto.Price,
        Stock = 0,
        IsAvailable = true
    };

    db.EBooks.Add(ebook);
    db.SaveChanges();
    return TypedResults.Created($"/ebook/{ebook.Id}",ebook);
}

static IResult GetAllEBooks(DataContext db)
{
    var ebooks = db.EBooks.ToList();
    return TypedResults.Ok(ebooks);
}



//Aquí ocupé el CreateEBookDto porque necesito los mismos datos para enviar, asi que para no crear más DTO y me quedaba poco tiempo ;(
static IResult UpdateEBook(int id, CreateEBookDto createEBookDto, DataContext db){
    var existEbook = db.EBooks.Where(c => c.Id == id).FirstOrDefault();
    if(existEbook == null)
    {   
        return TypedResults.NotFound("El EBook para actualizar no existe.");
    }
        existEbook.Title = createEBookDto.Title;
        existEbook.Author = createEBookDto.Author;
        existEbook.Genre = createEBookDto.Genre;
        existEbook.Format = createEBookDto.Format;
        if(createEBookDto.Price > 0){
            existEbook.Price = createEBookDto.Price;
        }

    db.Entry(existEbook).State = EntityState.Modified;
    db.SaveChanges();

    return TypedResults.NoContent(); 
}

static IResult ChangeAvailability(DataContext db)
{
    return Results.Ok();
}

static IResult IncrementStock(int id, [FromBody] IncrementStockDto incrementStockDto, DataContext db)
{
    var existEbook = db.EBooks.Where(c => c.Id == id).FirstOrDefault();
    if(existEbook == null)
    {   
        return TypedResults.NotFound("El EBook no existe");
    }

    if(incrementStockDto.Stock <= 0){
        return TypedResults.BadRequest("Debe ser mayor a 0 el stock.");
    }

    existEbook.Stock += incrementStockDto.Stock;
    db.Entry(existEbook).State = EntityState.Modified;
    db.SaveChanges();

    return TypedResults.Ok(existEbook);
}

static IResult PurchaseEBook([FromBody] PurchaseEBookDto purchaseEBookDto,  DataContext db)
{
    var existEBook = db.EBooks.Where(c => c.Id == purchaseEBookDto.Id).FirstOrDefault();
    if(existEBook == null)
    {   
        return TypedResults.NotFound("La silla no existe");
    }
    if(purchaseEBookDto.amount <= 0){
        return TypedResults.BadRequest("Debes proporcionar los datos de la compra");
    }
    if(existEBook.Stock < purchaseEBookDto.amount){
        return TypedResults.BadRequest("No hay stock suficiente");
    }
    int total = existEBook.Price * purchaseEBookDto.amount;
    if(purchaseEBookDto.Payment != total){
        return TypedResults.BadRequest("El pago no es valido");
    }
    existEBook.Stock -= purchaseEBookDto.amount;
    db.Entry(existEBook).State = EntityState.Modified;
    db.SaveChanges();

    return TypedResults.Ok("Compra realizada con éxito");
}

static IResult DeleteEBook(int id, DataContext db)
{
    var existEBook = db.EBooks.Where(c => c.Id == id).FirstOrDefault();
    if(existEBook == null)
    {   
        return TypedResults.NotFound("El EBook que desea eliminar no existe.");
    }

    db.EBooks.Remove(existEBook);
    db.SaveChanges();

    return TypedResults.NoContent();

}
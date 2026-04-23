using NUnit.Framework;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class SimpleNotesTests
{
    private RestClient client;
    private static string token;
    private static string createdNoteId;

    [OneTimeSetUp]
    public void Setup()
    {
        client = new RestClient("http://144.91.123.158:5005");

        var request = new RestRequest("/api/User/Authorization", Method.Post);
        request.AddJsonBody(new
        {
            email = "daniel.bashev2003@abv.bg",
            password = "2100709dD.."
        });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = JsonDocument.Parse(response.Content);
        token = json.RootElement.GetProperty("accessToken").GetString();

        client.AddDefaultHeader("Authorization", $"Bearer {token}");
    }

    [Test, Order(1)]
    public void CreateNote_WithoutRequiredFields_ShouldFail()
    {
        var request = new RestRequest("/api/Note/Create", Method.Post);
        request.AddJsonBody(new { });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test, Order(2)]
    public void CreateNote_ShouldSucceed()
    {
        var request = new RestRequest("/api/Note/Create", Method.Post);

        request.AddJsonBody(new
        {
            title = "Test Note 123",
            description = "This is a valid description with more than 30 characters.",
            status = "New"
        });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
        Assert.That(result.Msg, Is.EqualTo("Note created successfully!"));
    }

    [Test, Order(3)]
    public void GetAllNotes_ShouldReturnList_AndStoreLastId()
    {
        var request = new RestRequest("/api/Note/AllNotes", Method.Get);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var notes = JsonSerializer.Deserialize<List<NoteDto>>(
            JsonDocument.Parse(response.Content)
            .RootElement
            .GetProperty("allNotes")
            .GetRawText()
        );

        Assert.That(notes, Is.Not.Null);
        Assert.That(notes.Count, Is.GreaterThan(0));

        createdNoteId = notes.Last().Id;

        Assert.That(createdNoteId, Is.Not.Null.And.Not.Empty);
    }

    [Test, Order(4)]
    public void EditNote_ShouldSucceed()
    {
        var request = new RestRequest($"/api/Note/Edit/{createdNoteId}", Method.Put);

        request.AddJsonBody(new
        {
            title = "Edited Note Title",
            description = "Edited description with more than 30 characters length.",
            status = "Done"
        });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
        Assert.That(result.Msg, Is.EqualTo("Note edited successfully!"));
    }

    [Test, Order(5)]
    public void DeleteNote_ShouldSucceed()
    {
        var request = new RestRequest($"/api/Note/Delete/{createdNoteId}", Method.Delete);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
        Assert.That(result.Msg, Is.EqualTo("Note deleted successfully!"));
    }
}

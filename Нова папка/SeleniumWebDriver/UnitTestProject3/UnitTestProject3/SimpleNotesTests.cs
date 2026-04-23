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
    public void CreateNote_ShouldSucceed()
    {
        var request = new RestRequest("/api/Note/Create", Method.Post);

        request.AddJsonBody(new
        {
            title = "Test Note",
            description = "Valid description longer than 30 characters.",
            status = "New"
        });

        var response = client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GetInvoice
{
    public struct Document
    {
        [JsonPropertyName("DocumentNumber")]
        public string DocumentNumber { get; set; }
        [JsonPropertyName("Phone")]
        public string Phone { get; set; }
    }

    public struct MethodProperties
    {
        [JsonPropertyName("Documents")]
        public Document[] Documents { get; set; }
    }

    public struct InvoiceRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
        [JsonPropertyName("modelName")]
        public string ModelName { get; set; }
        [JsonPropertyName("calledMethod")]
        public string CalledMethod { get; set; }
        [JsonPropertyName("methodProperties")]
        public MethodProperties MethodProperties { get; set; }
    }

    public struct InvoiceDTO
    {
        [JsonPropertyName("Number")]
        public string Number { get; set; }
        [JsonPropertyName("Status")]
        public string Status { get; set; }
    }

    public struct InvoiceRepsonse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("data")]
        public InvoiceDTO[] Data { get; set; }
        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }
    };

    public static class InvoiceHandler
    {
        static HttpClient client = new HttpClient();
        static string apiKey = File.ReadAllText(".apikey");
        static string urlBase = "https://api.novaposhta.ua/v2.0/json/";

        static InvoiceHandler()
        {
            client.BaseAddress = new Uri(urlBase);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<InvoiceRepsonse> RequestInvoiceAsync(string invoiceNumber)
        {

            var requestMessage = new HttpRequestMessage();

            InvoiceRequest r = new()
            {
                ApiKey = apiKey,
                ModelName = "TrackingDocument",
                CalledMethod = "getStatusDocuments",
                MethodProperties = new()
                {
                    Documents = new[]
                    {
                            new Document
                            {
                                DocumentNumber = invoiceNumber,
                                Phone = ""
                            }
                        }
                }
            };

            requestMessage.Method = HttpMethod.Get;
            requestMessage.Content = JsonContent.Create(r);

            var response = await client.SendAsync(requestMessage);
            var responseString = await response.Content.ReadAsStringAsync();

            return await response.Content.ReadFromJsonAsync<InvoiceRepsonse>();
        }
    }
}
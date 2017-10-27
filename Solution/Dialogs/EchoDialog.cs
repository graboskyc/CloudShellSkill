using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            string csun = "";
            string cspw = "";
            string csdm = "";

            var message = await argument;

            string _baseUri = "https://demo.quali.com:3443/";
            string _authUri = _baseUri + "api/login";
            string _startUri = _baseUri + "api/v2/blueprints/"+message.Text.ToString()+"/start";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_authUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string body = "{\"username\":\""+csun+"\",\"password\":\""+cspw+"\",\"domain\":\""+csdm+"\"}";
            StringContent sc = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(_authUri, sc);
            string token = await response.Content.ReadAsStringAsync();
            token = token.Replace("\"","");

            string toUser = "Successfully started sandbox for " + message.Text.ToString();
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(_startUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic " + token);
                body = "{\"duration\": \"PT30M\",\"name\": \"From Sandbox API with Cortana\",\"params\": [] }";
                sc = new StringContent(body, Encoding.UTF8, "application/json");
                response = await client.PostAsync(_startUri, sc);
                //if (response.StatusCode.ToString() != "200")
                //{
                //    body = await response.Content.ReadAsStringAsync();
                //    toUser = "Could not start sandbox: " + body;
                //}
            }
            catch (Exception ex)
            {
                toUser = "Could not start the sandbox: " + ex.ToString();
            }

            await context.PostAsync(toUser);
            context.Wait(MessageReceivedAsync);
        }
    }
}
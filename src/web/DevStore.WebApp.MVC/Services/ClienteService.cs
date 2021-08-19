using DevStore.Core.Communication;
using DevStore.WebApp.MVC.Extensions;
using DevStore.WebApp.MVC.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevStore.WebApp.MVC.Services
{
    public interface IClientService
    {
        Task<AddressViewModel> GetAddress();
        Task<ResponseResult> AddAddress(AddressViewModel address);
    }

    public class ClientService : Service, IClientService
    {
        private readonly HttpClient _httpClient;

        public ClientService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ClienteUrl);
        }

        public async Task<AddressViewModel> GetAddress()
        {
            var Response = await _httpClient.GetAsync("/clients/address/");

            if (Response.StatusCode == HttpStatusCode.NotFound) return null;

            ManageResponseErrors(Response);

            return await DeserializeResponse<AddressViewModel>(Response);
        }

        public async Task<ResponseResult> AddAddress(AddressViewModel address)
        {
            var enderecoContent = GetContent(address);

            var Response = await _httpClient.PostAsync("/clients/address/", enderecoContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
    }
}
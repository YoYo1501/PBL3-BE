using BackendAPI.Models.DTOs.Payment.Requests;
using BackendAPI.Models.DTOs.Payment.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IPaymentService
{
    string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
}

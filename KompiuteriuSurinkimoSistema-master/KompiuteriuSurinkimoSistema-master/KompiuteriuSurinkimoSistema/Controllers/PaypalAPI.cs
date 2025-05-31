using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ComputerBuildSystem.Controllers
{
    public class PaypalAPI
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PaypalAPI(IConfiguration configuration)
        {
            _clientId = configuration["PayPal:ClientId"];
            _clientSecret = configuration["PayPal:ClientSecret"];
        }

        public (bool IsSuccess, string RedirectUrl) PaymentStatus(decimal amount, string returnUrl, string cancelUrl)
        {
            var apiContext = new APIContext(new OAuthTokenCredential(_clientId, _clientSecret).GetAccessToken());

            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        amount = new Amount
                        {
                            currency = "EUR",
                            total = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        description = "Payment for computer parts."
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var createdPayment = payment.Create(apiContext);
            var approvalUrl = createdPayment.links[1].href;

            return (true, approvalUrl);
        }

        public bool ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = new APIContext(new OAuthTokenCredential(_clientId, _clientSecret).GetAccessToken());

            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };

            var executedPayment = payment.Execute(apiContext, paymentExecution);

            return executedPayment.state.ToLower() == "approved";
        }
    }
}

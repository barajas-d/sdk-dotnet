using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using HttpMethodNet = System.Net.Http.HttpMethod;
using MercadoPago.Client.Order;
using MercadoPago.Config;
using MercadoPago.Http;
using MercadoPago.Resource.Order;
using MercadoPago.Serialization;
using MercadoPago.Tests.Http;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MercadoPago.Tests.Client.Order
{
    public class OrderClientMockedTest
    {
        [Fact]
        public void Serialize_CheckoutProOrderRequest_Success()
        {
            var serializer = new DefaultSerializer();
            var request = new OrderCreateRequest
            {
                Type = "online",
                TotalAmount = "500.00",
                ExternalReference = "ext_ref_manual_full_test",
                ProcessingMode = "manual",
                CaptureMode = "automatic",
                MarketplaceFee = "5.00",
                Description = "Travel package SAO-RIO with insurance",
                ExpirationTime = "P1D",
                Payer = new OrderPayerRequest
                {
                    Email = "buyer@mercadopago.com",
                    FirstName = "John",
                    LastName = "Smith",
                    Phone = new OrderPhoneRequest
                    {
                        AreaCode = "11",
                        Number = "999998888"
                    },
                    Identification = new OrderIdentificationRequest
                    {
                        Type = "CPF",
                        Number = "12345678909"
                    },
                    Address = new OrderAddressRequest
                    {
                        ZipCode = "01310-100",
                        StreetName = "Av. Paulista",
                        StreetNumber = "1000",
                        Neighborhood = "Bela Vista",
                        City = "São Paulo"
                    }
                },
                Shipment = new OrderShipmentRequest
                {
                    Mode = "custom",
                    LocalPickup = false,
                    Cost = "15.00",
                    FreeShipping = false,
                    FreeMethods = new List<OrderFreeMethodRequest>
                    {
                        new OrderFreeMethodRequest { Id = 73328 }
                    },
                    Address = new OrderAddressRequest
                    {
                        ZipCode = "01310-100",
                        StreetName = "Av. Paulista",
                        StreetNumber = "1000",
                        Floor = "3",
                        Apartment = "B",
                        Neighborhood = "Bela Vista",
                        City = "São Paulo"
                    }
                },
                Config = new OrderConfigRequest
                {
                    StatementDescriptor = "MYSTORE",
                    DefaultPaymentDueDate = "P1D",
                    Online = new OrderOnlineConfigRequest
                    {
                        AvailableFrom = "2026-01-01T00:00:00Z",
                        AllowedUserType = "account_only",
                        SuccessUrl = "https://example.com/success",
                        FailureUrl = "https://example.com/failure",
                        PendingUrl = "https://example.com/pending",
                        AutoReturn = "approved",
                        Tracks = new List<OrderTrackRequest>
                        {
                            new OrderTrackRequest
                            {
                                Type = "google_ad",
                                Values = new Dictionary<string, string>
                                {
                                    ["conversion_id"] = "21312312312123",
                                    ["conversion_label"] = "TEST"
                                }
                            }
                        }
                    },
                    PaymentMethod = new OrderPaymentMethodConfigRequest
                    {
                        MaxInstallments = 12,
                        NotAllowedIds = new List<string> { "amex" },
                        NotAllowedTypes = new List<string> { "ticket" },
                        Installments = new OrderInstallmentsRequest
                        {
                            InterestFree = new OrderInstallmentsInterestFreeRequest
                            {
                                Type = "range",
                                Values = new List<int> { 2, 6 }
                            }
                        }
                    }
                },
                Items = new List<OrderItemsRequest>
                {
                    new OrderItemsRequest
                    {
                        ExternalCode = "ITEM-001",
                        Title = "Flight SAO-RIO",
                        Description = "Round trip, economy class",
                        CategoryId = "travels",
                        PictureUrl = "https://example.com/img.jpg",
                        Quantity = 1,
                        UnitPrice = "450.00",
                        Type = "travel",
                        EventDate = "2027-01-15T00:00:00.000-03:00"
                    }
                },
                AdditionalInfo = new Dictionary<string, object>
                {
                    ["payer.registration_date"] = "2020-01-15T00:00:00.000-03:00",
                    ["payer.is_prime_user"] = true
                }
            };

            string rawJson = serializer.SerializeToJson(request);
            JObject json;
            using (var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(rawJson)))
            {
                reader.DateParseHandling = Newtonsoft.Json.DateParseHandling.None;
                json = JObject.Load(reader);
            }

            Assert.Equal("online", (string)json["type"]);
            Assert.Equal("manual", (string)json["processing_mode"]);
            Assert.Equal("automatic", (string)json["capture_mode"]);
            Assert.Equal("MYSTORE", (string)json["config"]["statement_descriptor"]);
            Assert.Equal("account_only", (string)json["config"]["online"]["allowed_user_type"]);
            Assert.Equal("approved", (string)json["config"]["online"]["auto_return"]);
            Assert.Equal("21312312312123", (string)json["config"]["online"]["tracks"][0]["values"]["conversion_id"]);
            Assert.Equal(12, (int)json["config"]["payment_method"]["max_installments"]);
            Assert.Equal("range", (string)json["config"]["payment_method"]["installments"]["interest_free"]["type"]);
            Assert.Equal("custom", (string)json["shipment"]["mode"]);
            Assert.Equal(73328, (int)json["shipment"]["free_methods"][0]["id"]);
            Assert.Equal("B", (string)json["shipment"]["address"]["apartment"]);
            Assert.Equal("2027-01-15T00:00:00.000-03:00", (string)json["items"][0]["event_date"]);
            Assert.True((bool)json["additional_info"]["payer.is_prime_user"]);
        }

        [Fact]
        public void Deserialize_CheckoutProOrderResponse_Success()
        {
            var serializer = new DefaultSerializer();
            const string response = @"{
                ""id"": ""ORDTST01KS5AJ6HTK2HRQ3XJ3C2JCKP9"",
                ""type"": ""online"",
                ""processing_mode"": ""manual"",
                ""external_reference"": ""parity-PREF-PM-01"",
                ""marketplace_fee"": ""10.00"",
                ""total_amount"": ""100.00"",
                ""checkout_url"": ""https://www.mercadopago.cl/checkout/v1/redirect?order_id=ORDTST01KS5AJ6HTK2HRQ3XJ3C2JCKP9"",
                ""expiration_time"": ""P1D"",
                ""country_code"": ""ARG"",
                ""user_id"": ""1858095454"",
                ""status"": ""created"",
                ""capture_mode"": ""automatic_async"",
                ""client_token"": ""eyJhbGciOiJSUzI1NiIs"",
                ""currency"": ""ARS"",
                ""created_date"": ""2026-05-21T13:10:56.845Z"",
                ""last_updated_date"": ""2026-05-21T13:10:56.845Z"",
                ""config"": {
                    ""online"": {
                        ""callback_url"": ""https://www.mercadopago.com.ar/"",
                        ""success_url"": ""https://www.mercadopago.com.ar/"",
                        ""pending_url"": ""https://www.mercadopago.com.ar/"",
                        ""failure_url"": ""https://www.mercadopago.com.ar/"",
                        ""available_from"": ""2026-05-16T18:32:00Z"",
                        ""auto_return"": ""approved"",
                        ""retries"": { ""allowed"": false }
                    },
                    ""payment_method"": {
                        ""max_installments"": 12,
                        ""not_allowed_ids"": [""amex""],
                        ""not_allowed_types"": [""ticket""],
                        ""installments"": {
                            ""interest_free"": { ""type"": ""range"", ""values"": [2, 6] }
                        }
                    }
                },
                ""shipment"": {
                    ""mode"": ""custom"",
                    ""local_pickup"": false,
                    ""cost"": ""15.00"",
                    ""free_shipping"": false,
                    ""free_methods"": [{ ""id"": 73328 }],
                    ""address"": { ""floor"": ""3"", ""apartment"": ""B"" }
                }
            }";

            MercadoPago.Resource.Order.Order order = serializer.DeserializeFromJson<MercadoPago.Resource.Order.Order>(response);

            Assert.Equal("ORDTST01KS5AJ6HTK2HRQ3XJ3C2JCKP9", order.Id);
            Assert.Equal("https://www.mercadopago.cl/checkout/v1/redirect?order_id=ORDTST01KS5AJ6HTK2HRQ3XJ3C2JCKP9", order.CheckoutUrl);
            Assert.Equal("eyJhbGciOiJSUzI1NiIs", order.ClientToken);
            Assert.Equal("approved", order.Config.Online.AutoReturn);
            Assert.False(order.Config.Online.Retries.Allowed.Value);
            Assert.Equal(12, order.Config.PaymentMethod.MaxInstallments.Value);
            Assert.Equal("amex", order.Config.PaymentMethod.NotAllowedIds[0]);
            Assert.Equal("range", order.Config.PaymentMethod.Installments.InterestFree.Type);
            Assert.Equal("custom", order.Shipment.Mode);
            Assert.Equal(73328, order.Shipment.FreeMethods[0].Id.Value);
            Assert.Equal("B", order.Shipment.Address.Apartment);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Search_SplitOneToManyFilters_TransportAndResponse(bool asynchronous)
        {
            const string responseJson = @"{
                ""data"": [{ ""id"": ""ORD01K9SN1Q5959CGN8QGW00EH7V5"", ""type"": ""online"",
                    ""status"": ""processed"", ""external_reference"": ""ext_ref_1234"" }],
                ""paging"": { ""total"": ""54"", ""total_pages"": ""3"", ""offset"": ""0"", ""limit"": ""20"" }
            }";
            HttpRequestMessage sentRequest = null;
            CancellationToken sentToken = default;
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                {
                    sentRequest = message;
                    sentToken = token;
                })
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson),
                });
            var client = new OrderClient(
                new DefaultHttpClient(new HttpClient(handler.Object)), new DefaultSerializer());
            var options = new RequestOptions { AccessToken = "offline-search-token" };
            options.CustomHeaders.Add("X-Search-Test", "split-1n");
            var filters = new OrderSearchRequest
            {
                BeginDate = "2025-11-01T00:00:00.000Z",
                EndDate = "2025-11-11T23:59:59.999Z",
                ExternalReference = "ext_ref_1234",
                Type = "online",
                Status = "processed",
                StatusDetail = "accredited",
                PaymentMethodId = "visa",
                PaymentMethodType = "credit_card",
                Page = 2,
                PageSize = 20,
                SortBy = "created_date",
                SortOrder = "desc",
            };
            using var cancellation = new CancellationTokenSource();

            OrderSearchResponse result = asynchronous
                ? await client.SearchAsync(filters, options, cancellation.Token)
                : client.Search(filters, options);

            handler.Protected().Verify(
                "SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethodNet.Get, sentRequest.Method);
            Assert.Equal("/v1/orders", sentRequest.RequestUri.AbsolutePath);
            Assert.Equal("Bearer offline-search-token", sentRequest.Headers.Authorization.ToString());
            Assert.Equal("split-1n", Assert.Single(sentRequest.Headers.GetValues("X-Search-Test")));
            Assert.Equal(asynchronous ? cancellation.Token : default, sentToken);
            var query = new Dictionary<string, string>();
            foreach (string pair in sentRequest.RequestUri.Query.TrimStart('?').Split('&'))
            {
                string[] parts = pair.Split('=');
                query.Add(Uri.UnescapeDataString(parts[0]), Uri.UnescapeDataString(parts[1]));
            }

            Assert.Equal(12, query.Count);
            Assert.Equal("2025-11-01T00:00:00.000Z", query["begin_date"]);
            Assert.Equal("2025-11-11T23:59:59.999Z", query["end_date"]);
            Assert.Equal("ext_ref_1234", query["external_reference"]);
            Assert.Equal("online", query["type"]);
            Assert.Equal("processed", query["status"]);
            Assert.Equal("accredited", query["status_detail"]);
            Assert.Equal("visa", query["payment_method_id"]);
            Assert.Equal("credit_card", query["payment_method_type"]);
            Assert.Equal("2", query["page"]);
            Assert.Equal("20", query["page_size"]);
            Assert.Equal("created_date", query["sort_by"]);
            Assert.Equal("desc", query["sort_order"]);
            Assert.NotNull(result.ApiResponse);
            Assert.Equal("ORD01K9SN1Q5959CGN8QGW00EH7V5", Assert.Single(result.Data).Id);
            Assert.Equal("processed", result.Data[0].Status);
            Assert.Equal("54", Convert.ToString(result.Paging.Total));
            Assert.Equal("3", Convert.ToString(result.Paging.TotalPages));
            Assert.Equal("0", Convert.ToString(result.Paging.Offset));
            Assert.Equal("20", Convert.ToString(result.Paging.Limit));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Create_QrDiscounts_TransportAndResponse(bool cashOut, bool asynchronous)
        {
            const string responseJson = "{\"id\":\"ORD-QR-1\",\"type\":\"qr\",\"status\":\"created\",\"discounts\":{\"payment_methods\":[{\"type\":\"account_money\",\"new_total_amount\":\"90.00\"}]}}";
            HttpRequestMessage sentRequest = null;
            string sentJson = null;
            CancellationToken sentToken = default;
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                {
                    sentRequest = message;
                    sentToken = token;
                    sentJson = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(responseJson),
                });
            var client = new OrderClient(new DefaultHttpClient(new HttpClient(handler.Object)), new DefaultSerializer());
            var options = new RequestOptions { AccessToken = "offline-qr-token" };
            options.CustomHeaders.Add("X-Order-Test", "qr-discounts");
            var request = new OrderCreateRequest
            {
                Type = "qr",
                TotalAmount = "100.00",
                Discounts = new OrderDiscountsRequest
                {
                    PaymentMethods = new List<OrderDiscountPaymentMethodRequest>
                    {
                        new OrderDiscountPaymentMethodRequest { Type = "account_money", NewTotalAmount = "90.00" },
                    },
                },
                Transactions = cashOut
                    ? new OrderTransactionRequest
                    {
                        CashOuts = new List<OrderCashOutRequest>
                        {
                            new OrderCashOutRequest { Amount = "100.00" },
                        },
                    }
                    : new OrderTransactionRequest
                    {
                        Payments = new List<OrderPaymentRequest>
                        {
                            new OrderPaymentRequest { Amount = "100.00" },
                        },
                    },
            };
            using var cancellation = new CancellationTokenSource();

            MercadoPago.Resource.Order.Order order = asynchronous
                ? await client.CreateAsync(request, options, cancellation.Token)
                : client.Create(request, options);

            handler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.Equal(HttpMethodNet.Post, sentRequest.Method);
            Assert.Equal("/v1/orders", sentRequest.RequestUri.AbsolutePath);
            Assert.Equal("Bearer offline-qr-token", sentRequest.Headers.Authorization.ToString());
            Assert.Equal("qr-discounts", Assert.Single(sentRequest.Headers.GetValues("X-Order-Test")));
            Assert.Equal(asynchronous ? cancellation.Token : default, sentToken);
            var json = JObject.Parse(sentJson);
            Assert.Equal("qr", (string)json["type"]);
            var discounts = Assert.IsType<JArray>(json["discounts"]["payment_methods"]);
            var discount = Assert.IsType<JObject>(Assert.Single(discounts));
            Assert.Equal(2, discount.Count);
            Assert.Equal("account_money", (string)discount["type"]);
            Assert.Equal("90.00", (string)discount["new_total_amount"]);
            Assert.Equal(cashOut, json["transactions"]["cash_outs"] != null);
            Assert.Equal(!cashOut, json["transactions"]["payments"] != null);
            Assert.Equal("ORD-QR-1", order.Id);
            Assert.Equal("created", order.Status);
            Assert.Equal("90.00", Assert.Single(order.Discounts.PaymentMethods).NewTotalAmount);
            Assert.NotNull(order.ApiResponse);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Create_WalletConnect_TransportAndResponse(bool recurring, bool asynchronous)
        {
            const string responseJson = "{\"id\":\"ORD-WALLET-1\",\"type\":\"online\",\"status\":\"created\"}";
            HttpRequestMessage sentRequest = null;
            string sentJson = null;
            CancellationToken sentToken = default;
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                {
                    sentRequest = message;
                    sentToken = token;
                    sentJson = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(responseJson),
                });
            var client = new OrderClient(new DefaultHttpClient(new HttpClient(handler.Object)), new DefaultSerializer());
            var options = new RequestOptions { AccessToken = "offline-wallet-token" };
            options.CustomHeaders.Add("X-Order-Test", "wallet-connect");
            var request = new OrderCreateRequest
            {
                Type = "online",
                TotalAmount = "100.00",
                AdditionalInfo = new Dictionary<string, object>
                {
                    ["sub_merchant"] = new Dictionary<string, object>
                    {
                        ["id"] = "seller-123",
                    },
                },
                Transactions = new OrderTransactionRequest
                {
                    Payments = new List<OrderPaymentRequest>
                    {
                        new OrderPaymentRequest
                        {
                            Amount = "100.00",
                            PaymentMethod = new OrderPaymentMethodRequest
                            {
                                Token = "offline-wallet-payment-token",
                            },
                            StatementDescriptor = "WALLET TEST",
                            StoredCredential = recurring ? new OrderStoredCredentialRequest
                            {
                                PaymentInitiator = "merchant",
                                Reason = "recurring",
                            } : null,
                        },
                    },
                },
            };
            using var cancellation = new CancellationTokenSource();

            MercadoPago.Resource.Order.Order order = asynchronous
                ? await client.CreateAsync(request, options, cancellation.Token)
                : client.Create(request, options);

            handler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.Equal(HttpMethodNet.Post, sentRequest.Method);
            Assert.Equal("/v1/orders", sentRequest.RequestUri.AbsolutePath);
            Assert.Equal("Bearer offline-wallet-token", sentRequest.Headers.Authorization.ToString());
            Assert.Equal("wallet-connect", Assert.Single(sentRequest.Headers.GetValues("X-Order-Test")));
            Assert.Equal(asynchronous ? cancellation.Token : default, sentToken);
            var json = JObject.Parse(sentJson);
            Assert.Equal("online", (string)json["type"]);
            Assert.Equal("seller-123", (string)json["additional_info"]["sub_merchant"]["id"]);
            var payment = Assert.IsType<JObject>(Assert.Single((JArray)json["transactions"]["payments"]));
            Assert.Equal("offline-wallet-payment-token", (string)payment["payment_method"]["token"]);
            Assert.Equal("WALLET TEST", (string)payment["statement_descriptor"]);
            if (recurring)
            {
                Assert.Equal("merchant", (string)payment["stored_credential"]["payment_initiator"]);
                Assert.Equal("recurring", (string)payment["stored_credential"]["reason"]);
            }
            else
            {
                Assert.Null(payment["stored_credential"]);
            }
            Assert.Equal("ORD-WALLET-1", order.Id);
            Assert.Equal("created", order.Status);
            Assert.NotNull(order.ApiResponse);
        }

        [Fact(Skip = "Not running in CI.")]
        public void Create_WithBoletoAndAdditionalInfo_PureMock_Success()
        {
            // Arrange
            var httpMock = new HttpClientMock();
            var serializer = new DefaultSerializer();
            var httpClient = new DefaultHttpClient(httpMock.HttpClient);
            var client = new OrderClient(httpClient, serializer);

            var createUrl = $"{MercadoPagoConfig.BaseUrl}/v1/orders";
            var httpResponse = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"ord_123\",\"status\":\"processed\"}")
            };
            httpMock.MockRequest(createUrl, HttpMethodNet.Post, httpResponse);

            var request = new OrderCreateRequest
            {
                Type = "online",
                TotalAmount = "1000.00",
                ExternalReference = "ext_ref_1234",
                CaptureMode = "automatic_async",
                Transactions = new OrderTransactionRequest
                {
                    Payments = new List<OrderPaymentRequest>
                    {
                        new OrderPaymentRequest
                        {
                            Amount = "1000.00",
                            PaymentMethod = new OrderPaymentMethodRequest { Id = "boleto", Type = "ticket" }
                        }
                    }
                },
                Payer = new OrderPayerRequest { Email = "test_user@example.com" },
                AdditionalInfo = new Dictionary<string, object>
                {
                    ["payer"] = new Dictionary<string, object> { ["authentication_type"] = "senha" },
                    ["shipment"] = new Dictionary<string, object> { ["express"] = true }
                }
            };

            // Act
            MercadoPago.Resource.Order.Order order = client.Create(request);

            // Assert
            Assert.NotNull(order);
            Assert.Equal("ord_123", order.Id);
            httpMock.VerifySended(HttpMethodNet.Post, new System.Uri(createUrl), Moq.Times.Once());
        }
    }
}

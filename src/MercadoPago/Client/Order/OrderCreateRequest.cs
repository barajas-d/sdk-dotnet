// API version: d0494f1c-8d81-4c76-ae1d-0c65bb8ef6de

namespace MercadoPago.Client.Order
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Request payload for creating a new payment order via <c>POST /v1/orders</c>.
    /// Contains all the information needed to define an order: type, amounts, transactions,
    /// payer details, line items, configuration, and shipping.
    /// </summary>
    /// <remarks>
    /// Check the parameters
    /// <a href="https://www.mercadopago.com/developers/en/reference/online-payments/checkout-api/create-order/post/">here</a>.
    /// </remarks>
    /// <seealso cref="OrderClient.Create(OrderCreateRequest, RequestOptions)"/>
    /// <seealso cref="OrderClient.CreateAsync(OrderCreateRequest, RequestOptions, System.Threading.CancellationToken)"/>
    public class OrderCreateRequest : IdempotentRequest
    {
        /// <summary>
        /// Order type identifier. Determines the processing flow (e.g., "online" for e-commerce transactions).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Unique external reference that you can use to synchronize this order with your own payment system.
        /// </summary>
        public string ExternalReference { get; set; }

        /// <summary>
        /// Total monetary amount of the order, expressed as a decimal string (e.g., "100.50").
        /// </summary>
        public string TotalAmount { get; set; }

        /// <summary>
        /// ISO 4217 currency code for the order (e.g., "BRL", "ARS", "MXN").
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Determines when the payment funds are captured. Defaults to "automatic_async".
        /// Use "manual" to authorize first and capture later via <see cref="OrderClient.Capture(string, RequestOptions)"/>.
        /// </summary>
        [JsonProperty("capture_mode")]
        public string CaptureMode { get; set; } = "automatic_async";

        /// <summary>
        /// Configures which processing mode to use for the order (e.g., "aggregator" or "gateway").
        /// </summary>
        public string ProcessingMode { get; set; }

        /// <summary>
        /// Human-readable description of the order, shown to the payer during checkout.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Marketplace identifier indicating the origin of the payment when operating as a marketplace.
        /// </summary>
        public string Marketplace { get; set; }

        /// <summary>
        /// Fee amount collected by a marketplace or MercadoPago application, expressed as a decimal string.
        /// </summary>
        public string MarketplaceFee { get; set; }

        /// <summary>
        /// Expiration time for the order in ISO 8601 format. After this time the order can no longer be processed.
        /// </summary>
        public string ExpirationTime { get; set; }

        /// <summary>
        /// Date and time (ISO 8601) from which the checkout becomes available to the payer.
        /// </summary>
        public string CheckoutAvailableAt { get; set; }

        /// <summary>
        /// Transaction information containing the list of payments associated with the order.
        /// </summary>
        /// <seealso cref="OrderTransactionRequest"/>
        public OrderTransactionRequest Transactions { get; set; }

        /// <summary>
        /// Payer details including name, email, identification, phone, and address.
        /// </summary>
        /// <seealso cref="OrderPayerRequest"/>
        public OrderPayerRequest Payer { get; set; }

        /// <summary>
        /// List of line items (products or services) included in the order.
        /// </summary>
        /// <seealso cref="OrderItemsRequest"/>
        public IList<OrderItemsRequest> Items { get; set; }

        /// <summary>
        /// Order-level configuration for payment method restrictions and online checkout behavior.
        /// </summary>
        /// <seealso cref="OrderConfigRequest"/>
        public OrderConfigRequest Config { get; set; }

        /// <summary>
        /// Shipping information for the order, including the delivery address.
        /// </summary>
        /// <seealso cref="OrderShipmentRequest"/>
        public OrderShipmentRequest Shipment { get; set; }

        /// <summary>
        /// Request-body additional_info for Checkout Pro and Wallet Connect order creation.
        /// Checkout Pro accepts an object with dotted keys such as "payer.registration_date"
        /// and "payer.is_prime_user"; keep those keys intact rather than nesting a payer object.
        /// Wallet Connect can supply a "sub_merchant" object, including integer address_door_number.
        /// This request property does not depend on additional_info appearing in the 201 response.
        /// </summary>
        [JsonProperty("additional_info")]
        public IDictionary<string, object> AdditionalInfo { get; set; }

        /// <summary>
        /// Request-only payment-method discounts for QR and QR-CA order creation.
        /// Serialized as discounts.payment_methods, an array whose items contain string
        /// type and new_total_amount fields, distinct from payment-level response discounts.
        /// </summary>
        /// <seealso cref="OrderDiscountsRequest"/>
        [JsonProperty("discounts")]
        public OrderDiscountsRequest Discounts { get; set; }

        /// <summary>
        /// Integration metadata identifying the integrator, platform, corporation, and sponsor.
        /// </summary>
        /// <seealso cref="OrderIntegrationDataRequest"/>
        public OrderIntegrationDataRequest IntegrationData { get; set; }
    }

    /// <summary>
    /// Payment-method discounts to include in a QR order creation request.
    /// </summary>
    public class OrderDiscountsRequest
    {
        /// <summary>
        /// Discounts offered for individual payment methods.
        /// </summary>
        [JsonProperty("payment_methods")]
        public IList<OrderDiscountPaymentMethodRequest> PaymentMethods { get; set; }
    }

    /// <summary>
    /// Discount offered for a payment method in a QR order creation request.
    /// </summary>
    public class OrderDiscountPaymentMethodRequest
    {
        /// <summary>
        /// Payment method type eligible for the discount.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Total amount after applying the discount.
        /// </summary>
        [JsonProperty("new_total_amount")]
        public string NewTotalAmount { get; set; }
    }
}

// API version: d0494f1c-8d81-4c76-ae1d-0c65bb8ef6de

namespace MercadoPago.Client.Order
{
    /// <summary>
    /// Stored credential information for recurring or merchant-initiated transactions.
    /// For Wallet Connect, include this node only for recurring payments; omit it for
    /// one-shot payments.
    /// </summary>
    /// <seealso cref="OrderPaymentRequest"/>
    public class OrderStoredCredentialRequest
    {
        /// <summary>
        /// Who initiated the payment. For Wallet Connect recurring payments, use "merchant".
        /// </summary>
        public string PaymentInitiator { get; set; }

        /// <summary>
        /// Reason for the stored credential. For Wallet Connect recurring payments, use "recurring".
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Indicates whether to store the payment method for future transactions.
        /// </summary>
        public bool? StorePaymentMethod { get; set; }

        /// <summary>
        /// Indicates whether this is the first payment in a series of recurring charges.
        /// </summary>
        public bool? FirstPayment { get; set; }

        /// <summary>
        /// Reference to the previous transaction in a recurring series. Required from the second
        /// charge onwards to link this payment to the original card-network authorization.
        /// Type: string (transaction ID).
        /// </summary>
        public string PrevTransactionRef { get; set; }
    }

}

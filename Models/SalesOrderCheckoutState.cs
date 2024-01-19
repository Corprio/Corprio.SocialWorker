namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Sales orders' entity property that captures checkout related information
    /// </summary>
    public class SalesOrderCheckoutState
    {
        /// <summary>
        /// True if the buyer has elected to proceed to payment in the checkout page.
        /// </summary>
        public bool IsPaymentClicked { get; set; }

        /// <summary>
        /// Delivery method elected by the buyer.
        /// </summary>
        public DeliveryOption ChosenDeliveryMethod { get; set; }

        /// <summary>
        /// Given name inputted by the buyer into the checkout form.
        /// Note: we need to store this in EP because the sales order only stores one bill name.
        /// </summary>
        public string BillPerson_GivenName { get; set; }

        /// <summary>
        /// Family name inputted by the buyer into the checkout form.
        /// Note: we need to store this in EP because the sales order only stores one bill name.
        /// </summary>
        public string BillPerson_FamilyName { get; set; }
    }
}

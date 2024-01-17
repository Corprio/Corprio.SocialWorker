namespace Corprio.SocialWorker.Models
{
    public class SalesOrderCheckoutState
    {
        public bool IsPaymentClicked { get; set; }

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

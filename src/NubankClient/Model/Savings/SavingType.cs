namespace NubankClient.Model.Savings
{
    public enum SavingType
    {
        Unknown,
        WelcomeEvent,
        TransferInEvent,
        TransferOutEvent,
        BarcodePaymentEvent,
        CanceledScheduledTransferOutEvent,
        AddToReserveEvent,
        DebitPurchaseEvent,
        BillPaymentEvent,
        CanceledScheduledBarcodePaymentRequestEvent,
        RemoveFromReserveEvent,
        TransferOutReversalEvent,
        SalaryPortabilityRequestEvent,
        SalaryPortabilityRequestApprovalEvent
    }
}

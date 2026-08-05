namespace Unify.Erp.Domain.Inventory;

public enum StockMovementType
{
    AdjustmentIn = 1,
    AdjustmentOut = 2,
    TransferOut = 3,
    TransferIn = 4,
    SaleIssue = 5,
    PurchaseReceipt = 6,
    ReturnIn = 7,
    ReturnOut = 8
}

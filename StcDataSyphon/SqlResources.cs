
namespace StcDataSyphon
{
    internal static class SqlResources
    {
        #region general queries

        public const string selectAllRowsSql = "SELECT * FROM {0};";
        public const string truncateTableSql = "TRUNCATE TABLE {0};";

        #endregion


        #region PSql/Stage1 queries
        public const string transactionsQueryFull = @"SELECT t.*
FROM Transactions t
WHERE t.idxFolio <> 0
AND t.thDocType <> 35;";

        public const string transactionsQueryRestricted = @"
SELECT
		thRunNo,
		thAcCode,
		thOurRef,
		thIdxDocCode,
		idxFolio,
		thDueDate,
		thTransDate,
		thDocType,
		thVAT_Standard_1,
		thVAT_Standard_2,
		thNetValue_1,
		thNetValue_2,
		thTotalVAT_1,
		thTotalVAT_2,
		thSettlePerc_1,
		thSettlePerc_2,
		thTotSettleDisc_1,
		thTotSettleDisc_2,
		thTotLineDisc_1,
		thTotLineDisc_2,
		thSettled_1,
		thSettled_2,
		thRemitRef,
		thTotal1_1,
		thTotal1_2,
		thTotal2_1,
		thTotal2_2,
		thTotalCost_1,
		thTotalCost_2,
		thTotalInvoiced_1,
		thTotalInvoiced_2,
		thYourRef
FROM Transactions;";


        public const string transactionLinesQueryFull = @"
SELECT tl.*
FROM Transactions t
INNER JOIN TransactionLines tl ON t.idxFolio = tl.idxFolio
WHERE t.idxFolio <> 0
AND t.thDocType <> 35
AND tl.IdxStockCode <> ''";

        public const string transactionLinesQueryRestricted = @"
SELECT
		tl.idxFolio,
		tl.tlLinePos,
		tl.tlRunNo,
		tl.IdxStockCode,
		tl.tlLineNo,
		tl.tlDocType,
		tl.tlQty_1,
		tl.tlQty_2,
		tl.tlQtyMul_1,
		tl.tlQtyMul_2,
		tl.tlQtyWOff_1,
		tl.tlQtyWOff_2,
		tl.tlQtyDel_1,
		tl.tlQtyDel_2,
		tl.tlStockDeductQty_1,
		tl.tlStockDeductQty_2,
		tl.tlOrderFolio,
		tl.tlOrderLineNo,
		tl.tlQtyPicked_1,
		tl.tlQtyPicked_2,
		tl.tlQtyPickedWO_1,
		tl.tlQtyPickedWO_2,
		tl.tlOurRef
FROM Transactions t
INNER JOIN TransactionLines tl ON t.idxFolio = tl.idxFolio
WHERE t.idxFolio <> 0
AND t.thDocType <> 35
AND tl.IdxStockCode <> '';";

        #endregion





        #region Stage2 queries
        public const string selectStatementStgCustomers = @"
SELECT
		TRIM(idxAcCode) AS idxAcCode,
		cuType,
		TRIM(cuCompany) AS cuCompany,
		TRIM(cuArea) AS cuArea,
		TRIM(cuAccType) AS cuAccType,
		TRIM(cuStateTo) AS cuStateTo,
		TRIM(cuVATRegNo) AS cuVATRegNo,
		TRIM(cuIdxVATRegNo) AS cuIdxVATRegNo,
		TRIM(cuAddr1) AS cuAddr1,
		TRIM(cuAddr2) AS cuAddr2,
		TRIM(cuAddr3) AS cuAddr3,
		TRIM(cuAddr4) AS cuAddr4,
		TRIM(cuAddr5) AS cuAddr5,
		TRIM(cuDelAddr1) AS cuDelAddr1,
		TRIM(cuDelAddr2) AS cuDelAddr2,
		TRIM(cuDelAddr3) AS cuDelAddr3,
		TRIM(cuDelAddr4) AS cuDelAddr4,
		TRIM(cuDelAddr5) AS cuDelAddr5,
		TRIM(cuContact) AS cuContact,
		TRIM(cuPhone) AS cuPhone,
		TRIM(cuFax) AS cuFax,
		cuCurrency,
		TRIM(cuVATCode) AS cuVATCode,
		cuCredLim_1,
		cuCredLim_2,
		TRIM(cuDepartment) AS cuDepartment,
		cuAccStatus,
		TRIM(cuMobile) AS cuMobile,
		TRIM(cuUser1) AS cuUser1,
		TRIM(cuUser2) AS cuUser2,
		TRIM(cuInvoiceTo) AS cuInvoiceTo,
		TRIM(cuIdxPostCode) AS cuIdxPostCode,
		TRIM(cuIdxAltCode) AS cuIdxAltCode,
		TRIM(cuUser3) AS cuUser3,
		TRIM(cuUser4) AS cuUser4,
		TRIM(cuIdxEmailAddr) AS cuIdxEmailAddr,
		cuDefaultTagNo,
		TRIM(cuUser5) AS cuUser5,
		TRIM(cuUser6) AS cuUser6
FROM raw_customers;";


        public const string selectStatementStgStock = @"
SELECT
		TRIM(IdxStockCode) AS IdxStockCode,
		TRIM(stDesc1) AS stDesc1,
		TRIM(stDesc2) AS stDesc2,
		TRIM(stDesc3) AS stDesc3,
		TRIM(stDesc4) AS stDesc4,
		TRIM(stDesc5) AS stDesc5,
		TRIM(stDesc6) AS stDesc6,
		TRIM(stUnitOfSale) AS stUnitOfSale,
		TRIM(stLocation) AS stLocation,
		TRIM(stIdxBinLocation) AS stIdxBinLocation,
		TRIM(stBarCode) AS stBarCode,
		TRIM(stUser1) AS stUser1,
		TRIM(stUser2) AS stUser2,
		TRIM(stUser3) AS stUser3,
		TRIM(stUser4) AS stUser4,
		TRIM(stUser5) AS stUser5,
		TRIM(stUser6) AS stUser6,
		TRIM(stUser7) AS stUser7,
		TRIM(stUser8) AS stUser8,
		TRIM(stUser9) AS stUser9,
		TRIM(stUser10) AS stUser10,
		stStockType
FROM raw_stock;";


        public const string selectStatementStgStockInventoryLevels = @"
SELECT
        s.IdxStockCode,
        s.stQtyInStock_1,
        s.stQtyInStock_2,
        s.stQtyPosted_1,
        s.stQtyPosted_2,
        s.stQtyAllocated_1,
        s.stQtyAllocated_2,
        s.stQtyOnOrder_1,
        s.stQtyOnOrder_2
FROM raw_stock s
WHERE s.stStockType = 'P';";


        public const string selectStatementStgStockPrices = @"
SELECT
        IdxStockCode,
        stCostPrice_1,
        stCostPrice_2,
        stSalesBandAPrice_1,
        stSalesBandAPrice_2,
        stSalesBandBPrice_1,
        stSalesBandBPrice_2,
        stSalesBandCPrice_1,
        stSalesBandCPrice_2,
        stSalesBandDPrice_1,
        stSalesBandDPrice_2,
        stSalesBandEPrice_1,
        stSalesBandEPrice_2,
        stSalesBandFPrice_1,
        stSalesBandFPrice_2,
        stSalesBandGPrice_1,
        stSalesBandGPrice_2,
        stSalesBandHPrice_1,
        stSalesBandHPrice_2
FROM raw_stock
WHERE stStockType = 'P'";

        // these tables already have a filtered set of columns and rows
        public const string selectStatementStgTransactionLines = "SELECT * FROM raw_transactionlines;";
        public const string selectStatementStgTransactions = "SELECT * FROM raw_transactions;";

        // but this stage executes quicker on the server with a reduced transactionlines query - go figure...
        // and we need to trim IdxStockCode
        public const string selectStatementStgTransactionLinesRestricted = @"
SELECT
		idxFolio,
		tlLinePos,
		tlRunNo,
		TRIM(IdxStockCode) AS IdxStockCode,
		tlLineNo,
		tlDocType,
		tlQty_1,
		tlQty_2,
		tlQtyMul_1,
		tlQtyMul_2,
		tlQtyWOff_1,
		tlQtyWOff_2,
		tlQtyDel_1,
		tlQtyDel_2,
		tlStockDeductQty_1,
		tlStockDeductQty_2,
		tlOrderFolio,
		tlOrderLineNo,
		tlQtyPicked_1,
		tlQtyPicked_2,
		tlQtyPickedWO_1,
		tlQtyPickedWO_2,
		tlOurRef
FROM raw_transactionlines;";

		// and a further trim is needed in transactions
		public const string selectStatementStgTransactionsRestricted = @"
SELECT
		thRunNo,
		TRIM(thAcCode) thAcCode,
		thOurRef,
		thIdxDocCode,
		idxFolio,
		thDueDate,
		thTransDate,
		thDocType,
		thVAT_Standard_1,
		thVAT_Standard_2,
		thNetValue_1,
		thNetValue_2,
		thTotalVAT_1,
		thTotalVAT_2,
		thSettlePerc_1,
		thSettlePerc_2,
		thTotSettleDisc_1,
		thTotSettleDisc_2,
		thTotLineDisc_1,
		thTotLineDisc_2,
		thSettled_1,
		thSettled_2,
		thRemitRef,
		thTotal1_1,
		thTotal1_2,
		thTotal2_1,
		thTotal2_2,
		thTotalCost_1,
		thTotalCost_2,
		thTotalInvoiced_1,
		thTotalInvoiced_2,
		TRIM(thYourRef) AS thYourRef
FROM raw_transactions;";


        #endregion

        #region stage 3 data copy stored procedure calls

        public const string dataCopyProcCustomers = "CALL `stc_core`.`UpdateCoreCustomers`();";
        public const string dataCopyProcStock = "CALL `stc_core`.`UpdateCoreStock`();";
        public const string dataCopyProcStockInventoryLevels = "CALL `stc_core`.`UpdateCoreStockInventoryLevels`();";
        public const string dataCopyProcStockPrices = "CALL `stc_core`.`UpdateCoreStockPrices`();";
        public const string dataCopyProcTransactionLines = "CALL `stc_core`.`UpdateCoreTransactionLines`();";
        public const string dataCopyProcTransactions = "CALL `stc_core`.`UpdateCoreTransactions`();";

        #endregion
    }




}

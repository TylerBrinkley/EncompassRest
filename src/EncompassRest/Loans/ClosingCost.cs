using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Newtonsoft.Json;

namespace EncompassRest.Loans
{
    public sealed partial class ClosingCost : IDirty
    {
        private Value<decimal?> _adjustmentFactor;
        public decimal? AdjustmentFactor { get { return _adjustmentFactor; } set { _adjustmentFactor = value; } }
        private Value<string> _aggregateAdjustmentFwbc;
        public string AggregateAdjustmentFwbc { get { return _aggregateAdjustmentFwbc; } set { _aggregateAdjustmentFwbc = value; } }
        private Value<decimal?> _borrowerPaidDiscountPointsTotalAmount;
        public decimal? BorrowerPaidDiscountPointsTotalAmount { get { return _borrowerPaidDiscountPointsTotalAmount; } set { _borrowerPaidDiscountPointsTotalAmount = value; } }
        private Value<decimal?> _brokerCommissionBasedPrice;
        public decimal? BrokerCommissionBasedPrice { get { return _brokerCommissionBasedPrice; } set { _brokerCommissionBasedPrice = value; } }
        private Value<decimal?> _brokerCommissionBasedUnitPercentage;
        public decimal? BrokerCommissionBasedUnitPercentage { get { return _brokerCommissionBasedUnitPercentage; } set { _brokerCommissionBasedUnitPercentage = value; } }
        private Value<decimal?> _brokerCommissionBasedUnitPrice;
        public decimal? BrokerCommissionBasedUnitPrice { get { return _brokerCommissionBasedUnitPrice; } set { _brokerCommissionBasedUnitPrice = value; } }
        private Value<string> _closingCostProgram;
        public string ClosingCostProgram { get { return _closingCostProgram; } set { _closingCostProgram = value; } }
        private Value<string> _closingCostScenarioXml;
        public string ClosingCostScenarioXml { get { return _closingCostScenarioXml; } set { _closingCostScenarioXml = value; } }
        public ClosingDisclosure1 ClosingDisclosure1 { get; set; }
        public ClosingDisclosure2 ClosingDisclosure2 { get; set; }
        public ClosingDisclosure3 ClosingDisclosure3 { get; set; }
        public ClosingDisclosure4 ClosingDisclosure4 { get; set; }
        public ClosingDisclosure5 ClosingDisclosure5 { get; set; }
        private Value<decimal?> _disclosedSalesPrice;
        public decimal? DisclosedSalesPrice { get { return _disclosedSalesPrice; } set { _disclosedSalesPrice = value; } }
        private Value<string> _escrowCompanyName;
        public string EscrowCompanyName { get { return _escrowCompanyName; } set { _escrowCompanyName = value; } }
        private Value<string> _escrowTableDesc1;
        public string EscrowTableDesc1 { get { return _escrowTableDesc1; } set { _escrowTableDesc1 = value; } }
        private Value<string> _escrowTableDesc2;
        public string EscrowTableDesc2 { get { return _escrowTableDesc2; } set { _escrowTableDesc2 = value; } }
        private Value<string> _escrowTableDesc3;
        public string EscrowTableDesc3 { get { return _escrowTableDesc3; } set { _escrowTableDesc3 = value; } }
        private Value<string> _escrowTableDesc4;
        public string EscrowTableDesc4 { get { return _escrowTableDesc4; } set { _escrowTableDesc4 = value; } }
        private Value<string> _escrowTableDesc5;
        public string EscrowTableDesc5 { get { return _escrowTableDesc5; } set { _escrowTableDesc5 = value; } }
        private Value<decimal?> _escrowTableFee;
        public decimal? EscrowTableFee { get { return _escrowTableFee; } set { _escrowTableFee = value; } }
        private Value<decimal?> _escrowTableFee1;
        public decimal? EscrowTableFee1 { get { return _escrowTableFee1; } set { _escrowTableFee1 = value; } }
        private Value<decimal?> _escrowTableFee2;
        public decimal? EscrowTableFee2 { get { return _escrowTableFee2; } set { _escrowTableFee2 = value; } }
        private Value<decimal?> _escrowTableFee3;
        public decimal? EscrowTableFee3 { get { return _escrowTableFee3; } set { _escrowTableFee3 = value; } }
        private Value<decimal?> _escrowTableFee4;
        public decimal? EscrowTableFee4 { get { return _escrowTableFee4; } set { _escrowTableFee4 = value; } }
        private Value<decimal?> _escrowTableFee5;
        public decimal? EscrowTableFee5 { get { return _escrowTableFee5; } set { _escrowTableFee5 = value; } }
        private Value<string> _escrowTableName;
        public string EscrowTableName { get { return _escrowTableName; } set { _escrowTableName = value; } }
        public FeeVarianceOther FeeVarianceOther { get; set; }
        private Value<List<FeeVariance>> _feeVariances;
        public List<FeeVariance> FeeVariances { get { return _feeVariances; } set { _feeVariances = value; } }
        public Gfe2010 Gfe2010 { get; set; }
        public Gfe2010Page Gfe2010Page { get; set; }
        public Gfe2010Section Gfe2010Section { get; set; }
        private Value<string> _id;
        public string Id { get { return _id; } set { _id = value; } }
        private Value<decimal?> _impoundHazInsRate;
        public decimal? ImpoundHazInsRate { get { return _impoundHazInsRate; } set { _impoundHazInsRate = value; } }
        private Value<decimal?> _impoundMortgInsPremRate;
        public decimal? ImpoundMortgInsPremRate { get { return _impoundMortgInsPremRate; } set { _impoundMortgInsPremRate = value; } }
        private Value<string> _impoundMortgInsPremYearlyBasis;
        public string ImpoundMortgInsPremYearlyBasis { get { return _impoundMortgInsPremYearlyBasis; } set { _impoundMortgInsPremYearlyBasis = value; } }
        private Value<decimal?> _impoundTaxesRate;
        public decimal? ImpoundTaxesRate { get { return _impoundTaxesRate; } set { _impoundTaxesRate = value; } }
        private Value<string> _impoundType1;
        public string ImpoundType1 { get { return _impoundType1; } set { _impoundType1 = value; } }
        private Value<string> _impoundType2;
        public string ImpoundType2 { get { return _impoundType2; } set { _impoundType2 = value; } }
        private Value<string> _impoundType3;
        public string ImpoundType3 { get { return _impoundType3; } set { _impoundType3 = value; } }
        private Value<string> _impoundType4;
        public string ImpoundType4 { get { return _impoundType4; } set { _impoundType4 = value; } }
        public LoanEstimate1 LoanEstimate1 { get; set; }
        public LoanEstimate2 LoanEstimate2 { get; set; }
        public LoanEstimate3 LoanEstimate3 { get; set; }
        private Value<decimal?> _proposedMonthlyHazardInsurance;
        public decimal? ProposedMonthlyHazardInsurance { get { return _proposedMonthlyHazardInsurance; } set { _proposedMonthlyHazardInsurance = value; } }
        private Value<decimal?> _proposedMonthlyMortgageInsurance;
        public decimal? ProposedMonthlyMortgageInsurance { get { return _proposedMonthlyMortgageInsurance; } set { _proposedMonthlyMortgageInsurance = value; } }
        private Value<decimal?> _section1000BorrowerPaidTotalAmount;
        public decimal? Section1000BorrowerPaidTotalAmount { get { return _section1000BorrowerPaidTotalAmount; } set { _section1000BorrowerPaidTotalAmount = value; } }
        private Value<decimal?> _section1000SellerPaidTotalAmount;
        public decimal? Section1000SellerPaidTotalAmount { get { return _section1000SellerPaidTotalAmount; } set { _section1000SellerPaidTotalAmount = value; } }
        private Value<decimal?> _settlementClosingFeeNewHudBorPaidAmount;
        public decimal? SettlementClosingFeeNewHudBorPaidAmount { get { return _settlementClosingFeeNewHudBorPaidAmount; } set { _settlementClosingFeeNewHudBorPaidAmount = value; } }
        private Value<decimal?> _settlementClosingFeeNewHudSelPaidAmount;
        public decimal? SettlementClosingFeeNewHudSelPaidAmount { get { return _settlementClosingFeeNewHudSelPaidAmount; } set { _settlementClosingFeeNewHudSelPaidAmount = value; } }
        private Value<string> _titleCompanyName;
        public string TitleCompanyName { get { return _titleCompanyName; } set { _titleCompanyName = value; } }
        private Value<decimal?> _titleExaminationNewHudSelPaidAmount;
        public decimal? TitleExaminationNewHudSelPaidAmount { get { return _titleExaminationNewHudSelPaidAmount; } set { _titleExaminationNewHudSelPaidAmount = value; } }
        private Value<string> _titleTable2010Name;
        public string TitleTable2010Name { get { return _titleTable2010Name; } set { _titleTable2010Name = value; } }
        private Value<string> _titleTableName;
        public string TitleTableName { get { return _titleTableName; } set { _titleTableName = value; } }
        private Value<decimal?> _totalForBorPaid1;
        public decimal? TotalForBorPaid1 { get { return _totalForBorPaid1; } set { _totalForBorPaid1 = value; } }
        private Value<decimal?> _totalForBorPaid2;
        public decimal? TotalForBorPaid2 { get { return _totalForBorPaid2; } set { _totalForBorPaid2 = value; } }
        private Value<decimal?> _totalForBorPaid3;
        public decimal? TotalForBorPaid3 { get { return _totalForBorPaid3; } set { _totalForBorPaid3 = value; } }
        private Value<decimal?> _totalForBorPaid4;
        public decimal? TotalForBorPaid4 { get { return _totalForBorPaid4; } set { _totalForBorPaid4 = value; } }
        private Value<decimal?> _totalForBorPaid5;
        public decimal? TotalForBorPaid5 { get { return _totalForBorPaid5; } set { _totalForBorPaid5 = value; } }
        private Value<decimal?> _totalForBorPaid6;
        public decimal? TotalForBorPaid6 { get { return _totalForBorPaid6; } set { _totalForBorPaid6 = value; } }
        private Value<decimal?> _totalForSellerPaid1;
        public decimal? TotalForSellerPaid1 { get { return _totalForSellerPaid1; } set { _totalForSellerPaid1 = value; } }
        private Value<decimal?> _totalForSellerPaid2;
        public decimal? TotalForSellerPaid2 { get { return _totalForSellerPaid2; } set { _totalForSellerPaid2 = value; } }
        private Value<decimal?> _totalForSellerPaid3;
        public decimal? TotalForSellerPaid3 { get { return _totalForSellerPaid3; } set { _totalForSellerPaid3 = value; } }
        private Value<decimal?> _totalForSellerPaid4;
        public decimal? TotalForSellerPaid4 { get { return _totalForSellerPaid4; } set { _totalForSellerPaid4 = value; } }
        private Value<decimal?> _totalForSellerPaid5;
        public decimal? TotalForSellerPaid5 { get { return _totalForSellerPaid5; } set { _totalForSellerPaid5 = value; } }
        private Value<decimal?> _totalForSellerPaid6;
        public decimal? TotalForSellerPaid6 { get { return _totalForSellerPaid6; } set { _totalForSellerPaid6 = value; } }
        private int _gettingDirty;
        private int _settingDirty; 
        internal bool Dirty
        {
            get
            {
                if (Interlocked.CompareExchange(ref _gettingDirty, 1, 0) != 0) return false;
                var dirty = _adjustmentFactor.Dirty
                    || _aggregateAdjustmentFwbc.Dirty
                    || _borrowerPaidDiscountPointsTotalAmount.Dirty
                    || _brokerCommissionBasedPrice.Dirty
                    || _brokerCommissionBasedUnitPercentage.Dirty
                    || _brokerCommissionBasedUnitPrice.Dirty
                    || _closingCostProgram.Dirty
                    || _closingCostScenarioXml.Dirty
                    || _disclosedSalesPrice.Dirty
                    || _escrowCompanyName.Dirty
                    || _escrowTableDesc1.Dirty
                    || _escrowTableDesc2.Dirty
                    || _escrowTableDesc3.Dirty
                    || _escrowTableDesc4.Dirty
                    || _escrowTableDesc5.Dirty
                    || _escrowTableFee.Dirty
                    || _escrowTableFee1.Dirty
                    || _escrowTableFee2.Dirty
                    || _escrowTableFee3.Dirty
                    || _escrowTableFee4.Dirty
                    || _escrowTableFee5.Dirty
                    || _escrowTableName.Dirty
                    || _feeVariances.Dirty
                    || _id.Dirty
                    || _impoundHazInsRate.Dirty
                    || _impoundMortgInsPremRate.Dirty
                    || _impoundMortgInsPremYearlyBasis.Dirty
                    || _impoundTaxesRate.Dirty
                    || _impoundType1.Dirty
                    || _impoundType2.Dirty
                    || _impoundType3.Dirty
                    || _impoundType4.Dirty
                    || _proposedMonthlyHazardInsurance.Dirty
                    || _proposedMonthlyMortgageInsurance.Dirty
                    || _section1000BorrowerPaidTotalAmount.Dirty
                    || _section1000SellerPaidTotalAmount.Dirty
                    || _settlementClosingFeeNewHudBorPaidAmount.Dirty
                    || _settlementClosingFeeNewHudSelPaidAmount.Dirty
                    || _titleCompanyName.Dirty
                    || _titleExaminationNewHudSelPaidAmount.Dirty
                    || _titleTable2010Name.Dirty
                    || _titleTableName.Dirty
                    || _totalForBorPaid1.Dirty
                    || _totalForBorPaid2.Dirty
                    || _totalForBorPaid3.Dirty
                    || _totalForBorPaid4.Dirty
                    || _totalForBorPaid5.Dirty
                    || _totalForBorPaid6.Dirty
                    || _totalForSellerPaid1.Dirty
                    || _totalForSellerPaid2.Dirty
                    || _totalForSellerPaid3.Dirty
                    || _totalForSellerPaid4.Dirty
                    || _totalForSellerPaid5.Dirty
                    || _totalForSellerPaid6.Dirty
                    || ClosingDisclosure1?.Dirty == true
                    || ClosingDisclosure2?.Dirty == true
                    || ClosingDisclosure3?.Dirty == true
                    || ClosingDisclosure4?.Dirty == true
                    || ClosingDisclosure5?.Dirty == true
                    || FeeVarianceOther?.Dirty == true
                    || Gfe2010?.Dirty == true
                    || Gfe2010Page?.Dirty == true
                    || Gfe2010Section?.Dirty == true
                    || LoanEstimate1?.Dirty == true
                    || LoanEstimate2?.Dirty == true
                    || LoanEstimate3?.Dirty == true;
                _gettingDirty = 0;
                return dirty;
            }
            set
            {
                if (Interlocked.CompareExchange(ref _settingDirty, 1, 0) != 0) return;
                _adjustmentFactor.Dirty = value;
                _aggregateAdjustmentFwbc.Dirty = value;
                _borrowerPaidDiscountPointsTotalAmount.Dirty = value;
                _brokerCommissionBasedPrice.Dirty = value;
                _brokerCommissionBasedUnitPercentage.Dirty = value;
                _brokerCommissionBasedUnitPrice.Dirty = value;
                _closingCostProgram.Dirty = value;
                _closingCostScenarioXml.Dirty = value;
                _disclosedSalesPrice.Dirty = value;
                _escrowCompanyName.Dirty = value;
                _escrowTableDesc1.Dirty = value;
                _escrowTableDesc2.Dirty = value;
                _escrowTableDesc3.Dirty = value;
                _escrowTableDesc4.Dirty = value;
                _escrowTableDesc5.Dirty = value;
                _escrowTableFee.Dirty = value;
                _escrowTableFee1.Dirty = value;
                _escrowTableFee2.Dirty = value;
                _escrowTableFee3.Dirty = value;
                _escrowTableFee4.Dirty = value;
                _escrowTableFee5.Dirty = value;
                _escrowTableName.Dirty = value;
                _feeVariances.Dirty = value;
                _id.Dirty = value;
                _impoundHazInsRate.Dirty = value;
                _impoundMortgInsPremRate.Dirty = value;
                _impoundMortgInsPremYearlyBasis.Dirty = value;
                _impoundTaxesRate.Dirty = value;
                _impoundType1.Dirty = value;
                _impoundType2.Dirty = value;
                _impoundType3.Dirty = value;
                _impoundType4.Dirty = value;
                _proposedMonthlyHazardInsurance.Dirty = value;
                _proposedMonthlyMortgageInsurance.Dirty = value;
                _section1000BorrowerPaidTotalAmount.Dirty = value;
                _section1000SellerPaidTotalAmount.Dirty = value;
                _settlementClosingFeeNewHudBorPaidAmount.Dirty = value;
                _settlementClosingFeeNewHudSelPaidAmount.Dirty = value;
                _titleCompanyName.Dirty = value;
                _titleExaminationNewHudSelPaidAmount.Dirty = value;
                _titleTable2010Name.Dirty = value;
                _titleTableName.Dirty = value;
                _totalForBorPaid1.Dirty = value;
                _totalForBorPaid2.Dirty = value;
                _totalForBorPaid3.Dirty = value;
                _totalForBorPaid4.Dirty = value;
                _totalForBorPaid5.Dirty = value;
                _totalForBorPaid6.Dirty = value;
                _totalForSellerPaid1.Dirty = value;
                _totalForSellerPaid2.Dirty = value;
                _totalForSellerPaid3.Dirty = value;
                _totalForSellerPaid4.Dirty = value;
                _totalForSellerPaid5.Dirty = value;
                _totalForSellerPaid6.Dirty = value;
                if (ClosingDisclosure1 != null) ClosingDisclosure1.Dirty = value;
                if (ClosingDisclosure2 != null) ClosingDisclosure2.Dirty = value;
                if (ClosingDisclosure3 != null) ClosingDisclosure3.Dirty = value;
                if (ClosingDisclosure4 != null) ClosingDisclosure4.Dirty = value;
                if (ClosingDisclosure5 != null) ClosingDisclosure5.Dirty = value;
                if (FeeVarianceOther != null) FeeVarianceOther.Dirty = value;
                if (Gfe2010 != null) Gfe2010.Dirty = value;
                if (Gfe2010Page != null) Gfe2010Page.Dirty = value;
                if (Gfe2010Section != null) Gfe2010Section.Dirty = value;
                if (LoanEstimate1 != null) LoanEstimate1.Dirty = value;
                if (LoanEstimate2 != null) LoanEstimate2.Dirty = value;
                if (LoanEstimate3 != null) LoanEstimate3.Dirty = value;
                _settingDirty = 0;
            }
        }
        bool IDirty.Dirty { get { return Dirty; } set { Dirty = value; } }
    }
}
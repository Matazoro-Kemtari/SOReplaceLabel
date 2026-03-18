using CsvHelper.Configuration;

namespace SOReplaceLabelLib.Data
{
    /// <summary>
    /// ShopOrder.txt(TSVファイル）のマッピング設定
    /// </summary>
    public class ShopOrderMap : ClassMap<ShopOrderTexts>
    {
        public ShopOrderMap()
        {
            Map(m => m.FactoryCode).Index(0);
            Map(m => m.PlaneName).Index(1);
            Map(m => m.PartsNo).Index(2);
            Map(m => m.IDNo).Index(3);
            Map(m => m.BarCode).Index(4);
            Map(m => m.Name).Index(5);
            Map(m => m.LeftPartsCount).Index(6);
            Map(m => m.RightPartsCount).Index(7);
            Map(m => m.Destination).Index(8);
            Map(m => m.RegistrantID).Index(9);
            Map(m => m.UsingShop).Index(10);
            Map(m => m.LiabilityShop).Index(11);
            Map(m => m.Order).Index(12);
            Map(m => m.Item).Index(13);
            Map(m => m.Lot).Index(14);
            Map(m => m.StartUnitNumber).Index(15);
            Map(m => m.EndUnitNumber).Index(16);
            Map(m => m.Status).Index(17);
            Map(m => m.Area).Index(18);
            Map(m => m.SequenceNumber).Index(19);
            Map(m => m.Shop).Index(20);
            Map(m => m.Finish).Index(21);
            Map(m => m.MissingSign).Index(22);
            Map(m => m.MND).Index(23);
            Map(m => m.AletFlag).Index(24);
        }
    }
}

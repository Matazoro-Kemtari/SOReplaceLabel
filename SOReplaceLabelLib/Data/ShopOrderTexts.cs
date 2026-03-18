using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOReplaceLabelLib.Data
{
    public class ShopOrderTexts : IEquatable<ShopOrderTexts>
    {
        /// <summary>
        /// 工場コード
        /// </summary>
        [Index(0)]
        public string FactoryCode { get; set; }

        /// <summary>
        /// 機種
        /// </summary>
        [Index(1)]
        public string PlaneName { get; set; }

        /// <summary>
        /// 部品番号
        /// </summary>
        [Index(2)]
        public string PartsNo { get; set; }

        /// <summary>
        /// IDNo
        /// </summary>
        [Index(3)]
        public string IDNo { get; set; }

        /// <summary>
        /// バーコード
        /// </summary>
        [Index(4)]
        public string BarCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Index(5)]
        public string Name { get; set; }

        /// <summary>
        /// 左個数
        /// </summary>
        [Index(6)]
        public string LeftPartsCount { get; set; }

        /// <summary>
        /// 右個数
        /// </summary>
        [Index(7)]
        public string RightPartsCount { get; set; }

        /// <summary>
        /// 送り先
        /// </summary>
        [Index(8)]
        public string Destination { get; set; }

        /// <summary>
        /// 登録者ID
        /// </summary>
        [Index(9)]
        public string RegistrantID { get; set; }

        /// <summary>
        /// 使用ショップ
        /// </summary>
        [Index(10)]
        public string UsingShop { get; set; }

        /// <summary>
        /// 責任ショップ
        /// </summary>
        [Index(11)]
        public string LiabilityShop { get; set; }

        /// <summary>
        /// オーダー
        /// </summary>
        [Index(12)]
        public string Order { get; set; }

        /// <summary>
        /// アイテム
        /// </summary>
        [Index(13)]
        public string Item { get; set; }

        /// <summary>
        /// ロット
        /// </summary>
        [Index(14)]
        public string Lot { get; set; }

        /// <summary>
        /// 開始号機
        /// </summary>
        [Index(15)]
        public string StartUnitNumber { get; set; }

        /// <summary>
        /// 終了号機
        /// </summary>
        [Index(16)]
        public string EndUnitNumber { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        [Index(17)]
        public string Status { get; set; }

        /// <summary>
        /// エリア
        /// </summary>
        [Index(18)]
        public string Area { get; set; }

        /// <summary>
        /// 連番
        /// </summary>
        [Index(19)]
        public string SequenceNumber { get; set; }

        /// <summary>
        /// ショップ
        /// </summary>
        [Index(20)]
        public string Shop { get; set; }

        /// <summary>
        /// 完了
        /// </summary>
        [Index(21)]
        public string Finish { get; set; }

        /// <summary>
        /// 欠品サイン
        /// </summary>
        [Index(22)]
        public string MissingSign { get; set; }

        /// <summary>
        /// MND
        /// </summary>
        [Index(23)]
        public string MND { get; set; }

        /// <summary>
        /// アラートフラグ
        /// </summary>
        [Index(24)]
        public string AletFlag { get; set; }

        /// <summary>
        /// 機種名取得
        /// </summary>
        public string GetActualPlaneName
        {
            get
            {
                switch (PlaneName)
                {
                    case "FP1":
                        return "777";
                    case "FP3":
                        return "737";
                    case "FPB":
                        return "747";
                    case "FPD":
                        return "787";
                    case "T":
                        return "767";
                    case "FPC":
                        return "MRJ";
                    default:
                        return PlaneName;
                }
            }
        }

        /// <summary>
        /// 欠品サイン取得
        /// </summary>
        public string GetActualMissingSign
        {
            get
            {
                switch (MissingSign)
                {
                    case "1":
                        return "欠品";
                    case "0":
                    case "2":
                    case "3":
                        return string.Empty;
                    default:
                        return "エラー（欠品サイン）";
                }
            }
        }

        public string[] GetTextsWithColumnNumber()
        {
            int columnNumber = 1;
            return new string[]
            {
                (columnNumber++).ToString("D2") + " " + FactoryCode,
                (columnNumber++).ToString("D2") + " " + PlaneName,
                (columnNumber++).ToString("D2") + " " + PartsNo,
                (columnNumber++).ToString("D2") + " " + IDNo,
                (columnNumber++).ToString("D2") + " " + BarCode,
                (columnNumber++).ToString("D2") + " " + Name,
                (columnNumber++).ToString("D2") + " " + LeftPartsCount,
                (columnNumber++).ToString("D2") + " " + RightPartsCount,
                (columnNumber++).ToString("D2") + " " + Destination,
                (columnNumber++).ToString("D2") + " " + RegistrantID,
                (columnNumber++).ToString("D2") + " " + UsingShop,
                (columnNumber++).ToString("D2") + " " + LiabilityShop,
                (columnNumber++).ToString("D2") + " " + Order,
                (columnNumber++).ToString("D2") + " " + Item,
                (columnNumber++).ToString("D2") + " " + Lot,
                (columnNumber++).ToString("D2") + " " + StartUnitNumber,
                (columnNumber++).ToString("D2") + " " + EndUnitNumber,
                (columnNumber++).ToString("D2") + " " + Status,
                (columnNumber++).ToString("D2") + " " + Area,
                (columnNumber++).ToString("D2") + " " + SequenceNumber,
                (columnNumber++).ToString("D2") + " " + Shop,
                (columnNumber++).ToString("D2") + " " + Finish,
                (columnNumber++).ToString("D2") + " " + MissingSign,
                (columnNumber++).ToString("D2") + " " + MND,
                (columnNumber++).ToString("D2") + " " + AletFlag,
            };
        }

        /// <summary>
        /// ラベル用連結テキストを取得
        /// </summary>
        /// <returns></returns>
        public string GetLabelLineTexts()
        {
            var line1 = GetActualPlaneName;
            var line2 = System.Text.RegularExpressions.Regex.Replace(
                Destination, "<br>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var missingSignText = GetActualMissingSign;

            if (string.IsNullOrEmpty(line2))
            {
                line2 = missingSignText;
            }
            else
            {
                if (!string.IsNullOrEmpty(missingSignText))
                {
                    line2 = line2 + " " + missingSignText;
                }
            }
            int totalCount = 0;
            if (int.TryParse(LeftPartsCount, out var leftCount))
            {
                totalCount += leftCount;
            }
            if (int.TryParse(RightPartsCount, out var rightCount))
            {
                totalCount += rightCount;
            }
            var line3 = $"{leftCount}:{rightCount}({totalCount})";

            var joinTexts = new string[] {
                    line1, line2, line3 };

            return string.Join(Environment.NewLine, joinTexts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ShopOrderTexts other)
        {
            //objがnullのときは等価でない
            if (other == null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            //Linqの遅延評価でマッチしないプロパティが見つかったら不一致と判定
            return PropertyEquals(other).All(isEqual => isEqual);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ShopOrderTexts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (PlaneName?.GetHashCode() ?? 0);
            hash = hash * 23 + (PartsNo?.GetHashCode() ?? 0);
            hash = hash * 23 + (BarCode?.GetHashCode() ?? 0);
            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            hash = hash * 23 + (Destination?.GetHashCode() ?? 0);
            hash = hash * 23 + (MissingSign?.GetHashCode() ?? 0);
            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private IEnumerable<bool> PropertyEquals(ShopOrderTexts other)
        {
            yield return other != null;
            yield return FactoryCode == other.FactoryCode;
            yield return PlaneName == other.PlaneName;
            yield return PartsNo == other.PartsNo;
            yield return IDNo == other.IDNo;
            yield return BarCode == other.BarCode;
            yield return Name == other.Name;
            yield return LeftPartsCount == other.LeftPartsCount;
            yield return RightPartsCount == other.RightPartsCount;
            yield return Destination == other.Destination;
            yield return RegistrantID == other.RegistrantID;
            yield return UsingShop == other.UsingShop;
            yield return LiabilityShop == other.LiabilityShop;
            yield return Order == other.Order;
            yield return Item == other.Item;
            yield return Lot == other.Lot;
            yield return StartUnitNumber == other.StartUnitNumber;
            yield return EndUnitNumber == other.EndUnitNumber;
            yield return Status == other.Status;
            yield return Area == other.Area;
            yield return SequenceNumber == other.SequenceNumber;
            yield return Shop == other.Shop;
            yield return Finish == other.Finish;
            yield return MissingSign == other.MissingSign;
            yield return MND == other.MND;
            yield return AletFlag == other.AletFlag;
        }
    }
}

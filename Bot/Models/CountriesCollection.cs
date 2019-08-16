using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public static class CountriesCollection
    {
        #region Fields

        public static readonly Dictionary<CountryCode, Country> Countries = new Dictionary<CountryCode, Country>()
        {
            { CountryCode.AT, new Country(CountryCode.AT, "AUSTRIA",        "AT", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.BY, new Country(CountryCode.BY, "BELARUS",        "BY", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.BE, new Country(CountryCode.BE, "BELGIUM",        "BE", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.BG, new Country(CountryCode.BG, "BULGARIA",       "BG", new ObservableCollection<CoutryRegion>()) },
            {
                CountryCode.CA, new Country(CountryCode.CA, "CANADA", "CA", new ObservableCollection<CoutryRegion>()
                {
                    new CoutryRegion("AB", "Alberta"),
                    new CoutryRegion("BC", "British Columbia"),
                    new CoutryRegion("LB", "Labrador"),
                    new CoutryRegion("MB", "Manitoba"),
                    new CoutryRegion("NB", "New Brunswick"),
                    new CoutryRegion("NF", "Newfoundland"),
                    new CoutryRegion("NS", "Nova Scotia"),
                    new CoutryRegion("NU", "Nunavut"),
                    new CoutryRegion("NW", "North West Terr."),
                    new CoutryRegion("ON", "Ontario"),
                    new CoutryRegion("PE", "Prince Edward Is."),
                    new CoutryRegion("QC", "Quebec"),
                    new CoutryRegion("SK", "Saskatchewen"),
                    new CoutryRegion("YU", "Yukon"),
                })
            },
            { CountryCode.HR, new Country(CountryCode.HR, "CROATIA",        "HR", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.CZ, new Country(CountryCode.CZ, "CZECH REPUBLIC", "CZ", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.DK, new Country(CountryCode.DK, "DENMARK",        "DK", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.EE, new Country(CountryCode.EE, "ESTONIA",        "EE", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.FI, new Country(CountryCode.FI, "FINLAND",        "FI", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.FR, new Country(CountryCode.FR, "FRANCE",         "FR", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.DE, new Country(CountryCode.DE, "GERMANY",        "DE", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.GR, new Country(CountryCode.GR, "GREECE",         "GR", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.HU, new Country(CountryCode.HU, "HUNGARY",        "HU", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.IS, new Country(CountryCode.IS, "ICELAND",        "IS", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.IE, new Country(CountryCode.IE, "IRELAND",        "IE", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.IT, new Country(CountryCode.IT, "ITALY",          "IT", new ObservableCollection<CoutryRegion>()) },
            {
                CountryCode.JP, new Country(CountryCode.JP, "JAPAN",          "JP", new ObservableCollection<CoutryRegion>()
                {
                    new CoutryRegion("三重県", "三重県 (Mie)"),
                    new CoutryRegion("京都府", "京都府 (Kyoto)"),
                    new CoutryRegion("佐賀県", "佐賀県 (Saga)"),
                    new CoutryRegion("兵庫県", "兵庫県 (Hyōgo)"),
                    new CoutryRegion("北海道", "北海道 (Hokkaido)"),
                    new CoutryRegion("千葉県", "千葉県 (Chiba)"),
                    new CoutryRegion("和歌山県", "和歌山県 (Wakayama)"),
                    new CoutryRegion("埼玉県", "埼玉県 (Saitama)"),
                    new CoutryRegion("大分県", "大分県 (Ōita)"),
                    new CoutryRegion("大阪府", "大阪府 (Osaka)"),
                    new CoutryRegion("奈良県", "奈良県 (Nara)"),
                    new CoutryRegion("宮城県", "宮城県 (Miyagi)"),
                    new CoutryRegion("宮崎県", "宮崎県 (Miyazaki)"),
                    new CoutryRegion("富山県", "富山県 (Toyama)"),
                    new CoutryRegion("山口県", "山口県 (Yamaguchi)"),
                    new CoutryRegion("山形県", "山形県 (Yamagata)"),
                    new CoutryRegion("山梨県", "山梨県 (Yamanashi)"),
                    new CoutryRegion("岐阜県", "岐阜県 (Gifu)"),
                    new CoutryRegion("岡山県", "岡山県 (Okayama)"),
                    new CoutryRegion("岩手県", "岩手県 (Iwate)"),
                    new CoutryRegion("島根県", "島根県 (Shimane)"),
                    new CoutryRegion("広島県", "広島県 (Hiroshima)"),
                    new CoutryRegion("徳島県", "徳島県 (Tokushima)"),
                    new CoutryRegion("愛媛県", "愛媛県 (Ehime)"),
                    new CoutryRegion("愛知県", "愛知県 (Aichi)"),
                    new CoutryRegion("新潟県", "新潟県 (Niigata)"),
                    new CoutryRegion("東京都", "東京都 (Tokyo)"),
                    new CoutryRegion("栃木県", "栃木県 (Tochigi)"),
                    new CoutryRegion("沖縄県", "沖縄県 (Okinawa)"),
                    new CoutryRegion("滋賀県", "滋賀県 (Shiga)"),
                    new CoutryRegion("熊本県", "熊本県 (Kumamoto)"),
                    new CoutryRegion("石川県", "石川県 (Ishikawa)"),
                    new CoutryRegion("神奈川県", "神奈川県 (Kanagawa)"),
                    new CoutryRegion("福井県", "福井県 (Fukui)"),
                    new CoutryRegion("福岡県", "福岡県 (Fukuoka)"),
                    new CoutryRegion("福島県", "福島県 (Fukushima)"),
                    new CoutryRegion("秋田県", "秋田県 (Akita)"),
                    new CoutryRegion("群馬県", "群馬県 (Gunma)"),
                    new CoutryRegion("茨城県", "茨城県 (Ibaraki)"),
                    new CoutryRegion("長崎県", "長崎県 (Nagasaki)"),
                    new CoutryRegion("長野県", "長野県 (Nagano)"),
                    new CoutryRegion("青森県", "青森県 (Aomori)"),
                    new CoutryRegion("静岡県", "静岡県 (Shizuoka)"),
                    new CoutryRegion("香川県", "香川県 (Kagawa)"),
                    new CoutryRegion("高知県", "高知県 (Kōchi)"),
                    new CoutryRegion("鳥取県", "鳥取県 (Tottori)"),
                    new CoutryRegion("鹿児島県", "鹿児島県 (Kagoshima)"),
                })
            },
            { CountryCode.LV, new Country(CountryCode.LV, "LATVIA",         "LV", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.LT, new Country(CountryCode.LT, "LITHUANIA",      "LT", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.LU, new Country(CountryCode.LU, "LUXEMBOURG",     "LU", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.MC, new Country(CountryCode.MC, "MONACO",         "MC", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.NL, new Country(CountryCode.NL, "NETHERLANDS",    "NL", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.NO, new Country(CountryCode.NO, "NORWAY",         "NO", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.PL, new Country(CountryCode.PL, "POLAND",         "PL", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.PT, new Country(CountryCode.PT, "PORTUGAL",       "PT", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.RO, new Country(CountryCode.RO, "ROMANIA",        "RO", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.RU, new Country(CountryCode.RU, "RUSSIA",         "RU", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.SK, new Country(CountryCode.SK, "SLOVAKIA",       "SK", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.SI, new Country(CountryCode.SI, "SLOVENIA",       "SI", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.ES, new Country(CountryCode.ES, "SPAIN",          "ES", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.SE, new Country(CountryCode.SE, "SWEDEN",         "SE", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.CH, new Country(CountryCode.CH, "SWITZERLAND",    "CH", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.TR, new Country(CountryCode.TR, "TURKEY",         "TR", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.UK, new Country(CountryCode.UK, "United Kingdom", "UK", new ObservableCollection<CoutryRegion>()) },
            { CountryCode.NB, new Country(CountryCode.NB, "UK (N. Ireland)","NB", new ObservableCollection<CoutryRegion>()) },
            {
                CountryCode.US, new Country(CountryCode.US, "USA", "US", new ObservableCollection<CoutryRegion>()
                {
                    new CoutryRegion("AK", "Alaska"),
                    new CoutryRegion("AZ", "Arizona"),
                    new CoutryRegion("AR", "Arkansas"),
                    new CoutryRegion("CA", "California"),
                    new CoutryRegion("CO", "Colorado"),
                    new CoutryRegion("CT", "Connecticut"),
                    new CoutryRegion("DE", "Delaware"),
                    new CoutryRegion("DC", "District Of Columbia"),
                    new CoutryRegion("FL", "Florida"),
                    new CoutryRegion("GA", "Georgia"),
                    new CoutryRegion("HI", "Hawaii"),
                    new CoutryRegion("ID", "Idaho"),
                    new CoutryRegion("IL", "Illinois"),
                    new CoutryRegion("IN", "Indiana"),
                    new CoutryRegion("IA", "Iowa"),
                    new CoutryRegion("KS", "Kansas"),
                    new CoutryRegion("KY", "Kentucky"),
                    new CoutryRegion("LA", "Louisiana"),
                    new CoutryRegion("ME", "Maine"),
                    new CoutryRegion("MD", "Maryland"),
                    new CoutryRegion("MA", "Massachusetts"),
                    new CoutryRegion("MI", "Michigan"),
                    new CoutryRegion("MN", "Minnesota"),
                    new CoutryRegion("MS", "Mississippi"),
                    new CoutryRegion("MO", "Missouri"),
                    new CoutryRegion("MT", "Montana"),
                    new CoutryRegion("NE", "Nebraska"),
                    new CoutryRegion("NV", "Nevada"),
                    new CoutryRegion("NH", "New Hampshire"),
                    new CoutryRegion("NJ", "New Jersey"),
                    new CoutryRegion("NM", "New Mexico"),
                    new CoutryRegion("NY", "New York"),
                    new CoutryRegion("NC", "North Carolina"),
                    new CoutryRegion("ND", "North Dakota"),
                    new CoutryRegion("OH", "Ohio"),
                    new CoutryRegion("OK", "Oklahoma"),
                    new CoutryRegion("OR", "Oregon"),
                    new CoutryRegion("PA", "Pennsylvania"),
                    new CoutryRegion("RI", "Rhode Island"),
                    new CoutryRegion("SC", "South Carolina"),
                    new CoutryRegion("SD", "South Dakota"),
                    new CoutryRegion("TN", "Tennessee"),
                    new CoutryRegion("TX", "Texas"),
                    new CoutryRegion("UT", "Utah"),
                    new CoutryRegion("VT", "Vermont"),
                    new CoutryRegion("VA", "Virginia"),
                    new CoutryRegion("WA", "Washington"),
                    new CoutryRegion("WV", "West Virginia"),
                    new CoutryRegion("WI", "Wisconsin"),
                    new CoutryRegion("WY", "Wyoming"),
                    new CoutryRegion("AL", "Alabama")
                })
            }
        };

        public static Dictionary<CountryCode, Country>.ValueCollection List => Countries.Values;

        #endregion
    }
}

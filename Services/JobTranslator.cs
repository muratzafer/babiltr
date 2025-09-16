namespace babiltr.Services
{
    public class JobCategoryTranslator
    {
        public static string TranslateJobType(string type)
        {
            switch (type)
            {
                case "TR":
                    return "Çeviri";
                case "LO":
                    return "Yerelleştirme";
                case "ED":
                    return "Düzenleme";
                case "PR":
                    return "Son Okuma";
                case "RE":
                    return "Redaksiyon";
                case "CW":
                    return "İçerik Üretimi";
                default:
                    return type;
            }
        }
        public static string TranslateJobCategory(string category)
        {
            switch (category)
            {
                case "PB":
                    return "Proje Bazlı";
                case "OT":
                    return "Tek Seferlik";
                case "LT":
                    return "Uzun Süreli";
                case "PT":
                    return "Yarı Zamanlı";
                case "IN":
                    return "Staj";
                case "OTR":
                    return "Diğer";
                default:
                    return category;
            }
        }

        public static string TranslateCountry(string countryCode)
        {
            switch (countryCode)
            {
                case "NULL":
                    return "Seçiniz";
                case "TUR":
                    return "Türkiye";
                case "USA":
                    return "Amerika Birleşik Devletleri";
                case "CAD":
                    return "Kanada";
                case "CHINA":
                    return "Çin";
                case "UK":
                    return "Birleşik Krallık";
                case "GER":
                    return "Almanya";
                case "FRA":
                    return "Fransa";
                case "ITA":
                    return "İtalya";
                case "JPN":
                    return "Japonya";
                case "AUS":
                    return "Avustralya";
                case "BRA":
                    return "Brezilya";
                case "IND":
                    return "Hindistan";
                case "MEX":
                    return "Meksika";
                case "RUS":
                    return "Rusya";
                case "ESP":
                    return "İspanya";
                case "ARG":
                    return "Arjantin";
                case "KOR":
                    return "Güney Kore";
                case "SAU":
                    return "Suudi Arabistan";
                case "SAF":
                    return "Güney Afrika";
                case "NED":
                    return "Hollanda";
                case "SWE":
                    return "İsveç";
                case "NOR":
                    return "Norveç";
                case "SWI":
                    return "İsviçre";
                case "DEN":
                    return "Danimarka";
                case "BEL":
                    return "Belçika";
                case "AUT":
                    return "Avusturya";
                case "NZ":
                    return "Yeni Zelanda";
                case "POL":
                    return "Polonya";
                case "EGY":
                    return "Mısır";
                case "ISR":
                    return "İsrail";
                default:
                    return countryCode;
            }
        }

        public static string TranslateCity(string cityCode)
        {
            switch (cityCode)
            {
                case "AD":
                    return "Adana";
                case "ADI":
                    return "Adıyaman";
                case "AFY":
                    return "Afyonkarahisar";
                case "AGR":
                    return "Ağrı";
                case "AKS":
                    return "Aksaray";
                case "AMA":
                    return "Amasya";
                case "ANK":
                    return "Ankara";
                case "ANT":
                    return "Antalya";
                case "ART":
                    return "Artvin";
                case "AYD":
                    return "Aydın";
                case "BAL":
                    return "Balıkesir";
                case "BAR":
                    return "Bartın";
                case "BAT":
                    return "Batman";
                case "BAY":
                    return "Bayburt";
                case "BIL":
                    return "Bilecik";
                case "BIN":
                    return "Bingöl";
                case "BIT":
                    return "Bitlis";
                case "BOL":
                    return "Bolu";
                case "BRD":
                    return "Burdur";
                case "BRS":
                    return "Bursa";
                case "CAN":
                    return "Çanakkale";
                case "CKR":
                    return "Çankırı";
                case "COR":
                    return "Çorum";
                case "DEN":
                    return "Denizli";
                case "DIY":
                    return "Diyarbakır";
                case "DUZ":
                    return "Düzce";
                case "EDI":
                    return "Edirne";
                case "ELA":
                    return "Elazığ";
                case "ERZ":
                    return "Erzincan";
                case "ERU":
                    return "Erzurum";
                case "ESK":
                    return "Eskişehir";
                case "GAZ":
                    return "Gaziantep";
                case "GIR":
                    return "Giresun";
                case "GMS":
                    return "Gümüşhane";
                case "HAK":
                    return "Hakkâri";
                case "HTY":
                    return "Hatay";
                case "IGD":
                    return "Iğdır";
                case "ISP":
                    return "Isparta";
                case "IST":
                    return "İstanbul";
                case "IZM":
                    return "İzmir";
                case "KAH":
                    return "Kahramanmaraş";
                case "KRB":
                    return "Karabük";
                case "KRM":
                    return "Karaman";
                case "KRS":
                    return "Kars";
                case "KAS":
                    return "Kastamonu";
                case "KAY":
                    return "Kayseri";
                case "KIL":
                    return "Kilis";
                case "KIR":
                    return "Kırıkkale";
                case "KIC":
                    return "Kırklareli";
                case "KRH":
                    return "Kırşehir";
                case "KOC":
                    return "Kocaeli";
                case "KON":
                    return "Konya";
                case "KUT":
                    return "Kütahya";
                case "MAL":
                    return "Malatya";
                case "MAN":
                    return "Manisa";
                case "MAR":
                    return "Mardin";
                case "MER":
                    return "Mersin";
                case "MUG":
                    return "Muğla";
                case "MUS":
                    return "Muş";
                case "NEV":
                    return "Nevşehir";
                case "NIG":
                    return "Niğde";
                case "ORD":
                    return "Ordu";
                case "OSM":
                    return "Osmaniye";
                case "RIZ":
                    return "Rize";
                case "SAK":
                    return "Sakarya";
                case "SAM":
                    return "Samsun";
                case "SAN":
                    return "Şanlıurfa";
                case "SIR":
                    return "Siirt";
                case "SIN":
                    return "Sinop";
                case "SIV":
                    return "Sivas";
                case "SRT":
                    return "Şırnak";
                case "TEK":
                    return "Tekirdağ";
                case "TOK":
                    return "Tokat";
                case "TRA":
                    return "Trabzon";
                case "TUN":
                    return "Tunceli";
                case "USK":
                    return "Uşak";
                case "VAN":
                    return "Van";
                case "YAL":
                    return "Yalova";
                case "YOZ":
                    return "Yozgat";
                case "ZON":
                    return "Zonguldak";
                default:
                    return cityCode;
            }
        }
    }
}


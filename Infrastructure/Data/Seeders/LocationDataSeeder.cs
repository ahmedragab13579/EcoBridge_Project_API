using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Models;
using EcoBridge.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcoBridge.Infrastructure.Data.Seeders;

public class LocationDataSeeder(IEcoBridgeDbContext dbContext, RoleManager<ApplicationRole> roleM, ILogger<LocationDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {


            var existingRoles = await roleM.Roles.Select(r => r.NormalizedName).ToListAsync(cancellationToken);
            var rolesToSeed = new[] { "DONOR", "ADMIN", "CHARITY", "VOLUNTEER" };
            var rolesAdded = false;

            foreach (var r in rolesToSeed)
            {
                if (!existingRoles.Contains(r))
                {
                    await roleM.CreateAsync(new ApplicationRole(r));
                    rolesAdded = true;
                }
            }

            if (rolesAdded)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var egypt = await dbContext.Countries
              .Include(c => c.Governorates)
              .ThenInclude(g => g.Cities)
              .FirstOrDefaultAsync(c => c.Code == "EG", cancellationToken);

            if (egypt == null)
            {
                logger.LogInformation("Seeding Egypt country, governorates, and cities...");

                egypt = new Country
                {
                    Code = "EG",
                    NameAr = "مصر",
                    NameEn = "Egypt"
                };

                dbContext.Countries.Add(egypt);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Created Country 'Egypt' (Id: {CountryId})", egypt.Id);
            }

            var existingGovs = await dbContext.Governorates
              .Where(g => g.CountryId == egypt.Id)
              .ToDictionaryAsync(g => g.Code, cancellationToken);

            var seedGovs = GetGovernoratesData();
            var addedGovCount = 0;
            var addedCityCount = 0;

            foreach (var (govCode, govNameAr, govNameEn, govCities) in seedGovs)
            {
                if (!existingGovs.TryGetValue(govCode, out var governorate))
                {
                    governorate = new Governorate
                    {
                        CountryId = egypt.Id,
                        Code = govCode,
                        NameAr = govNameAr,
                        NameEn = govNameEn
                    };

                    dbContext.Governorates.Add(governorate);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    existingGovs[govCode] = governorate;
                    addedGovCount++;
                }

                var existingCityCodes = await dbContext.Cities
                  .Where(c => c.GovernorateId == governorate.Id)
                  .Select(c => c.Code)
                  .ToHashSetAsync(cancellationToken);

                var citiesToAdd = new List<City>();
                foreach (var (cityCode, cityNameAr, cityNameEn) in govCities)
                {
                    if (!existingCityCodes.Contains(cityCode))
                    {
                        citiesToAdd.Add(new City
                        {
                            GovernorateId = governorate.Id,
                            Code = cityCode,
                            NameAr = cityNameAr,
                            NameEn = cityNameEn
                        });
                    }
                }

                if (citiesToAdd.Count > 0)
                {
                    await dbContext.Cities.AddRangeAsync(citiesToAdd, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    addedCityCount += citiesToAdd.Count;
                }
            }

            logger.LogInformation("Location data seeding completed. Added {GovCount} governorates and {CityCount} cities.", addedGovCount, addedCityCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding location data.");
            throw;
        }
    }

    private static List<(string GovCode, string NameAr, string NameEn, List<(string CityCode, string CityNameAr, string CityNameEn)> Cities)> GetGovernoratesData()
    {
        return new List<(string, string, string, List<(string, string, string)>)>
    {
      (
        "EG-01", "القاهرة", "Cairo",
        new List<(string, string, string)>
        {
          ("EG-01-01", "15 مايو", "15 May"),
          ("EG-01-02", "الازبكية", "Al Azbakeyah"),
          ("EG-01-03", "البساتين", "Al Basatin"),
          ("EG-01-04", "التبين", "Tebin"),
          ("EG-01-05", "الخليفة", "El-Khalifa"),
          ("EG-01-06", "الدراسة", "El darrasa"),
          ("EG-01-07", "الدرب الاحمر", "Aldarb Alahmar"),
          ("EG-01-08", "الزاوية الحمراء", "Zawya al-Hamra"),
          ("EG-01-09", "الزيتون", "El-Zaytoun"),
          ("EG-01-10", "الساحل", "Sahel"),
          ("EG-01-11", "السلام", "El Salam"),
          ("EG-01-12", "السيدة زينب", "Sayeda Zeinab"),
          ("EG-01-13", "الشرابية", "El Sharabeya"),
          ("EG-01-14", "مدينة الشروق", "Shorouk"),
          ("EG-01-15", "الظاهر", "El Daher"),
          ("EG-01-16", "العتبة", "Ataba"),
          ("EG-01-17", "القاهرة الجديدة", "New Cairo"),
          ("EG-01-18", "المرج", "El Marg"),
          ("EG-01-19", "عزبة النخل", "Ezbet el Nakhl"),
          ("EG-01-20", "المطرية", "Matareya"),
        }
      ),
      (
        "EG-02", "الجيزة", "Giza",
        new List<(string, string, string)>
        {
          ("EG-02-01", "الجيزة", "Giza"),
          ("EG-02-02", "السادس من أكتوبر", "Sixth of October"),
          ("EG-02-03", "الشيخ زايد", "Cheikh Zayed"),
          ("EG-02-04", "الحوامدية", "Hawamdiyah"),
          ("EG-02-05", "البدرشين", "Al Badrasheen"),
          ("EG-02-06", "الصف", "Saf"),
          ("EG-02-07", "أطفيح", "Atfih"),
          ("EG-02-08", "العياط", "Al Ayat"),
          ("EG-02-09", "الباويطي", "Al-Bawaiti"),
          ("EG-02-10", "منشأة القناطر", "ManshiyetAl Qanater"),
          ("EG-02-11", "أوسيم", "Oaseem"),
          ("EG-02-12", "كرداسة", "Kerdasa"),
          ("EG-02-13", "أبو النمرس", "Abu Nomros"),
          ("EG-02-14", "كفر غطاطي", "Kafr Ghati"),
          ("EG-02-15", "منشأة البكاري", "Manshiyet Al Bakari"),
          ("EG-02-16", "الدقى", "Dokki"),
          ("EG-02-17", "العجوزة", "Agouza"),
          ("EG-02-18", "الهرم", "Haram"),
          ("EG-02-19", "الوراق", "Warraq"),
          ("EG-02-20", "امبابة", "Imbaba"),
        }
      ),
      (
        "EG-03", "الأسكندرية", "Alexandria",
        new List<(string, string, string)>
        {
          ("EG-03-01", "ابو قير", "Abu Qir"),
          ("EG-03-02", "الابراهيمية", "Al Ibrahimeyah"),
          ("EG-03-03", "الأزاريطة", "Azarita"),
          ("EG-03-04", "الانفوشى", "Anfoushi"),
          ("EG-03-05", "الدخيلة", "Dekheila"),
          ("EG-03-06", "السيوف", "El Soyof"),
          ("EG-03-07", "العامرية", "Ameria"),
          ("EG-03-08", "اللبان", "El Labban"),
          ("EG-03-09", "المفروزة", "Al Mafrouza"),
          ("EG-03-10", "المنتزه", "El Montaza"),
          ("EG-03-11", "المنشية", "Mansheya"),
          ("EG-03-12", "الناصرية", "Naseria"),
          ("EG-03-13", "امبروزو", "Ambrozo"),
          ("EG-03-14", "باب شرق", "Bab Sharq"),
          ("EG-03-15", "برج العرب", "Bourj Alarab"),
          ("EG-03-16", "ستانلى", "Stanley"),
          ("EG-03-17", "سموحة", "Smouha"),
          ("EG-03-18", "سيدى بشر", "Sidi Bishr"),
          ("EG-03-19", "شدس", "Shads"),
          ("EG-03-20", "غيط العنب", "Gheet Alenab"),
        }
      ),
      (
        "EG-04", "الدقهلية", "Dakahlia",
        new List<(string, string, string)>
        {
          ("EG-04-01", "المنصورة", "Mansoura"),
          ("EG-04-02", "طلخا", "Talkha"),
          ("EG-04-03", "ميت غمر", "Mitt Ghamr"),
          ("EG-04-04", "دكرنس", "Dekernes"),
          ("EG-04-05", "أجا", "Aga"),
          ("EG-04-06", "منية النصر", "Menia El Nasr"),
          ("EG-04-07", "السنبلاوين", "Sinbillawin"),
          ("EG-04-08", "الكردي", "El Kurdi"),
          ("EG-04-09", "بني عبيد", "Bani Ubaid"),
          ("EG-04-10", "المنزلة", "Al Manzala"),
          ("EG-04-11", "تمي الأمديد", "tami al'amdid"),
          ("EG-04-12", "الجمالية", "aljamalia"),
          ("EG-04-13", "شربين", "Sherbin"),
          ("EG-04-14", "المطرية", "Mataria"),
          ("EG-04-15", "بلقاس", "Belqas"),
          ("EG-04-16", "ميت سلسيل", "Meet Salsil"),
          ("EG-04-17", "جمصة", "Gamasa"),
          ("EG-04-18", "محلة دمنة", "Mahalat Damana"),
          ("EG-04-19", "نبروه", "Nabroh"),
        }
      ),
      (
        "EG-05", "البحر الأحمر", "Red Sea",
        new List<(string, string, string)>
        {
          ("EG-05-01", "الغردقة", "Hurghada"),
          ("EG-05-02", "رأس غارب", "Ras Ghareb"),
          ("EG-05-03", "سفاجا", "Safaga"),
          ("EG-05-04", "القصير", "El Qusiar"),
          ("EG-05-05", "مرسى علم", "Marsa Alam"),
          ("EG-05-06", "الشلاتين", "Shalatin"),
          ("EG-05-07", "حلايب", "Halaib"),
          ("EG-05-08", "الدهار", "Aldahar"),
        }
      ),
      (
        "EG-06", "البحيرة", "Beheira",
        new List<(string, string, string)>
        {
          ("EG-06-01", "دمنهور", "Damanhour"),
          ("EG-06-02", "كفر الدوار", "Kafr El Dawar"),
          ("EG-06-03", "رشيد", "Rashid"),
          ("EG-06-04", "إدكو", "Edco"),
          ("EG-06-05", "أبو المطامير", "Abu al-Matamir"),
          ("EG-06-06", "أبو حمص", "Abu Homs"),
          ("EG-06-07", "الدلنجات", "Delengat"),
          ("EG-06-08", "المحمودية", "Mahmoudiyah"),
          ("EG-06-09", "الرحمانية", "Rahmaniyah"),
          ("EG-06-10", "إيتاي البارود", "Itai Baroud"),
          ("EG-06-11", "حوش عيسى", "Housh Eissa"),
          ("EG-06-12", "شبراخيت", "Shubrakhit"),
          ("EG-06-13", "كوم حمادة", "Kom Hamada"),
          ("EG-06-14", "بدر", "Badr"),
          ("EG-06-15", "وادي النطرون", "Wadi Natrun"),
          ("EG-06-16", "النوبارية الجديدة", "New Nubaria"),
          ("EG-06-17", "النوبارية", "Alnoubareya"),
        }
      ),
      (
        "EG-07", "الفيوم", "Fayoum",
        new List<(string, string, string)>
        {
          ("EG-07-01", "الفيوم", "Fayoum"),
          ("EG-07-02", "الفيوم الجديدة", "Fayoum El Gedida"),
          ("EG-07-03", "طامية", "Tamiya"),
          ("EG-07-04", "سنورس", "Snores"),
          ("EG-07-05", "إطسا", "Etsa"),
          ("EG-07-06", "إبشواي", "Epschway"),
          ("EG-07-07", "يوسف الصديق", "Yusuf El Sediaq"),
          ("EG-07-08", "الحادقة", "Hadqa"),
          ("EG-07-09", "اطسا", "Atsa"),
          ("EG-07-10", "الجامعة", "Algamaa"),
          ("EG-07-11", "السيالة", "Sayala"),
        }
      ),
      (
        "EG-08", "الغربية", "Gharbiya",
        new List<(string, string, string)>
        {
          ("EG-08-01", "طنطا", "Tanta"),
          ("EG-08-02", "المحلة الكبرى", "Al Mahalla Al Kobra"),
          ("EG-08-03", "كفر الزيات", "Kafr El Zayat"),
          ("EG-08-04", "زفتى", "Zefta"),
          ("EG-08-05", "السنطة", "El Santa"),
          ("EG-08-06", "قطور", "Qutour"),
          ("EG-08-07", "بسيون", "Basion"),
          ("EG-08-08", "سمنود", "Samannoud"),
        }
      ),
      (
        "EG-09", "الإسماعلية", "Ismailia",
        new List<(string, string, string)>
        {
          ("EG-09-01", "الإسماعيلية", "Ismailia"),
          ("EG-09-02", "فايد", "Fayed"),
          ("EG-09-03", "القنطرة شرق", "Qantara Sharq"),
          ("EG-09-04", "القنطرة غرب", "Qantara Gharb"),
          ("EG-09-05", "التل الكبير", "El Tal El Kabier"),
          ("EG-09-06", "أبو صوير", "Abu Sawir"),
          ("EG-09-07", "القصاصين الجديدة", "Kasasien El Gedida"),
          ("EG-09-08", "نفيشة", "Nefesha"),
          ("EG-09-09", "الشيخ زايد", "Sheikh Zayed"),
        }
      ),
      (
        "EG-10", "المنوفية", "Menofia",
        new List<(string, string, string)>
        {
          ("EG-10-01", "شبين الكوم", "Shbeen El Koom"),
          ("EG-10-02", "مدينة السادات", "Sadat City"),
          ("EG-10-03", "منوف", "Menouf"),
          ("EG-10-04", "سرس الليان", "Sars El-Layan"),
          ("EG-10-05", "أشمون", "Ashmon"),
          ("EG-10-06", "الباجور", "Al Bagor"),
          ("EG-10-07", "قويسنا", "Quesna"),
          ("EG-10-08", "بركة السبع", "Berkat El Saba"),
          ("EG-10-09", "تلا", "Tala"),
          ("EG-10-10", "الشهداء", "Al Shohada"),
        }
      ),
      (
        "EG-11", "المنيا", "Minya",
        new List<(string, string, string)>
        {
          ("EG-11-01", "المنيا", "Minya"),
          ("EG-11-02", "المنيا الجديدة", "Minya El Gedida"),
          ("EG-11-03", "العدوة", "El Adwa"),
          ("EG-11-04", "مغاغة", "Magagha"),
          ("EG-11-05", "بني مزار", "Bani Mazar"),
          ("EG-11-06", "مطاي", "Mattay"),
          ("EG-11-07", "سمالوط", "Samalut"),
          ("EG-11-08", "المدينة الفكرية", "Madinat El Fekria"),
          ("EG-11-09", "ملوي", "Meloy"),
          ("EG-11-10", "دير مواس", "Deir Mawas"),
          ("EG-11-11", "ابو قرقاص", "Abu Qurqas"),
          ("EG-11-12", "ارض سلطان", "Ard Sultan"),
        }
      ),
      (
        "EG-12", "القليوبية", "Qaliubiya",
        new List<(string, string, string)>
        {
          ("EG-12-01", "بنها", "Banha"),
          ("EG-12-02", "قليوب", "Qalyub"),
          ("EG-12-03", "شبرا الخيمة", "Shubra Al Khaimah"),
          ("EG-12-04", "القناطر الخيرية", "Al Qanater Charity"),
          ("EG-12-05", "الخانكة", "Khanka"),
          ("EG-12-06", "كفر شكر", "Kafr Shukr"),
          ("EG-12-07", "طوخ", "Tukh"),
          ("EG-12-08", "قها", "Qaha"),
          ("EG-12-09", "العبور", "Obour"),
          ("EG-12-10", "الخصوص", "Khosous"),
          ("EG-12-11", "شبين القناطر", "Shibin Al Qanater"),
          ("EG-12-12", "مسطرد", "Mostorod"),
        }
      ),
      (
        "EG-13", "الوادي الجديد", "New Valley",
        new List<(string, string, string)>
        {
          ("EG-13-01", "الخارجة", "El Kharga"),
          ("EG-13-02", "باريس", "Paris"),
          ("EG-13-03", "موط", "Mout"),
          ("EG-13-04", "الفرافرة", "Farafra"),
          ("EG-13-05", "بلاط", "Balat"),
          ("EG-13-06", "الداخلة", "Dakhla"),
        }
      ),
      (
        "EG-14", "السويس", "Suez",
        new List<(string, string, string)>
        {
          ("EG-14-01", "السويس", "Suez"),
          ("EG-14-02", "الجناين", "Alganayen"),
          ("EG-14-03", "عتاقة", "Ataqah"),
          ("EG-14-04", "العين السخنة", "Ain Sokhna"),
          ("EG-14-05", "فيصل", "Faysal"),
        }
      ),
      (
        "EG-15", "اسوان", "Aswan",
        new List<(string, string, string)>
        {
          ("EG-15-01", "أسوان", "Aswan"),
          ("EG-15-02", "أسوان الجديدة", "Aswan El Gedida"),
          ("EG-15-03", "دراو", "Drau"),
          ("EG-15-04", "كوم أمبو", "Kom Ombo"),
          ("EG-15-05", "نصر النوبة", "Nasr Al Nuba"),
          ("EG-15-06", "كلابشة", "Kalabsha"),
          ("EG-15-07", "إدفو", "Edfu"),
          ("EG-15-08", "الرديسية", "Al-Radisiyah"),
          ("EG-15-09", "البصيلية", "Al Basilia"),
          ("EG-15-10", "السباعية", "Al Sibaeia"),
          ("EG-15-11", "ابوسمبل السياحية", "Abo Simbl Al Siyahia"),
          ("EG-15-12", "مرسى علم", "Marsa Alam"),
        }
      ),
      (
        "EG-16", "اسيوط", "Assiut",
        new List<(string, string, string)>
        {
          ("EG-16-01", "أسيوط", "Assiut"),
          ("EG-16-02", "أسيوط الجديدة", "Assiut El Gedida"),
          ("EG-16-03", "ديروط", "Dayrout"),
          ("EG-16-04", "منفلوط", "Manfalut"),
          ("EG-16-05", "القوصية", "Qusiya"),
          ("EG-16-06", "أبنوب", "Abnoub"),
          ("EG-16-07", "أبو تيج", "Abu Tig"),
          ("EG-16-08", "الغنايم", "El Ghanaim"),
          ("EG-16-09", "ساحل سليم", "Sahel Selim"),
          ("EG-16-10", "البداري", "El Badari"),
          ("EG-16-11", "صدفا", "Sidfa"),
        }
      ),
      (
        "EG-17", "بني سويف", "Beni Suef",
        new List<(string, string, string)>
        {
          ("EG-17-01", "بني سويف", "Bani Sweif"),
          ("EG-17-02", "بني سويف الجديدة", "Beni Suef El Gedida"),
          ("EG-17-03", "الواسطى", "Al Wasta"),
          ("EG-17-04", "ناصر", "Naser"),
          ("EG-17-05", "إهناسيا", "Ehnasia"),
          ("EG-17-06", "ببا", "beba"),
          ("EG-17-07", "الفشن", "Fashn"),
          ("EG-17-08", "سمسطا", "Somasta"),
          ("EG-17-09", "الاباصيرى", "Alabbaseri"),
          ("EG-17-10", "مقبل", "Mokbel"),
        }
      ),
      (
        "EG-18", "بورسعيد", "Port Said",
        new List<(string, string, string)>
        {
          ("EG-18-01", "بورسعيد", "PorSaid"),
          ("EG-18-02", "بورفؤاد", "Port Fouad"),
          ("EG-18-03", "العرب", "Alarab"),
          ("EG-18-04", "حى الزهور", "Zohour"),
          ("EG-18-05", "حى الشرق", "Alsharq"),
          ("EG-18-06", "حى الضواحى", "Aldawahi"),
          ("EG-18-07", "حى المناخ", "Almanakh"),
          ("EG-18-08", "حى مبارك", "Mubarak"),
        }
      ),
      (
        "EG-19", "دمياط", "Damietta",
        new List<(string, string, string)>
        {
          ("EG-19-01", "دمياط", "Damietta"),
          ("EG-19-02", "دمياط الجديدة", "New Damietta"),
          ("EG-19-03", "رأس البر", "Ras El Bar"),
          ("EG-19-04", "فارسكور", "Faraskour"),
          ("EG-19-05", "الزرقا", "Zarqa"),
          ("EG-19-06", "السرو", "alsaru"),
          ("EG-19-07", "الروضة", "alruwda"),
          ("EG-19-08", "كفر البطيخ", "Kafr El-Batikh"),
          ("EG-19-09", "عزبة البرج", "Azbet Al Burg"),
          ("EG-19-10", "ميت أبو غالب", "Meet Abou Ghalib"),
          ("EG-19-11", "كفر سعد", "Kafr Saad"),
        }
      ),
      (
        "EG-20", "الشرقية", "Sharkia",
        new List<(string, string, string)>
        {
          ("EG-20-01", "الزقازيق", "Zagazig"),
          ("EG-20-02", "العاشر من رمضان", "Al Ashr Men Ramadan"),
          ("EG-20-03", "منيا القمح", "Minya Al Qamh"),
          ("EG-20-04", "بلبيس", "Belbeis"),
          ("EG-20-05", "مشتول السوق", "Mashtoul El Souq"),
          ("EG-20-06", "القنايات", "Qenaiat"),
          ("EG-20-07", "أبو حماد", "Abu Hammad"),
          ("EG-20-08", "القرين", "El Qurain"),
          ("EG-20-09", "ههيا", "Hehia"),
          ("EG-20-10", "أبو كبير", "Abu Kabir"),
          ("EG-20-11", "فاقوس", "Faccus"),
          ("EG-20-12", "الصالحية الجديدة", "El Salihia El Gedida"),
          ("EG-20-13", "الإبراهيمية", "Al Ibrahimiyah"),
          ("EG-20-14", "ديرب نجم", "Deirb Negm"),
          ("EG-20-15", "كفر صقر", "Kafr Saqr"),
          ("EG-20-16", "أولاد صقر", "Awlad Saqr"),
          ("EG-20-17", "الحسينية", "Husseiniya"),
          ("EG-20-18", "صان الحجر القبلية", "san alhajar alqablia"),
          ("EG-20-19", "منشأة أبو عمر", "Manshayat Abu Omar"),
        }
      ),
      (
        "EG-21", "جنوب سيناء", "South Sinai",
        new List<(string, string, string)>
        {
          ("EG-21-01", "الطور", "Al Toor"),
          ("EG-21-02", "شرم الشيخ", "Sharm El-Shaikh"),
          ("EG-21-03", "دهب", "Dahab"),
          ("EG-21-04", "نويبع", "Nuweiba"),
          ("EG-21-05", "طابا", "Taba"),
          ("EG-21-06", "سانت كاترين", "Saint Catherine"),
          ("EG-21-07", "أبو رديس", "Abu Redis"),
          ("EG-21-08", "أبو زنيمة", "Abu Zenaima"),
          ("EG-21-09", "رأس سدر", "Ras Sidr"),
        }
      ),
      (
        "EG-22", "كفر الشيخ", "Kafr Al sheikh",
        new List<(string, string, string)>
        {
          ("EG-22-01", "كفر الشيخ", "Kafr El Sheikh"),
          ("EG-22-02", "وسط البلد كفر الشيخ", "Kafr El Sheikh Downtown"),
          ("EG-22-03", "دسوق", "Desouq"),
          ("EG-22-04", "فوه", "Fooh"),
          ("EG-22-05", "مطوبس", "Metobas"),
          ("EG-22-06", "برج البرلس", "Burg Al Burullus"),
          ("EG-22-07", "بلطيم", "Baltim"),
          ("EG-22-08", "مصيف بلطيم", "Masief Baltim"),
          ("EG-22-09", "الحامول", "Hamol"),
          ("EG-22-10", "بيلا", "Bella"),
          ("EG-22-11", "الرياض", "Riyadh"),
          ("EG-22-12", "سيدي سالم", "Sidi Salm"),
          ("EG-22-13", "قلين", "Qellen"),
          ("EG-22-14", "سيدي غازي", "Sidi Ghazi"),
        }
      ),
      (
        "EG-23", "مطروح", "Matrouh",
        new List<(string, string, string)>
        {
          ("EG-23-01", "مرسى مطروح", "Marsa Matrouh"),
          ("EG-23-02", "الحمام", "El Hamam"),
          ("EG-23-03", "العلمين", "Alamein"),
          ("EG-23-04", "الضبعة", "Dabaa"),
          ("EG-23-05", "النجيلة", "Al-Nagila"),
          ("EG-23-06", "سيدي براني", "Sidi Brani"),
          ("EG-23-07", "السلوم", "Salloum"),
          ("EG-23-08", "سيوة", "Siwa"),
          ("EG-23-09", "مارينا", "Marina"),
          ("EG-23-10", "الساحل الشمالى", "North Coast"),
        }
      ),
      (
        "EG-24", "الأقصر", "Luxor",
        new List<(string, string, string)>
        {
          ("EG-24-01", "الأقصر", "Luxor"),
          ("EG-24-02", "الأقصر الجديدة", "New Luxor"),
          ("EG-24-03", "إسنا", "Esna"),
          ("EG-24-04", "طيبة الجديدة", "New Tiba"),
          ("EG-24-05", "الزينية", "Al ziynia"),
          ("EG-24-06", "البياضية", "Al Bayadieh"),
          ("EG-24-07", "القرنة", "Al Qarna"),
          ("EG-24-08", "أرمنت", "Armant"),
          ("EG-24-09", "الطود", "Al Tud"),
        }
      ),
      (
        "EG-25", "قنا", "Qena",
        new List<(string, string, string)>
        {
          ("EG-25-01", "قنا", "Qena"),
          ("EG-25-02", "قنا الجديدة", "New Qena"),
          ("EG-25-03", "ابو طشت", "Abu Tesht"),
          ("EG-25-04", "نجع حمادي", "Nag Hammadi"),
          ("EG-25-05", "دشنا", "Deshna"),
          ("EG-25-06", "الوقف", "Alwaqf"),
          ("EG-25-07", "قفط", "Qaft"),
          ("EG-25-08", "نقادة", "Naqada"),
          ("EG-25-09", "فرشوط", "Farshout"),
          ("EG-25-10", "قوص", "Quos"),
        }
      ),
      (
        "EG-26", "شمال سيناء", "North Sinai",
        new List<(string, string, string)>
        {
          ("EG-26-01", "العريش", "Arish"),
          ("EG-26-02", "الشيخ زويد", "Sheikh Zowaid"),
          ("EG-26-03", "نخل", "Nakhl"),
          ("EG-26-04", "رفح", "Rafah"),
          ("EG-26-05", "بئر العبد", "Bir al-Abed"),
          ("EG-26-06", "الحسنة", "Al Hasana"),
        }
      ),
      (
        "EG-27", "سوهاج", "Sohag",
        new List<(string, string, string)>
        {
          ("EG-27-01", "سوهاج", "Sohag"),
          ("EG-27-02", "سوهاج الجديدة", "Sohag El Gedida"),
          ("EG-27-03", "أخميم", "Akhmeem"),
          ("EG-27-04", "أخميم الجديدة", "Akhmim El Gedida"),
          ("EG-27-05", "البلينا", "Albalina"),
          ("EG-27-06", "المراغة", "El Maragha"),
          ("EG-27-07", "المنشأة", "almunsha'a"),
          ("EG-27-08", "دار السلام", "Dar AISalaam"),
          ("EG-27-09", "جرجا", "Gerga"),
          ("EG-27-10", "جهينة الغربية", "Jahina Al Gharbia"),
          ("EG-27-11", "ساقلته", "Saqilatuh"),
          ("EG-27-12", "طما", "Tama"),
          ("EG-27-13", "طهطا", "Tahta"),
          ("EG-27-14", "الكوثر", "Alkawthar"),
        }
      ),
    };
    }
}

# MetroSharp

### بک‌اند، API، اتوماسیون و زیرساخت Metro Moviez

MetroSharp بک‌اند و زیرساخت اصلی **Metro Moviez** هست.

این پروژه یه محیط متمرکز برای مدیریت یه کتابخونه‌ی بزرگ فیلم و سریال فراهم می‌کنه؛ از پیدا کردن محتوا و دانلود گرفته تا پردازش فایل‌ها، نگهداری اطلاعات فیلم‌ها، مدیریت دانلودها و ارائه‌ی محتوا به کلاینت‌های مختلف.

پروژه با **ASP.NET Core 10** ساخته شده و ساختارش بر پایه‌ی **Clean Architecture** و **CQRS** هست.

در حال حاضر کتابخونه بیشتر از **+20,000 فیلم** داره و ساختار پروژه طوری طراحی شده که در آینده بتونه با حجم خیلی بیشتری از محتوا هم کار کنه.

---

# فهرست مطالب

* [MetroSharp چیه؟](#metrosharp-چیه)
* [MetroSharp چه کارهایی انجام میده؟](#metrosharp-چه-کارهایی-انجام-میده)
* [معماری کلی سیستم](#معماری-کلی-سیستم)
* [سیستم چطوری کار می‌کنه؟](#سیستم-چطوری-کار-میکنه)
* [چرخه‌ی عمر محتوا](#چرخهی-عمر-محتوا)
* [بخش مدیریت](#بخش-مدیریت)
* [کلاینت‌ها](#کلاینتها)
* [ربات تلگرام](#ربات-تلگرام)
* [اپلیکیشن موبایل](#اپلیکیشن-موبایل)
* [REST API](#rest-api)
* [سیستم دانلود و تورنت](#سیستم-دانلود-و-تورنت)
* [اطلاعات فیلم و سریال](#اطلاعات-فیلم-و-سریال)
* [دیتابیس](#دیتابیس)
* [Background Serviceها](#background-serviceها)
* [ساختار پروژه](#ساختار-پروژه)
* [معماری پروژه](#معماری-پروژه)
* [CQRS](#cqrs)
* [Clean Architecture](#clean-architecture)
* [Infrastructure](#infrastructure)
* [Persistence](#persistence)
* [Domain](#domain)
* [Application](#application)
* [API](#api)
* [Resource](#resource)
* [احراز هویت و دسترسی‌ها](#احراز-هویت-و-دسترسیها)
* [لاگ و مانیتورینگ](#لاگ-و-مانیتورینگ)
* [Reverse Proxy و Deployment](#reverse-proxy-و-deployment)
* [سرویس‌های خارجی](#سرویسهای-خارجی)
* [جریان معمول درخواست‌ها](#جریان-معمول-درخواستها)
* [جریان معمول دانلود](#جریان-معمول-دانلود)
* [هدف‌های طراحی](#هدفهای-طراحی)
* [چرا پروژه این شکلی طراحی شده؟](#چرا-پروژه-این-شکلی-طراحی-شده)
* [توسعه‌های آینده](#توسعههای-آینده)

---

# MetroSharp چیه؟

MetroSharp در واقع **هسته‌ی بک‌اند Metro Moviez** هست.

البته فقط یه Web API ساده نیست.

MetroSharp بین چند سیستم مختلف قرار می‌گیره و همه‌ی اون‌ها رو به هم وصل و هماهنگ می‌کنه.

این سیستم‌ها شامل مواردی مثل:

* ASP.NET Core Web API
* دیتابیس
* Prowlarr
* qBittorrent
* سرویس‌های پردازش فایل
* ربات تلگرام
* اپلیکیشن موبایل
* کلاینت‌های وب
* فایل‌استوریج
* Background Serviceها

هستن.

ایده‌ی اصلی اینه که هر کدوم از این بخش‌ها وظیفه‌ی خودشون رو داشته باشن و MetroSharp مسئول هماهنگ کردن اون‌ها باشه.

به شکل ساده:

```text
                    ┌─────────────────────┐
                    │    MetroSharp API   │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
        Mobile App       Telegram Bot       Web Client
              │                │                │
              └────────────────┼────────────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     MetroSharp      │
                    │    Business Logic   │
                    └──────────┬──────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
      Database             qBittorrent          Prowlarr
          │                    │                    │
          │                    ▼                    │
          │              Downloaded Media           │
          │                    │                    │
          └────────────────────┼────────────────────┘
                               ▼
                         Media Library
```

---

# MetroSharp چه کارهایی انجام میده؟

MetroSharp چند وظیفه‌ی اصلی داره.

## ۱. مدیریت فیلم و سریال

اطلاعات مربوط به فیلم‌ها و سریال‌ها داخل سیستم نگهداری و مدیریت میشه.

مثلاً:

* فیلم
* سریال
* فصل
* قسمت
* ژانر
* بازیگران
* کارگردان
* امتیاز
* سال انتشار
* اطلاعات دانلود
* اطلاعات فایل
* پوستر و تصاویر
* وضعیت در دسترس بودن محتوا

---

## ۲. پیدا کردن محتوا

MetroSharp می‌تونه از زیرساخت جستجوی تورنت برای پیدا کردن نسخه‌های مختلف فیلم و سریال استفاده کنه.

برای این کار از **Prowlarr** استفاده میشه.

در نتیجه MetroSharp لازم نیست خودش برای تک‌تک Indexerها پیاده‌سازی جدا داشته باشه.

Prowlarr این قسمت رو مدیریت می‌کنه.

---

## ۳. مدیریت دانلود

بعد از اینکه نسخه‌ی مناسب یک فیلم پیدا شد، MetroSharp می‌تونه دانلود اون رو به **qBittorrent** بسپره.

خود MetroSharp قرار نیست Torrent Client باشه.

در عوض:

```text
MetroSharp
    │
    │ درخواست دانلود
    ▼
qBittorrent
    │
    │ دانلود
    ▼
Storage
```

qBittorrent مسئول دانلود واقعی فایل هست و MetroSharp وضعیت و روند کار رو مدیریت می‌کنه.

---

## ۴. پردازش فایل‌های دانلودشده

وقتی دانلود تموم شد، Background Serviceهای MetroSharp می‌تونن فایل جدید رو شناسایی و پردازش کنن.

این پردازش می‌تونه شامل مواردی مثل:

* پیدا کردن فایل دانلودشده
* تشخیص اینکه فایل مربوط به کدوم فیلم یا سریاله
* آپدیت کردن وضعیت دانلود
* استخراج اطلاعات فایل
* انتقال یا مرتب‌سازی فایل
* آماده کردن محتوا برای استفاده‌ی کاربر

باشه.

---

## ۵. ارائه‌ی REST API

MetroSharp یه REST API در اختیار کلاینت‌ها قرار میده.

کلاینت‌ها می‌تونن از طریق API به مواردی مثل این‌ها دسترسی داشته باشن:

* فیلم‌ها
* سریال‌ها
* جستجو
* ژانرها
* اطلاعات فیلم
* وضعیت دانلود
* کاربران
* مدیریت سیستم

API در واقع مرز اصلی بین MetroSharp و کلاینت‌هاست.

---

## ۶. ربات تلگرام

MetroSharp یه Telegram Bot هم داره که به عنوان یکی از کلاینت‌های سیستم عمل می‌کنه.

کاربر می‌تونه از طریق تلگرام فیلم‌ها رو جستجو کنه و با کتابخونه‌ی Metro Moviez کار کنه.

---

## ۷. اپلیکیشن موبایل

یه اپلیکیشن موبایل هم برای مدیریت و کار با سرور وجود داره/در حال توسعه است.

این اپلیکیشن مستقیماً با دیتابیس یا qBittorrent کار نمی‌کنه.

همه‌چیز از طریق API انجام میشه.

لینک سورس اپلیکیشن موبایل بعداً به این README اضافه میشه.

---

# معماری کلی سیستم

ساختار MetroSharp بر اساس **Clean Architecture** و **CQRS** طراحی شده.

به صورت ساده:

```text
┌────────────────────────────────────────────────────┐
│                     Clients                        │
│                                                    │
│  Mobile App │ Telegram Bot │ Web Client │ Other    │
└───────────────────────┬────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────┐
│                       API                          │
│              HTTP / Controllers / Endpoints        │
└───────────────────────┬────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────┐
│                   Application                      │
│                                                    │
│ Commands │ Queries │ Handlers │ DTOs │ Behaviors   │
└───────────────┬───────────────────────┬────────────┘
                │                       │
                ▼                       ▼
┌────────────────────────┐    ┌──────────────────────┐
│         Domain         │    │    Infrastructure    │
│                        │    │                      │
│ Entities               │    │ Telegram             │
│ Value Objects          │    │ qBittorrent          │
│ Business Rules         │    │ Prowlarr             │
│ Interfaces             │    │ Media Services       │
└────────────┬───────────┘    └──────────────────────┘
             │
             ▼
┌────────────────────────────────────────────────────┐
│                    Persistence                     │
│                                                    │
│ EF Core │ DbContexts │ Configurations │ Migrations │
└────────────────────────────────────────────────────┘
```

یکی از مهم‌ترین اصول پروژه اینه که منطق اصلی برنامه نباید به تکنولوژی خاصی وابسته باشه.

مثلاً Business Logic نباید بدونه که دانلودها با qBittorrent انجام میشن.

---

# سیستم چطوری کار می‌کنه؟

MetroSharp در اصل نقش **هماهنگ‌کننده‌ی اصلی سیستم** رو داره.

به جای اینکه هر کلاینت مستقیماً با همه‌ی سرویس‌ها ارتباط داشته باشه، همه چیز از طریق MetroSharp انجام میشه.

مثلاً معماری بد می‌تونست این شکلی باشه:

```text
Mobile ──────► Database
Mobile ──────► qBittorrent
Mobile ──────► Prowlarr
Mobile ──────► Storage

Telegram ────► Database
Telegram ────► qBittorrent
```

این مدل خیلی سریع تبدیل به یه سیستم شلوغ و سخت برای نگهداری میشه.

مدل MetroSharp اینطوریه:

```text
                    MetroSharp
                 ┌──────────────┐
                 │              │
Mobile ─────────►│              │
Telegram ───────►│     API      │
Web ────────────►│              │
                 │              │
                 └──────┬───────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
       Database     qBittorrent    Prowlarr
```

در نتیجه Business Logic در یک جای مشخص قرار داره.

---

# چرخه‌ی عمر محتوا

یکی از قسمت‌های مهم MetroSharp چرخه‌ی عمر یک فیلم یا سریاله.

مثلاً برای یک فیلم:

```text
Movie discovered
       │
       ▼
Metadata created
       │
       ▼
Search for available release
       │
       ▼
Torrent discovered
       │
       ▼
Download requested
       │
       ▼
qBittorrent downloads content
       │
       ▼
Download completed
       │
       ▼
MetroSharp detects/processes file
       │
       ▼
Media record updated
       │
       ▼
Movie becomes available
       │
       ├──────────────► REST API
       │
       ├──────────────► Mobile App
       │
       └──────────────► Telegram Bot
```

نکته‌ی مهم اینه که **وجود یک فیلم در دیتابیس الزاماً به معنی آماده بودن فایل اون نیست.**

ممکنه اطلاعات یک فیلم داخل دیتابیس وجود داشته باشه، ولی فایل اون هنوز دانلود نشده باشه.

---

# بخش مدیریت

MetroSharp بخش‌های مدیریتی هم داره که مدیر سیستم می‌تونه از طریق اون‌ها سرور و محتوای موجود رو کنترل کنه.

کارهایی مثل:

* مدیریت فیلم‌ها
* مدیریت سریال‌ها
* مدیریت کاربران
* مدیریت دانلودها
* شروع دانلود
* لغو دانلود
* بررسی وضعیت دانلود
* مدیریت Metadata
* مدیریت فایل‌ها
* بررسی وضعیت سیستم
* مدیریت قابلیت‌های مرتبط با Telegram

از این طریق قابل انجام هستن.

نکته‌ی مهم اینه که پنل یا اپلیکیشن مدیریت مستقیماً با دیتابیس کار نمی‌کنه.

مسیر کلی این شکلیه:

```text
Manager
   │
   ▼
Management Client
   │
   ▼
REST API
   │
   ▼
Application Layer
   │
   ▼
Persistence / Infrastructure
```

---

# کلاینت‌ها

Metro Moviez قرار نیست فقط به یک کلاینت محدود باشه.

در حال حاضر سیستم می‌تونه توسط چند کلاینت مختلف استفاده بشه:

### Mobile App

اپلیکیشن موبایل برای کار با سیستم.

### Telegram Bot

ربات تلگرام برای جستجو و دسترسی به محتوا.

### Web Application

یک Frontend وب هم می‌تونه از همین API استفاده کنه.

نکته‌ی مهم اینه که Business Logic داخل کلاینت‌ها پیاده‌سازی نمیشه.

کلاینت فقط با API صحبت می‌کنه.

---

# ربات تلگرام

ربات تلگرام یکی از کلاینت‌های MetroSharp محسوب میشه.

ربات قرار نیست منطق اصلی سیستم رو داخل خودش پیاده‌سازی کنه.

در عوض:

```text
Telegram User
      │
      ▼
Telegram
      │
      ▼
Metro Moviez Bot
      │
      ▼
MetroSharp
      │
      ├── Database
      ├── Media Library
      └── Download System
```

ربات می‌تونه امکاناتی مثل این‌ها داشته باشه:

* جستجوی فیلم
* جستجوی سریال
* نمایش اطلاعات فیلم
* نمایش وضعیت محتوا
* دسترسی به محتوا
* Navigation بین فیلم‌ها
* امکانات مربوط به کاربران

این جداسازی باعث میشه بتونیم Telegram Bot رو تغییر بدیم بدون اینکه Core سیستم خراب بشه.

---

# اپلیکیشن موبایل

اپلیکیشن موبایل یک Client جدا از MetroSharp هست.

این اپلیکیشن منطق اصلی سرور رو داخل خودش نداره.

در عوض با REST API ارتباط برقرار می‌کنه.

```text
┌─────────────────┐
│   Mobile App    │
└────────┬────────┘
         │ HTTPS
         ▼
┌─────────────────┐
│   MetroSharp    │
│      API        │
└────────┬────────┘
         │
         ▼
    Application
         │
         ▼
       Domain
```

این ساختار باعث میشه بعداً بتونیم کلاینت موبایل رو عوض یا حتی چند کلاینت مختلف ایجاد کنیم، بدون اینکه Core برنامه تغییر اساسی کنه.

---

# REST API

API مرز ارتباطی MetroSharp با دنیای بیرونه.

کارهای اصلی این لایه شامل:

* دریافت HTTP Request
* Authentication
* Authorization
* Validation
* تبدیل Request به Command یا Query
* تبدیل نتیجه به HTTP Response

هست.

Controllerها نباید پر از Business Logic باشن.

به طور معمول جریان درخواست این شکلیه:

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Command / Query
     │
     ▼
Handler
     │
     ▼
Domain / Infrastructure
     │
     ▼
Result
     │
     ▼
HTTP Response
```

---

# سیستم دانلود و تورنت

MetroSharp خودش Torrent Client نیست.

دانلود واقعی توسط **qBittorrent** انجام میشه.

این جداسازی چند تا مزیت داره:

* منطق Torrent از برنامه جدا می‌مونه
* مدیریت دانلودها به qBittorrent سپرده میشه
* MetroSharp روی Business Logic تمرکز می‌کنه
* وضعیت دانلودها قابل پیگیریه
* در آینده میشه Torrent Client رو عوض کرد

جریان کلی:

```text
MetroSharp
     │
     │ Search
     ▼
 Prowlarr
     │
     │ Results
     ▼
MetroSharp
     │
     │ Select Release
     ▼
qBittorrent
     │
     │ Download
     ▼
Storage
```

بعد MetroSharp می‌تونه وضعیت دانلود رو بررسی کنه و دیتابیس رو آپدیت کنه.

---

# Prowlarr

Prowlarr وظیفه‌ی مدیریت و جستجو بین Indexerها رو بر عهده داره.

به جای اینکه MetroSharp برای هر Indexer یک Integration جدا داشته باشه، Prowlarr این بخش رو مدیریت می‌کنه.

مثلاً:

```text
MetroSharp
     │
     ▼
 Prowlarr
     │
 ├── Indexer A
 ├── Indexer B
 ├── Indexer C
 └── ...
```

این باعث میشه MetroSharp نسبت به Indexerهای مختلف وابستگی مستقیم نداشته باشه.

---

# اطلاعات فیلم و سریال

هر فیلم یا سریال اطلاعات مختلفی داره.

برای مثال:

* عنوان
* عنوان اصلی
* سال انتشار
* توضیحات
* Poster
* Backdrop
* ژانر
* بازیگران
* کارگردان
* امتیاز
* Runtime
* External ID
* اطلاعات Release

بخشی از این اطلاعات می‌تونه از سرویس‌های Metadata خارجی دریافت بشه.

Metadata و سرویس مربوط به اون از Core برنامه جدا نگه داشته شده تا اگر در آینده Provider عوض شد، لازم نباشه کل سیستم تغییر کنه.

---

# دیتابیس

MetroSharp برای دسترسی به دیتابیس از **Entity Framework Core** استفاده می‌کنه.

لایه‌ی Persistence مسئول مواردی مثل:

* DbContext
* Entity Configuration
* Relationshipها
* Migrationها
* Queryها
* Repositoryها
* Database implementation

هست.

در پروژه چند Context جدا برای بخش‌های مختلف سیستم وجود داره.

برای مثال:

```text
Application Data
Download Data
Telegram Data
```

این جداسازی کمک می‌کنه قسمت‌های مختلف سیستم بیش از حد به هم وابسته نشن.

---

# Background Serviceها

MetroSharp فقط یک برنامه‌ی Request/Response ساده نیست.

بعضی کارها باید بدون اینکه کاربر مستقیماً درخواست بده، در پس‌زمینه انجام بشن.

برای همین Background Serviceها بخش مهمی از سیستم هستن.

مثلاً:

* بررسی وضعیت دانلودها
* پردازش دانلودهای کامل‌شده
* Sync کردن وضعیت Media
* بروزرسانی Metadata
* پردازش فایل‌ها
* کارهای زمان‌بندی‌شده
* عملیات مربوط به Telegram
* Maintenance

مثلاً یک دانلود ممکنه این مراحل رو طی کنه:

```text
Search started
      ↓
Torrent found
      ↓
Download requested
      ↓
Download started
      ↓
Download progressing
      ↓
Download completed
      ↓
File detected
      ↓
Media processed
      ↓
Movie available
```

تمام این مراحل لزوماً در پاسخ به یک HTTP Request انجام نمیشن.

---

# ساختار پروژه

ساختار کلی پروژه به این شکله:

```text
/
├── API/
├── Application/
├── Domain/
├── Infrastructure/
├── Persistence/
└── Resource/
```

هر Project مسئولیت خودش رو داره.

---

# API

پروژه‌ی `API` نقطه‌ی ورود اصلی برنامه است.

چیزهایی مثل:

* Controllers
* Middleware
* Configuration
* Dependency Injection
* Authentication
* Swagger
* Application Startup
* HTTP-specific functionality

اینجا قرار می‌گیرن.

API باید تا حد امکان Thin باشه و Business Logic داخل Controllerها قرار نگیره.

مثلاً:

```text
API
 │
 └── Controller
       │
       ▼
   Application
```

---

# Application

پروژه‌ی `Application` جاییه که Use Caseهای اصلی برنامه تعریف میشن.

این قسمت عمدتاً محل پیاده‌سازی CQRS هست.

ساختار کلی می‌تونه چیزی شبیه این باشه:

```text
Application/
├── Commands/
├── Queries/
├── Handlers/
├── DTOs/
├── Behaviors/
├── Interfaces/
└── Services/
```

Application جواب این سؤال‌ها رو میده:

> وقتی مدیر یک فیلم ایجاد می‌کنه چه اتفاقی باید بیفته؟

یا:

> وقتی کاربر دنبال یک فیلم می‌گرده چه اتفاقی باید بیفته؟

این لایه Business Workflow رو مدیریت می‌کنه، بدون اینکه مستقیماً به یک تکنولوژی خاص وابسته باشه.

---

# Domain

`Domain` هسته‌ی اصلی برنامه است.

این بخش باید مستقل‌ترین قسمت سیستم باشه.

مثلاً شامل:

```text
Domain/
├── Entities/
├── ValueObjects/
├── Enums/
├── Interfaces/
└── Domain Rules
```

مفاهیم اصلی سیستم مثل:

* Movie
* Series
* Season
* Episode
* User
* Download
* Media File
* Genre

اینجا تعریف میشن.

Domain نباید مستقیماً به چیزهایی مثل این‌ها وابسته باشه:

* ASP.NET Core
* Entity Framework Core
* qBittorrent
* Telegram
* Prowlarr
* HTTP
* Database خاص

هدف اینه که Core برنامه مستقل از تکنولوژی باقی بمونه.

---

# Infrastructure

`Infrastructure` جاییه که ارتباط با سرویس‌های خارجی پیاده‌سازی میشه.

مثلاً:

```text
Infrastructure/
├── Telegram/
├── QBitTorrent/
├── Prowlarr/
├── Metadata/
├── Media/
└── Services/
```

فرض کنیم Application یک Interface به اسم زیر داشته باشه:

```csharp
ITorrentService
```

Infrastructure میاد implementation واقعی اون رو فراهم می‌کنه:

```text
ITorrentService
       ▲
       │
       │ implements
       │
QBitTorrentService
```

در نتیجه Application لازم نیست بدونه qBittorrent دقیقاً چطوری کار می‌کنه.

فقط می‌دونه یک `ITorrentService` داره که می‌تونه ازش استفاده کنه.

---

# Persistence

`Persistence` مسئول قسمت‌های مربوط به دیتابیسه.

مواردی مثل:

* Entity Framework Core
* DbContext
* Entity Configuration
* Relationships
* Migrations
* Database Implementation

اینجا قرار می‌گیرن.

Domain لازم نیست بدونه EF Core اصلاً وجود داره.

---

# Resource

پروژه‌ی `Resource` برای Resourceهای مشترک استفاده میشه.

بسته به نیاز پروژه می‌تونه شامل مواردی مثل:

* Localization
* Resource Files
* Shared Constants
* Embedded Resources

باشه.

این کار باعث میشه Resourceها با Business Logic قاطی نشن.

---

# معماری پروژه

یکی از اصول اصلی Clean Architecture اینه که Dependencyها باید به سمت Core برنامه حرکت کنن.

به صورت ساده:

```text
             ┌──────────────┐
             │     API      │
             └──────┬───────┘
                    │
                    ▼
             ┌──────────────┐
             │ Application  │
             └──────┬───────┘
                    │
             ┌──────┴──────┐
             ▼             ▼
        ┌──────────┐  ┌──────────────┐
        │  Domain  │  │Infrastructure│
        └──────────┘  └──────────────┘
              ▲              │
              │              │
              └──────┬───────┘
                     ▼
                Persistence
```

هدف اصلی اینه که Business Logic به External Service خاصی قفل نشه.

---

# CQRS

MetroSharp از **CQRS — Command Query Responsibility Segregation** استفاده می‌کنه.

ایده‌ی اصلی خیلی ساده است:

> عملیاتی که اطلاعات رو تغییر میدن از عملیاتی که فقط اطلاعات رو می‌خونن جدا باشن.

---

## Command

Command یعنی:

> «یه کاری انجام بده.»

مثلاً:

```text
CreateMovie
UpdateMovie
DeleteMovie
StartDownload
CancelDownload
UpdateMovieMetadata
```

جریان کلی:

```text
Command
   │
   ▼
Command Handler
   │
   ▼
Domain / Infrastructure
   │
   ▼
State Changed
```

---

## Query

Query یعنی:

> «اطلاعات رو بهم بده.»

مثلاً:

```text
GetMovie
GetMovies
SearchMovies
GetDownloadStatus
GetMovieDetails
GetGenres
```

جریان:

```text
Query
  │
  ▼
Query Handler
  │
  ▼
Database / Read Model
  │
  ▼
Result
```

---

# چرا CQRS؟

یک سیستم مثل Metro Moviez تعداد زیادی عملیات Read داره:

```text
Search movies
List movies
Get movie details
Get genres
Get available content
```

ولی عملیات تغییر اطلاعات متفاوت هستن:

```text
Create movie
Start download
Update metadata
Delete media
```

با CQRS هر Use Case واضح‌تر و مستقل‌تر میشه.

به جای اینکه یک `MovieService` بزرگ داشته باشیم که بعداً صدها متد داخلش قرار بگیره، عملیات‌ها می‌تونن به Command و Queryهای مشخص تقسیم بشن.

---

# Abstraction در Infrastructure

یکی از نکات مهم معماری MetroSharp اینه که سرویس‌های خارجی مستقیماً وارد Business Logic نشن.

مثلاً:

```text
Application
     │
     ▼
ITorrentService
     ▲
     │
     └── QBitTorrentService
```

همین الگو می‌تونه برای سرویس‌های دیگه هم استفاده بشه:

```text
IMetadataService
ITelegramService
IFileService
ISearchService
```

مزیتش:

* تست راحت‌تر
* وابستگی کمتر
* امکان عوض کردن سرویس
* کد تمیزتر
* جداسازی بهتر مسئولیت‌ها

---

# احراز هویت و دسترسی‌ها

همه‌ی کاربران قرار نیست دسترسی یکسانی داشته باشن.

برای مثال:

```text
                 User
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
 Public Operations      User Operations


                Manager
                   │
                   ▼
          Management Operations
                   │
        ┌──────────┼──────────┐
        ▼          ▼          ▼
     Movies    Downloads    Users
```

یعنی یک کاربر عادی نباید بتونه عملیات مدیریتی انجام بده.

Authorization در سمت API/Application کنترل میشه تا تمام کلاینت‌ها از یک سیستم دسترسی مشترک استفاده کنن.

---

# لاگ و مانیتورینگ

به خاطر اینکه MetroSharp Background Service و عملیات طولانی داره، Logging اهمیت زیادی پیدا می‌کنه.

مثلاً اگر یک دانلود مشکل پیدا کنه، باید بشه فهمید:

* چه عملیاتی در حال انجام بوده
* مربوط به کدوم فیلم بوده
* با کدوم سرویس ارتباط داشته
* کجا خطا اتفاق افتاده
* دلیل خطا چی بوده

مثلاً:

```text
Search started
      ↓
Torrent found
      ↓
Download requested
      ↓
Download started
      ↓
Download completed
      ↓
File processing started
      ↓
Movie updated
```

اگر یکی از این مراحل شکست بخوره، Logها باید کمک کنن بفهمیم دقیقاً مشکل کجا بوده.

---

# Reverse Proxy و Deployment

MetroSharp برای اجرا روی سرور می‌تونه پشت Reverse Proxy قرار بگیره.

معماری کلی Deployment:

```text
                    Internet
                       │
                       ▼
                  Cloudflare
                       │
                       ▼
                     Caddy
                       │
              ┌────────┴────────┐
              │                 │
              ▼                 ▼
        MetroSharp API      Public Media
              │
              ▼
        Internal Services
              │
      ┌───────┼────────┐
      ▼       ▼        ▼
   Database qBit     Prowlarr
```

Reverse Proxy کارهایی مثل این‌ها رو انجام میده:

* HTTPS
* Domain Routing
* Reverse Proxy
* مدیریت Certificate
* جدا کردن سرویس‌های داخلی از اینترنت
* Routing

اینطوری لازم نیست همه‌ی سرویس‌های داخلی مستقیماً روی اینترنت در دسترس باشن.

---

# سرویس‌های خارجی

MetroSharp از چند سرویس و تکنولوژی خارجی استفاده می‌کنه.

## ASP.NET Core

Framework اصلی Backend.

استفاده شده برای:

* REST API
* Dependency Injection
* Middleware
* Authentication
* Background Service
* HTTP Infrastructure

---

## Entity Framework Core

برای ارتباط با دیتابیس و Persistence استفاده میشه.

---

## qBittorrent

مسئول دانلود Torrentهاست.

MetroSharp خودش Torrent Client نیست و از qBittorrent برای این کار استفاده می‌کنه.

---

## Prowlarr

مسئول مدیریت و جستجوی Indexerهاست.

MetroSharp از Prowlarr برای پیدا کردن Releaseهای مناسب استفاده می‌کنه.

---

## Telegram

برای ارتباط با کاربران از طریق Telegram Bot استفاده میشه.

---

## Cloudflare / Caddy

برای بخش Networking و Reverse Proxy زیرساخت استفاده میشن.

---

# جریان معمول یک درخواست API

فرض کنیم کاربر می‌خواد اطلاعات یک فیلم رو بگیره.

درخواست تقریباً این مسیر رو طی می‌کنه:

```text
User
 │
 │ GET /api/movies/{id}
 ▼
API Controller
 │
 │ Query
 ▼
GetMovieQuery
 │
 ▼
GetMovieQueryHandler
 │
 ├──► Persistence
 │
 └──► Domain
 │
 ▼
Movie DTO
 │
 ▼
API Response
 │
 ▼
User
```

Controller فقط درخواست رو دریافت می‌کنه و کار اصلی توسط Application انجام میشه.

---

# جریان معمول دانلود

دانلود یکی از مهم‌ترین Workflowهای سیستم هست.

جریان کلی:

```text
Manager/User
     │
     ▼
API
     │
     ▼
StartDownloadCommand
     │
     ▼
Command Handler
     │
     ▼
Search / Select Release
     │
     ▼
Prowlarr
     │
     ▼
Torrent Result
     │
     ▼
qBittorrent
     │
     ▼
Torrent Download
     │
     ▼
Storage
     │
     ▼
Background Worker
     │
     ▼
Media Processing
     │
     ▼
Database Updated
     │
     ▼
Movie Available
```

این یکی از اصلی‌ترین Workflowهای MetroSharp محسوب میشه.

---

# جداسازی Metadata و Media

در MetroSharp اطلاعات یک فیلم و فایل واقعی اون دو مفهوم جدا هستن.

مثلاً:

```text
Movie
 ├── Metadata
 │    ├── Title
 │    ├── Description
 │    ├── Poster
 │    ├── Cast
 │    └── Genres
 │
 └── Media
      ├── File
      ├── Size
      ├── Quality
      └── Availability
```

این یعنی می‌تونیم اطلاعات یک فیلم رو داشته باشیم، حتی اگر فایل فیلم هنوز دانلود نشده باشه.

همچنین می‌تونیم Metadata رو آپدیت کنیم بدون اینکه فایل اصلی دستکاری بشه.

---

# قابلیت توسعه

MetroSharp طوری ساخته شده که رشد پروژه باعث نشه مجبور بشیم کل سیستم رو از اول بنویسیم.

مثلاً اگر تعداد فیلم‌ها از:

```text
20,000
```

به:

```text
100,000+
```

برسه، ساختار کلی پروژه همچنان قابل استفاده خواهد بود.

در آینده میشه مواردی مثل این‌ها رو اضافه کرد:

* Workerهای جداگانه
* Job Queue
* Caching
* Search Engine
* Object Storage
* CDN
* Media Processing Server
* API Scaling

بدون اینکه لازم باشه Clientها دوباره از صفر نوشته بشن.

---

# هدف‌های طراحی

MetroSharp بر اساس چند اصل اصلی ساخته شده.

## ۱. جداسازی مسئولیت‌ها

هر بخش باید کار خودش رو انجام بده.

```text
API             → HTTP
Application     → Use Cases
Domain          → Business Rules
Persistence     → Database
Infrastructure  → External Services
```

---

## ۲. قابل تعویض بودن سرویس‌ها

تکنولوژی‌های خارجی نباید وارد Core برنامه بشن.

مثلاً:

```text
Application
     │
     ▼
ITorrentService
     ▲
     │
QBitTorrent implementation
```

اگر یک روز تصمیم بگیریم qBittorrent رو عوض کنیم، نباید مجبور بشیم Business Logic رو از نو بنویسیم.

---

## ۳. پشتیبانی از چند Client

Backend نباید به یک Client خاص وابسته باشه.

یک API می‌تونه همزمان توسط این‌ها استفاده بشه:

* Mobile
* Web
* Telegram
* Desktop
* Third-party clients

---

## ۴. اتوماسیون

مدیریت تعداد زیادی فیلم و دانلود با انجام دستی همه‌ی کارها عملاً غیرممکنه.

برای همین Background Serviceها و Automation بخش مهمی از MetroSharp هستن.

---

## ۵. قابل نگهداری بودن

پروژه به چند بخش تقسیم شده تا با اضافه شدن قابلیت‌های جدید، کل کدبیس تبدیل به یک توده‌ی بزرگ و به‌هم‌ریخته نشه.

---

# چرا پروژه این شکلی طراحی شده؟

پروژه‌ای مثل MetroSharp خیلی راحت می‌تونه پیچیده بشه.

چون همزمان با چیزهای مختلفی سر و کار داره:

* HTTP
* Database
* Authentication
* Media
* Torrent
* External API
* Telegram
* Background Processing
* File System
* Metadata
* چند Client مختلف

اگر همه‌ی این‌ها رو داخل یک ASP.NET Core Project بریزیم، پروژه خیلی زود سخت و دردسرساز میشه.

برای همین مسئولیت‌ها جدا شدن:

```text
                     MetroSharp
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
      API            Application         Workers
        │                 │                 │
        │                 ▼                 │
        │              Domain               │
        │                 │                 │
        └──────────┬──────┴──────┬──────────┘
                   │             │
                   ▼             ▼
              Persistence   Infrastructure
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
               qBittorrent    Prowlarr    Telegram
```

هر قسمت می‌تونه تا حد زیادی مستقل از قسمت‌های دیگه توسعه پیدا کنه.

---

# فلسفه‌ی پروژه

هدف MetroSharp این نیست که یک Enterprise Platform فوق‌العاده عظیم و پیچیده ساخته بشه.

هدف اینه که یک سیستم واقعی و قابل استفاده داشته باشیم که در عین حال:

* ساختار مناسبی داشته باشه
* قابل فهم باشه
* قابل توسعه باشه
* قابل تست باشه
* قابل نگهداری باشه
* برای یک Media Platform نسبتاً بزرگ جواب بده

یعنی نه اونقدر ساده که بعداً پروژه به هم بریزه، نه اونقدر پیچیده که برای اضافه کردن یک قابلیت ساده مجبور بشیم ده‌ها abstraction اضافه کنیم.

---

# Technology Stack

| بخش                 | تکنولوژی                     |
| ------------------- | ---------------------------- |
| Backend             | ASP.NET Core 9               |
| زبان                | C#                           |
| Architecture        | Clean Architecture           |
| Application Pattern | CQRS                         |
| Database Access     | Entity Framework Core        |
| Database            | SQLite / Relational Database |
| Logging             | Serilog                      |
| Torrent Client      | qBittorrent                  |
| Indexer Management  | Prowlarr                     |
| Messaging / Client  | Telegram Bot                 |
| Reverse Proxy       | Caddy                        |
| CDN / DNS / Edge    | Cloudflare                   |
| Mobile Client       | React Native / Expo          |
| API Style           | REST                         |

---

# ساختار Repository

یک نمای ساده از ساختار پروژه:

```text
MetroSharp/
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Configuration/
│   └── Program.cs
│
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   ├── Behaviors/
│   └── Interfaces/
│
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Telegram/
│   ├── QBitTorrent/
│   ├── Prowlarr/
│   ├── Metadata/
│   ├── Media/
│   └── Services/
│
├── Persistence/
│   ├── Contexts/
│   ├── Configurations/
│   ├── Migrations/
│   └── Repositories/
│
└── Resource/
    ├── Localization/
    └── Resources/
```

ممکنه ساختار واقعی Repository در طول توسعه تغییر کنه و پوشه‌ها یا Projectهای بیشتری بهش اضافه بشن.

---

# خلاصه

MetroSharp بک‌اند و زیرساخت اصلی **Metro Moviez** هست.

این پروژه یک REST API ساده نیست؛ بلکه مجموعه‌ای از API، دیتابیس، Background Workerها، سیستم مدیریت Media، Torrent Management، Indexer Integration و Telegram رو کنار هم قرار میده.

ایده‌ی اصلی پروژه اینه که هر بخش مسئولیت خودش رو داشته باشه:

```text
                    ┌───────────────┐
                    │    Clients    │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │      API      │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │  Application  │
                    │    + CQRS     │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │    Domain     │
                    └───────┬───────┘
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
       ┌──────────────┐           ┌──────────────┐
       │ Persistence  │           │Infrastructure│
       └──────┬───────┘           └──────┬───────┘
              │                          │
              ▼                    ┌─────┼─────┐
          Database                 ▼     ▼     ▼
                               qBit  Prowlarr Telegram
```

در نهایت MetroSharp نقش **هسته و لایه‌ی هماهنگ‌کننده‌ی Metro Moviez** رو داره.

کلاینت‌ها، دیتابیس، سیستم دانلود، Telegram و سرویس‌های مختلف لازم نیست مستقیماً با هم درگیر بشن؛ MetroSharp وسط این سیستم قرار می‌گیره و منطق و هماهنگی کل سیستم رو مدیریت می‌کنه.

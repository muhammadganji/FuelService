# سرویس میزان سوخت
این API مقدار سوخت را براساس نزدیک‌ترین تاریخ-زمان موجود در داده‌های ثبت‌شده اکسل بر میگرداند.
## Requirement
- install sdk .net 10
- download and run the app

## API
```
GET /api/fuel?timestamp={date-time}
```
---

## first examle
```
input: 2025-01-07T11:24:04Z
output: 23.86
voltage in excel: 4514
```


## second examle
```
input: 2025-01-07T04:55:39Z 
output: 69.98
voltage in excel: 130
```

<!-- Muhammad Ganji nezhad --/>
<!-- https://linkedin.com/in/muhammad-Ganji-Nezhad --/>
<!-- 2025-12-02 --/>


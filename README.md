# 🫀 PatientDashboard

Real-time patient vitals demo built with **.NET 8**, **ASP.NET Core (Razor Pages)**, **SignalR**, **EF Core (SQLite)**, and **Identity**.

<p align="center">
  <img alt="dotnet" src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white">
  <img alt="aspnet core" src="https://img.shields.io/badge/ASP.NET_Core-Razor_Pages-5C2D91?logo=dotnet&logoColor=white">
  <img alt="signalr" src="https://img.shields.io/badge/SignalR-Realtime-0E76A8">
  <img alt="ef core" src="https://img.shields.io/badge/EF_Core-SQLite-0F80C3?logo=sqlite&logoColor=white">
</p>

---

## ✨ What’s inside

- 👥 Patients list with actions (Monitor, Export CSV)
- 📈 Live charts (HR, SpO₂, BP) + last 10 values table
- 🕒 24h overview with trends, counters, and latest 20 readings
- ▶️ One-click **30s simulation** per patient (UI or API)
- 🔐 Auth: all pages require login (except `/Identity/*`)
- 🧰 Auto-migrations + seeded data (patients, vitals, admin user)

---

## 🖥️ Quick start

Default login that is seeded:
Email: admin@test.com
Password: Pass12345!@#$%

ready to run with dotnet run from the folder that has the solution file

<img width="493" height="33" alt="image" src="https://github.com/user-attachments/assets/7fb5a200-efe3-4c21-a32f-8f44dd39f8f0" />

then navigato to Url

<img width="1204" height="517" alt="image" src="https://github.com/user-attachments/assets/bd937823-137f-481f-82da-dc64973a22c0" />

<img width="1461" height="869" alt="image" src="https://github.com/user-attachments/assets/8007efda-3bb3-4e92-8c2c-614c46f583df" />

or run dockerized in the folder that contains the .yml file run docker compose up -d --build

<img width="484" height="32" alt="image" src="https://github.com/user-attachments/assets/854efa99-667e-4250-937f-d44954ec2a0a" />

then optionally open from docker desktop

<img width="1701" height="99" alt="image" src="https://github.com/user-attachments/assets/15627034-23e5-4171-b363-9590be5f93be" />

the app runs at 

<img width="1525" height="816" alt="image" src="https://github.com/user-attachments/assets/0e95f117-5830-48c7-86d1-cf0a95734b4f" />




Database & Seeding

Tables: Patients, VitalSigns, Identity tables

Seed data (DbSeeder.Seed)

Patients: John Doe (101), Jane Smith (102), Bob Johnson (103)

Vitals per patient: exactly 10 historical records spaced 30s apart:

6 × Normal, 3 × Warning, 1 × Critical

Status is calculated using the same thresholds as the live service
UI Pages

24h Overview (Pages/Index.cshtml)

Filter by patient

3 charts (HR, SpO₂, BP)

Status counters and latest 20 readings table

/Patients — Patients List (Pages/Patients/Index.cshtml)

Action: Monitor

Action: Export Vital Signs to CSV

/Patients/Monitor?id={id} — Monitor Page

Live charts + last 10 values table

Start 30s Simulation button


CSV Export Format

From Patients → Export Vital Signs to CSV:

Filename: {PatientName}_vitals_{UTC_TIMESTAMP}.csv

Columns: MeasuredAtUtc,HeartRate,Systolic,Diastolic,OxygenSaturation,Status

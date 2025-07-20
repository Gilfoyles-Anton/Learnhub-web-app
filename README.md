# Learnhub Online Learning Platform

## Project Overview
Learnhub is an e-learning website built with **ASP.NET Core MVC** using the MVC architectural pattern. It integrates **SQL Server** (LocalDB) and uses Visual Studio’s built-in **Database Service Explorer** for data management. The system supports two user roles—**Regular Users** and **Administrators**—and offers features such as course browsing, enrollment, management, announcement publishing, and data export.

---

## Tech Stack

| Category             | Technology                            | Version    |
| -------------------- | ------------------------------------- | ---------- |
| Programming Language | C#                                    | 8.0+       |
| Framework            | ASP.NET Core MVC                      | 5.0/6.0    |
| ORM                  | Entity Framework Core                 | 5.0/6.0    |
| Database             | SQL Server LocalDB / SQL Server 2019+ | —          |
| IDE                  | Visual Studio 2019/2022               | —          |
| Frontend             | HTML5, CSS3, Bootstrap 4, jQuery      | —          |
| Authentication       | ASP.NET Core Identity                 | —          |
| Version Control      | Git, GitHub                           | —          |

---

## Environment

| Environment        | Description                                  |
| ------------------ | -------------------------------------------- |
| Operating System   | Windows 10/11                                |
| Database           | LocalDB (VS built-in) or SQL Server instance |
| Supported Browsers | Chrome, Edge, Firefox, Safari                |

---

## New Features (Latest Week Deliverables)

The latest additions to the platform include the following features:

1. **Wallet Activation Code Management**: Search, edit, delete, sort, and reset wallet activation codes.
2. **User Management**: Search, add, edit, delete, sort, and reset user information.
3. **Admin Activation Code Management**: Search, add, edit, delete, sort, and reset admin activation codes.
4. **Course Video Interaction**: Users can play, like, collect, purchase, and increase course view counts via interactions.
5. **User Wallet & Course Info**: Wallet recharge via activation code; display liked, purchased, and collected courses in the profile.
6. **Wallet-Course Conversion System**: Recharge wallet and use wallet balance to purchase courses.
7. **Interactive Personal Page**: Clicking browsed/liked/purchased/favorited courses leads to the course playback page.
8. **Third-Party Payment Integration**: Support for Alipay, WeChat Pay, and PayNow via QR codes and official web links. Includes currency exchange reference redirection.
9. **Forum (Discussion Board)**: Full CRUD support for forum topics, with table/card view, sorting, image uploads, and reset functions.
10. **User Personalization**: Users can choose or upload background images for the site, with real-time dynamic loading and persistent state between sessions.

---

## Use Case Overview

| ID   | Use Case                       | Actor                       | Precondition                  | Postcondition                              |
| ---- | ------------------------------ | --------------------------- | ----------------------------- | ------------------------------------------- |
| UC01 | User Registration              | Regular User                | No existing account           | New user account is created                 |
| UC02 | User Login                     | Regular User / Administrator | Registered account            | Access to the system is granted             |
| UC03 | Browse Courses                 | Regular User                | Logged in                     | Course list is displayed                    |
| UC04 | View Course Details            | Regular User                | Logged in & course selected   | Detailed course information is displayed    |
| UC05 | Enroll in Course               | Regular User                | Logged in & viewing course    | Enrollment added to “My Courses”            |
| UC06 | View My Courses                | Regular User                | Logged in & enrolled in course | List of enrolled courses is displayed       |
| UC07 | Create/Edit/Delete Course      | Administrator               | Logged in with admin rights   | Course catalog is updated                   |
| UC08 | Create/Edit/Delete Announcement | Administrator              | Logged in with admin rights   | System announcements are updated            |
| UC09 | User Management                | Administrator               | Logged in with admin rights   | Users can be added/edited/deleted           |
| UC10 | Data Statistics & Export       | Administrator               | Logged in                     | User/course enrollment data exported        |

---

## Feature List

| Module        | Feature                            | Description                                           |
| ------------- | ---------------------------------- | ----------------------------------------------------- |
| Common        | User Registration                  | Register with email/username and send verification email |
| Common        | User Login/Logout                  | Cookie-based authentication                           |
| Common        | Password Reset                     | Reset password via email link                         |
| Courses       | Course Listing & Search            | Pagination; filter by category or keyword             |
| Courses       | Course Details                     | Includes lesson list, instructor info, syllabus       |
| Courses       | Course Enrollment                  | Add course to personal learning plan                  |
| User Dashboard| My Courses                         | List enrolled courses; option to cancel enrollment    |
| Admin Panel   | Course Management                  | Create, edit, delete courses and lessons              |
| Admin Panel   | User Management                    | View user list, assign roles, reset passwords         |
| Admin Panel   | Announcement Management            | CRUD for system announcements                         |
| Admin Panel   | Data Export                        | Export reports in CSV/Excel format                    |
| Admin Panel   | Wallet Activation Code Management  | CRUD and search/sort/reset wallet recharge codes      |
| Admin Panel   | Admin Activation Code Management   | CRUD and search/sort/reset admin access codes         |
| Payments      | Wallet Recharge via Activation Code| Convert activation codes into wallet currency         |
| Payments      | Third-Party Payment Integration    | Alipay, WeChat Pay, PayNow with QR and redirection    |
| Forum         | Forum Management                   | CRUD for forum topics, with image uploads and view modes |
| Personalization | Background Image Customization   | User-uploaded or selected backgrounds, stored persistently |

---

## User Roles

| Role            | Permissions                                                    | Accessible Pages                                                       |
| --------------- | -------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Regular User    | Browse/enroll in courses; view profile & enrolled courses      | Home, Course List, Course Details, User Dashboard                      |
| Administrator   | All regular user permissions plus system data management       | Admin Panel (Course Management, User Management, Announcement Management, Data Statistics, Wallet/Admin Code Management) |

---

## Database Design

| Table Name        | Description             | Key Columns                                   |
| ----------------- | ----------------------- | --------------------------------------------- |
| AspNetUsers       | System users            | Id, UserName, Email, PasswordHash, Role       |
| Courses           | Courses                 | Id, Title, Description, Price, CreateDate     |
| Lectures          | Course lectures         | Id, CourseId, Title, ContentUrl, Order        |
| Enrollments       | Enrollments             | Id, UserId, CourseId, EnrollDate              |
| Announcements     | Announcements           | Id, Title, Content, PublishDate               |
| WalletCodes       | Wallet recharge codes   | Id, Code, Value, IsUsed, CreatedAt            |
| AdminCodes        | Admin access codes      | Id, Code, IsUsed, CreatedAt                   |
| ForumTopics       | Forum topics            | Id, Title, Description, ImageUrl, CreatedAt   |
| Likes             | User course likes       | Id, UserId, CourseId                          |
| Favorites         | User course favorites   | Id, UserId, CourseId                          |
| Purchases         | User course purchases   | Id, UserId, CourseId, PaidAmount              |

---

## Project Structure

| Path           | Description                        |
| -------------- | ---------------------------------- |
| /Controllers   | MVC controllers                    |
| /Models        | Entity and ViewModel classes       |
| /Views         | Razor view files                   |
| /Data          | DbContext and migration files      |
| /wwwroot       | Static assets (CSS/JS/Images)      |
| /Services      | Business logic layer               |
| /Helpers       | Utility classes                    |

---

## Team Member Contribution (Latest Sprint)

| Member         | Completed Modules                                                                                       |
| -------------- | ------------------------------------------------------------------------------------------------------- |
| **Luo Ziyi**   | User Management System (CRUD, filter, sort, pagination), User Personalization (background selection/upload) |
| **Zhang Zhiheng**    | Course Access via Dashboard Links, Third-party Payment System (Alipay, WeChat, PayNow), Forum System (CRUD, UI, image) |
| **Li Yuze**    | Wallet Activation Code Management, Admin Code Management, Course Interaction (like, purchase, collect), Wallet Logic |

---

## Quick Start

1. Clone the repository  
   ```bash
   git clone https://github.com/Gilfoyles-Anton/Learnhub-web-app.git


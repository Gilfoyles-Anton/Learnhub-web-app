# MVC Web Application Development Process

## 1. Requirement Analysis & System Design
| Aspect                       | Details |
|------------------------------|--------------------------------------|
| **Business Requirement Analysis** | Define functional requirements, user roles, usage scenarios |
| **Architecture Design**       | MVC, layered architecture, RESTful API design |

## 2. Development Environment Setup
| Category          | Tools & Technologies |
|------------------|--------------------|
| **Development Tools** | Visual Studio, Git, Postman |
| **Frameworks & Libraries** | ASP.NET Core MVC, Entity Framework, Vue, React |
| **Databases** | SQL Server, MySQL, MongoDB |

## 3. Database Design & Setup
| Aspect                | Details |
|----------------------|--------------------------------------|
| **Database Modeling** | E-R model, normalization |
| **Data Access Layer** | Repository Pattern, dependency injection |
| **Optimization** | Stored Procedure Optimization |

## 4. MVC Application Development
| Component  | Description |
|-----------|-------------------------------|
| **Model** | Data validation, ViewModel |
| **Controller** | Business logic, API design |
| **View** | Razor, frontend componentization |

## 5. Frontend Integration
| Technology | Purpose |
|-----------|----------------------------------|
| **CSS Frameworks** | Bootstrap, Tailwind CSS |
| **JavaScript Frameworks** | Vue, React |
| **Frontend Interaction** | AJAX, Axios |

## 6. Testing & Optimization
| Testing Type | Tools |
|-------------|----------------------|
| **Unit Testing** | XUnit, NUnit |
| **API Testing** | Postman, Swagger |
| **Database Optimization** | Indexing, connection pooling |

## 7. Deployment & Operations
| Process | Tools & Services |
|--------|----------------------------------|
| **Server Deployment** | IIS, Docker, Kubernetes |
| **Cloud Services** | Azure, AWS |
| **CI/CD Automation** | GitHub Actions, Azure DevOps |

---

### 📌 Notes
- This document provides a structured guideline for developing an MVC-based web application.
- For more details, refer to official documentation of **ASP.NET Core**, **Entity Framework**, and other listed technologies.
- Feel free to contribute and improve this repository!

# Shopping Website Development Project

# LearnHub Online Learning Platform Development Project

## 1. Project Overview
This project aims to develop an **online learning platform, LearnHub**, where users can search for courses, enroll in learning programs, and participate in course discussions. The system adopts the **MVC (Model-View-Controller) architecture**, is developed using **ASP.NET Core**, and supports course purchasing and interactive discussions to foster knowledge sharing and community learning.

---

## 2. Key Features
| Feature Module      | Description |
|------------------|----------------------------|
| **User Registration/Login** | Identity verification via email or phone number, password recovery support |
| **Course Browsing & Categorization** | Courses are categorized (Programming, Design, Business, etc.) and can be filtered using keywords |
| **Course Enrollment** | Free courses can be accessed instantly, paid courses require payment through third-party gateways |
| **Course Discussion Forum** | Each course includes a comment section where users can ask questions and engage in discussions |
| **Video Upload** | Users can upload educational videos, which are reviewed by administrators before publishing |
| **Learning Progress Tracking** | Users can monitor their progress and view completed courses |
| **Payment System** | Stripe or PayPal integration for secure online payments |
| **Admin Panel** | Course management, user control, content moderation |

---

## 3. Technology Stack
| Category         | Selected Technologies |
|-----------------|--------------------------------|
| **Development Tools** | Visual Studio, GitHub |
| **Backend Framework** | ASP.NET Core MVC, Entity Framework |
| **Frontend Framework** | Vue.js, React, Bootstrap |
| **Database** | SQL Server, MySQL |
| **API Interaction** | RESTful API, AJAX, Axios |
| **Payment Integration** | Stripe, PayPal |
| **Deployment Methods** | Azure, AWS, Docker, Kubernetes |

---

## 4. Use Case Descriptions
### 4.1 User Registration & Login
| Use Case Name  | User Registration & Login |
|---------------|----------------------------|
| **Actors**    | Learners, Administrators |
| **Preconditions** | User has not registered or logged in |
| **Basic Flow** | User provides email or phone number → Account verification → Successful login |
| **Extended Flow** | If the user forgets their password, they can recover it via email/phone |
| **Exceptions** | Login failure due to incorrect password or inactive account |

### 4.2 Course Browsing & Filtering
| Use Case Name  | Course Browsing & Filtering |
|---------------|----------------------------|
| **Actors**    | Learners |
| **Preconditions** | User is logged in |
| **Basic Flow** | User accesses the course catalog → Filters courses by category or keyword |
| **Extended Flow** | User can bookmark courses for future learning |
| **Exceptions** | No matching courses found |

### 4.3 Course Enrollment & Payment
| Use Case Name  | Course Enrollment & Payment |
|---------------|----------------------------|
| **Actors**    | Learners |
| **Preconditions** | User is logged in |
| **Basic Flow** | User selects a course → Paid courses lead to the payment interface → Course unlocks after successful payment |
| **Extended Flow** | User can request a refund following platform rules |
| **Exceptions** | Payment failure, course access not granted |

### 4.4 Course Forum Interaction
| Use Case Name  | Course Forum Interaction |
|---------------|----------------------------|
| **Actors**    | Learners, Instructors |
| **Preconditions** | User has enrolled in a course |
| **Basic Flow** | User enters the course forum → Posts questions or replies to others |
| **Extended Flow** | A "Popular Discussions" section enhances engagement |
| **Exceptions** | Comments violating community rules are removed |

### 4.5 Video Upload & Moderation
| Use Case Name  | Video Upload & Moderation |
|---------------|----------------------------|
| **Actors**    | Learners, Administrators |
| **Preconditions** | User has permission to upload videos |
| **Basic Flow** | User selects a file → Submits for review → Admin approves before public display |
| **Extended Flow** | Videos are categorized using tags for better searchability |
| **Exceptions** | Videos violating copyright or content regulations are rejected |

---

## 5. Development Timeline (6 Weeks)
| Development Phase | Task Description |
|---------------|-------------------------------|
| **Week 1**  | Project planning, database design, MVC project initialization |
| **Week 2**  | User management module development, course browsing functionality |
| **Week 3**  | Course enrollment system, forum construction |
| **Week 4**  | Payment integration, user video upload moderation |
| **Week 5**  | Testing, UI/UX improvements, security enhancements |
| **Week 6**  | Project deployment, user feedback collection, final adjustments |

---

## 6. Team Role Assignment
| Role          | Responsibilities |
|--------------|----------------------------|
| **Product Manager** | Requirement analysis, project planning, team coordination |
| **UI/UX Designer** | User interface design and interaction optimization |
| **Frontend Developer** | Responsible for frontend component development, user interactions |
| **Backend Developer** | Server-side logic, database management, API design |
| **QA Tester** | Functionality testing, security validation, and optimization |
| **Content Moderator** | Reviewing course and video content for compliance |

---

## 7. Team Communication & Management
| Tool             | Purpose |
|-----------------|----------------------------|
| **Slack**       | Team communication, task assignment |
| **Microsoft Teams** | Online meetings, document sharing |
| **Discord**     | Real-time technical discussions |
| **Jira / Notion** | Task management & progress tracking |

---

## 8. Project Directory Structure

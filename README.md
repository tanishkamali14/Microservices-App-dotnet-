# Dotnet Microservices Shop

A scalable e-commerce backend built using **ASP.NET Core microservices architecture**, focusing on modular design, distributed systems, and containerized deployment.

---

## 🚀 Tech Stack
ASP.NET Core • Web APIs • gRPC • RabbitMQ (MassTransit) • Ocelot API Gateway  
Docker • SQL Server • PostgreSQL • MongoDB • Redis • Entity Framework Core • Dapper  

---

## 🏗️ Architecture
- Designed **independent microservices** (Catalog, Basket, Discount, Ordering)  
- Implemented **API Gateway (Ocelot)** for routing and aggregation  
- Enabled **asynchronous communication** using RabbitMQ and synchronous calls via gRPC  
- Applied **Clean Architecture, CQRS, and DDD principles**  

---

## ⚙️ Features
- Distributed, loosely coupled services with separate databases  
- Event-driven checkout and ordering workflows  
- Containerized setup using Docker Compose  
- Scalable and production-style backend design  

---

## ▶️ Run Locally
```bash
docker-compose up --build

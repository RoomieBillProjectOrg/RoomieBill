# RoomieBill System Architecture

```mermaid
graph TB
    subgraph "Frontend Application (MAUI)"
        UI[User Interface]
        Pages[XAML Pages]
        Services[API Services]
        FModels[Frontend Models]
        
        UI --> Pages
        Pages --> Services
        Services --> FModels
    end

    subgraph "Backend Server (ASP.NET Core)"
        API[API Controllers]
        Facades[Business Logic Facades]
        Validators[Validators]
        BModels[Domain Models]
        DAL[Data Access Layer]
        DB[(SQL Database)]
        Notifications[Notification Service]
        
        API --> Facades
        Facades --> BModels
        Facades --> Validators
        BModels --> DAL
        DAL --> DB
        Facades --> Notifications
    end

    Services --HTTP--> API

    subgraph "Core Features"
        direction BT
        Invites[/"Invite System"/]
        Users[/"User Management"/]
        Reminders[/"Payment Reminders"/]
        Payments[/"Payment Processing"/]
        Groups[/"Group Management"/]
        Expenses[/"Expense Management"/]
        
        Invites --- Users
        Users --- Reminders
        Reminders --- Payments
        Payments --- Groups
        Groups --- Expenses
    end
    
    API --> Expenses

```

## Component Details

### Frontend (MAUI Application)
- **User Interface**: XAML-based UI components
- **Pages**: Login, Register, Group Management, Expenses, Payments
- **Services**: API integration services for backend communication
- **Models**: Data models matching backend DTOs

### Backend (ASP.NET Core)
- **API Controllers**: RESTful endpoints for client communication
- **Facades**: Business logic implementation
- **Validators**: Input validation and business rules
- **Domain Models**: Core business entities
- **Data Access**: Entity Framework Core with SQL Server
- **Notification Service**: Email notifications for invites/reminders

### Core Features
- **Expense Management**: Track and split expenses
- **Group Management**: Create/manage roommate groups
- **Payment Processing**: Handle payments between users
- **Payment Reminders**: Automated payment notifications
- **User Management**: Authentication and profile management
- **Invite System**: Group invitation handling

## Data Flow

```mermaid
sequenceDiagram
    participant Client as MAUI Client
    participant API as API Controllers
    participant BL as Business Logic
    participant DB as Database
    participant NS as Notification Service

    Client->>API: HTTP Request
    API->>BL: Process Request
    BL->>DB: Data Operations
    DB-->>BL: Data Response
    BL-->>API: Business Response
    API-->>Client: HTTP Response
    
    opt Notifications Required
        BL->>NS: Trigger Notification
        NS-->>Client: Send Email/Push
    end
```

## Testing Architecture

```mermaid
graph TB
    subgraph "Test Projects"
        UT[Unit Tests]
        subgraph "Test Categories"
            VT[Validator Tests]
            FT[Facade Tests]
            ST[Service Tests]
            HT[Handler Tests]
        end
        
        UT --> VT
        UT --> FT
        UT --> ST
        UT --> HT
    end

    subgraph "Test Coverage"
        EV[ExpenseValidator]
        EH[ExpenseHandler]
        GF[GroupFacade]
        IF[InviteFacade]
        UF[UserFacade]
        PR[PaymentReminder]
        PS[PaymentService]
    end

    VT --> EV
    HT --> EH
    FT --> GF
    FT --> IF
    FT --> UF
    ST --> PR
    ST --> PS
```

## Data Relationships

```mermaid
erDiagram
    User ||--o{ Group : "belongs to"
    Group ||--o{ Expense : "has"
    User ||--o{ Expense : "creates"
    Expense ||--|{ ExpenseSplit : "splits into"
    User ||--o{ PaymentReminder : "has"
    Group ||--o{ PaymentReminder : "contains"
    User ||--o{ Invite : "receives"
    Group ||--o{ Invite : "generates"
```


## Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant Client as MAUI Client
    participant Auth as Auth Controller
    participant DB as Database

    User->>Client: Enter Credentials
    Client->>Auth: Login Request
    Auth->>DB: Validate User
    DB-->>Auth: User Data
    Auth-->>Client: JWT Token
    Client->>Client: Store Token
    Client-->>User: Login Success
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Client Deployment"
        MA[MAUI App Store]
        iOS[iOS App]
        And[Android App]
        Win[Windows App]
        
        MA --> iOS
        MA --> And
        MA --> Win
    end

    subgraph "Server Deployment"
        LB[Load Balancer]
        API1[API Server 1]
        API2[API Server 2]
        DB[(SQL Server)]
        Redis[Redis Cache]
        
        LB --> API1
        LB --> API2
        API1 --> DB
        API2 --> DB
        API1 --> Redis
        API2 --> Redis
    end

    iOS --HTTP/S--> LB
    And --HTTP/S--> LB
    Win --HTTP/S--> LB
```

## Security Architecture

```mermaid
graph LR
    subgraph "Security Layers"
        Auth[Authentication]
        JWT[JWT Token]
        HTTPS[HTTPS/TLS]
        Val[Input Validation]
        Enc[Data Encryption]
        
        Auth --> JWT
        JWT --> HTTPS
        HTTPS --> Val
        Val --> Enc
    end

    subgraph "Security Features"
        PP[Password Policy]
        RL[Rate Limiting]
        SS[SQL Sanitization]
        XSS[XSS Prevention]
        CSRF[CSRF Protection]
    end
    
    Val --> PP
    Val --> RL
    Val --> SS
    Val --> XSS
    Val --> CSRF
```

Key Security Features:
- **Authentication**: JWT-based authentication with refresh tokens
- **Transport Security**: HTTPS/TLS for all API communications
- **Data Protection**: Encryption at rest for sensitive data
- **Input Validation**: Client and server-side validation
- **Rate Limiting**: Prevents abuse of API endpoints
- **SQL Protection**: Parameterized queries and ORM usage
- **XSS Prevention**: Input sanitization and output encoding
- **CSRF Protection**: Anti-forgery tokens for forms

## System Components & Deployment

### Component Distribution

The RoomieBill system is distributed across three main deployment targets:

1. **Client Devices**
   - MAUI Application Files (*.dll, *.exe)
   - Local Configuration (appsettings.json)
   - Cache Directory (~/AppData/Local/RoomieBill)
   - JWT Token Storage
   - UI Resources (images, styles)

2. **Application Servers**
   - ASP.NET Core Web API
   - Business Logic Layer
   - API Controllers
   - Validation Components
   - Email Service
   - Logging Service

3. **Database Servers**
   - SQL Server Database
   - Redis Cache
   - Database Backups
   - Migration Scripts

### File Distribution

```mermaid
graph TB
    subgraph "Client Device"
        direction TB
        subgraph "MAUI App"
            ClientConfig[/"appsettings.json"/]
            UIResources[/"Resources/*.xaml"/]
            ClientDLL[/"RoomieBill.dll"/]
            Cache[/"Local Cache"/]
        end
    end

    subgraph "Application Server"
        direction TB
        subgraph "Web API"
            ServerConfig[/"appsettings.json"/]
            Controllers[/"*Controller.cs"/]
            Services[/"*Service.cs"/]
            Facades[/"*Facade.cs"/]
        end
        
        subgraph "File Storage"
            Logs[/"*.log"/]
            Temp[/"temp/*"/]
            EmailTemplates[/"templates/*"/]
        end
    end

    subgraph "Database Server"
        direction TB
        subgraph "Storage"
            SQLData[/"*.mdf"/]
            SQLLogs[/"*.ldf"/]
            Backups[/"*.bak"/]
        end
        
        subgraph "Redis"
            Cache2[/"Cache Data"/]
            SessionData[/"Session Data"/]
        end
    end

    Client1[Client 1] --> MAUI
    Client2[Client 2] --> MAUI
    Client3[Client 3] --> MAUI
    MAUI["MAUI Apps"] --> LoadBalancer
    LoadBalancer --> AppServer1["App Server 1"]
    LoadBalancer --> AppServer2["App Server 2"]
    AppServer1 --> MainDB["Primary DB"]
    AppServer2 --> MainDB
    AppServer1 --> RedisCache["Redis Cache"]
    AppServer2 --> RedisCache
```

### Component Responsibilities

1. **MAUI Frontend (Client-side)**
   - User Interface Rendering
   - Local State Management
   - API Communication
   - Form Validation
   - File Location: Deployed to client devices via app stores

2. **ASP.NET Core Backend (Server-side)**
   - API Endpoints
   - Business Logic
   - Data Validation
   - Authentication/Authorization
   - File Location: Deployed to cloud servers

3. **Database Layer**
   - Data Storage
   - Transaction Management
   - Data Integrity
   - File Location: Dedicated database servers

### File Locations & Purpose

#### Client Files
- `/FrontendApplication/*.dll` - MAUI application binaries
- `/FrontendApplication/Resources/*` - UI resources and assets
- `%AppData%/RoomieBill/cache/*` - Local cache data
- `%AppData%/RoomieBill/config.json` - Client configuration

#### Server Files
- `/Roomiebill.Server/*.dll` - Server application binaries
- `/Roomiebill.Server/appsettings.json` - Server configuration
- `/Roomiebill.Server/logs/*` - Application logs
- `/Roomiebill.Server/templates/*` - Email templates

#### Database Files
- `SQLSERVER/RoomieBill.mdf` - Main database file
- `SQLSERVER/RoomieBill.ldf` - Database log file
- `REDIS/dump.rdb` - Redis persistence file

### Communication Patterns

```mermaid
sequenceDiagram
    participant Client as MAUI Client
    participant API as Load Balancer
    participant Server as App Server
    participant Cache as Redis Cache
    participant DB as SQL Server

    Client->>+API: HTTP Request
    API->>+Server: Route Request
    
    alt Cache Hit
        Server->>Cache: Check Cache
        Cache-->>Server: Return Cached Data
    else Cache Miss
        Server->>DB: Query Data
        DB-->>Server: Return Data
        Server->>Cache: Update Cache
    end
    
    Server-->>-API: Response
    API-->>-Client: HTTP Response
```

### Network Configuration

1. **Client-Server Communication**
   - Protocol: HTTPS (443)
   - Load Balancer: Azure Load Balancer
   - SSL/TLS: Version 1.3
   - Connection Pool: Max 100 per server

2. **Inter-Server Communication**
   - Protocol: Internal TCP
   - Port Range: 8000-8010
   - Firewall Rules: Allow internal subnet only
   - Load Balancing: Round-robin

3. **Database Connections**
   - SQL Server: Port 1433
   - Redis: Port 6379
   - Connection Pooling: Enabled
   - Max Pool Size: 200

4. **Network Security**
   - VPN Access for Administration
   - Network Segmentation
   - DDoS Protection
   - Web Application Firewall (WAF)

### Data Persistence & State Management

1. **Client-Side State**
   ```mermaid
   graph LR
       subgraph "MAUI Client"
           LS[Local Storage]
           IC[In-Memory Cache]
           SS[Session Storage]
           
           LS --> IC
           SS --> IC
       end
       
       subgraph "State Types"
           UI[UI State]
           Auth[Auth Data]
           Forms[Form Data]
           
           IC --> UI
           LS --> Auth
           SS --> Forms
       end
   ```

2. **Server-Side State**
   ```mermaid
   graph LR
       subgraph "Persistence Layers"
           Redis[Redis Cache]
           SQL[SQL Server]
           Temp[Temp Storage]
           
           Redis --> SQL
           Temp --> SQL
       end
       
       subgraph "Data Categories"
           Session[Session Data]
           Trans[Transactional]
           Perm[Permanent]
           
           Redis --> Session
           SQL --> Trans
           SQL --> Perm
       end
   ```

#### State Management Strategy

1. **Transient Data**
   - UI State: Client memory
   - Session Data: Redis cache
   - Form Data: Local storage
   
2. **Persistent Data**
   - User Data: SQL Server
   - Business Data: SQL Server
   - Auth Tokens: Secure storage
   
3. **Caching Strategy**
   - L1: Client memory cache
   - L2: Redis distributed cache
   - L3: SQL Server
   
4. **Data Sync**
   - Real-time: WebSocket notifications
   - Background: Periodic sync
   - On-demand: User-triggered

## Error Handling & Logging

```mermaid
graph TB
    subgraph "Error Handling"
        GE[Global Exception Handler]
        CE[Custom Exceptions]
        subgraph "Error Types"
            VE[Validation Errors]
            BE[Business Logic Errors]
            AE[Authentication Errors]
            DE[Data Access Errors]
        end
        
        GE --> CE
        CE --> VE
        CE --> BE
        CE --> AE
        CE --> DE
    end

    subgraph "Logging Framework"
        Log[Logger]
        subgraph "Log Types"
            IL[Info Logs]
            EL[Error Logs]
            DL[Debug Logs]
            ML[Metrics Logs]
        end
        
        Log --> IL
        Log --> EL
        Log --> DL
        Log --> ML
    end

    subgraph "Monitoring"
        Met[Metrics]
        Trace[Tracing]
        Alert[Alerts]
        
        Log --> Met
        Met --> Trace
        Trace --> Alert
    end
```

Key Error Handling Features:
- **Global Exception Handler**: Centralizes error handling
- **Custom Exceptions**: Domain-specific error types
- **Error Response Model**: Standardized error responses
- **Logging & Monitoring**: Tracks system health and issues

# API ENDPOINTS SPECIFICATION
## RESTful API Design for Waste Collection Platform

---

##  AUTHENTICATION ENDPOINTS

### **POST** `/api/auth/register`
**Description:** Register new user (Citizen/Enterprise/Collector)  
**Request Body:**
```json
{
  "fullName": "string",
  "email": "string",
  "password": "string",
  "phone": "string",
  "role": "Citizen|Enterprise|Collector",
  "districtId": "guid" // Required for Enterprise/Collector
}
```
**Response:** `201 Created`
```json
{
  "success": true,
  "message": "Registration successful. Please wait for admin approval.", // For Enterprise
  "data": {
    "userId": "guid",
    "email": "string",
    "role": "string"
  }
}
```

---

### **POST** `/api/auth/login`
**Description:** Authenticate user and get JWT token  
**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "token": "jwt-token-string",
    "userId": "guid",
    "role": "string",
    "expiresAt": "2024-12-31T23:59:59Z"
  }
}
```

---

### **POST** `/api/auth/refresh-token`
**Description:** Refresh expired JWT token  
**Authorization:** Bearer token  
**Response:** `200 OK` (same as login)

---

### **POST** `/api/auth/logout`
**Description:** Invalidate current token (optional - depends on token blacklisting)  
**Authorization:** Bearer token  
**Response:** `200 OK`

---

##  CITIZEN ENDPOINTS

### **POST** `/api/citizen/reports`
**Description:** Create new waste report  
**Authorization:** Bearer token (Citizen role)  
**Request Body:**
```json
{
  "description": "string",
  "wasteType": "Plastic|Paper|Metal|Organic|Electronic|Hazardous",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "images": ["base64-string"] // Array of base64 images
}
```
**Response:** `201 Created`
```json
{
  "success": true,
  "message": "Report created successfully",
  "data": {
    "reportId": "guid",
    "status": "Pending",
    "createdAt": "2024-02-05T10:00:00Z"
  }
}
```

---

### **GET** `/api/citizen/reports`
**Description:** Get citizen's report history with filters  
**Authorization:** Bearer token (Citizen role)  
**Query Parameters:**
- `status` (optional): Filter by status
- `fromDate` (optional): Start date
- `toDate` (optional): End date
- `page` (default: 1)
- `pageSize` (default: 10)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "reportId": "guid",
        "description": "string",
        "wasteType": "string",
        "status": "Collected",
        "createdAt": "2024-02-05T10:00:00Z",
        "collectorName": "string",
        "pointsEarned": 50
      }
    ],
    "totalCount": 25,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

---

### **GET** `/api/citizen/reports/{reportId}`
**Description:** Get specific report details  
**Authorization:** Bearer token (Citizen role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "reportId": "guid",
    "description": "string",
    "wasteType": "string",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "status": "OnTheWay",
    "images": ["url1", "url2"],
    "collectorName": "John Doe",
    "collectorPhone": "+84901234567",
    "createdAt": "2024-02-05T10:00:00Z",
    "statusHistory": [
      {
        "status": "Pending",
        "timestamp": "2024-02-05T10:00:00Z"
      },
      {
        "status": "Accepted",
        "timestamp": "2024-02-05T10:05:00Z"
      }
    ]
  }
}
```

---

### **GET** `/api/citizen/points`
**Description:** Get citizen's point balance and history  
**Authorization:** Bearer token (Citizen role)  
**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalPoints": 450,
    "history": [
      {
        "pointLogId": "guid",
        "amount": 50,
        "reason": "Report Collected",
        "reportId": "guid",
        "createdAt": "2024-02-05T10:00:00Z"
      },
      {
        "pointLogId": "guid",
        "amount": -10,
        "reason": "Invalid Report",
        "reportId": "guid",
        "createdAt": "2024-02-04T15:00:00Z"
      }
    ]
  }
}
```

---

### **GET** `/api/citizen/vouchers`
**Description:** Get available vouchers for redemption  
**Authorization:** Bearer token (Citizen role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "voucherId": "guid",
      "voucherName": "10% Off Groceries",
      "pointsRequired": 100,
      "stockQuantity": 50,
      "expiryDate": "2024-12-31T23:59:59Z"
    }
  ]
}
```

---

### **POST** `/api/citizen/vouchers/{voucherId}/redeem`
**Description:** Redeem voucher with points  
**Authorization:** Bearer token (Citizen role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Voucher redeemed successfully",
  "data": {
    "voucherCode": "ABC123XYZ",
    "remainingPoints": 350
  }
}
```

---

##  ENTERPRISE ENDPOINTS

### **GET** `/api/enterprise/reports/pending`
**Description:** Get list of pending reports matching enterprise's service area and waste types  
**Authorization:** Bearer token (Enterprise role)  
**Query Parameters:**
- `wasteType` (optional): Filter by waste type
- `page` (default: 1)
- `pageSize` (default: 10)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "reportId": "guid",
        "citizenName": "Anonymous", // Privacy protection
        "description": "Large plastic waste",
        "wasteType": "Plastic",
        "districtName": "District 1",
        "latitude": 10.762622,
        "longitude": 106.660172,
        "createdAt": "2024-02-05T10:00:00Z",
        "images": ["url1"]
      }
    ],
    "totalCount": 15,
    "currentPage": 1
  }
}
```

---

### **POST** `/api/enterprise/reports/{reportId}/accept`
**Description:** Accept a pending report  
**Authorization:** Bearer token (Enterprise role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Report accepted successfully",
  "data": {
    "reportId": "guid",
    "status": "Accepted"
  }
}
```
**Error Responses:**
- `400 Bad Request` - Capacity limit reached (R3)
- `404 Not Found` - Report not found or not in service area

---

### **POST** `/api/enterprise/reports/{reportId}/reject`
**Description:** Reject a report with feedback  
**Authorization:** Bearer token (Enterprise role)  
**Request Body:**
```json
{
  "reason": "Not in our service area|Waste type not supported|Other",
  "feedback": "string"
}
```
**Response:** `200 OK`

---

### **POST** `/api/enterprise/reports/{reportId}/assign`
**Description:** Assign collector to accepted report  
**Authorization:** Bearer token (Enterprise role)  
**Request Body:**
```json
{
  "collectorId": "guid"
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Collector assigned successfully",
  "data": {
    "reportId": "guid",
    "collectorName": "John Doe",
    "status": "Assigned"
  }
}
```

---

### **GET** `/api/enterprise/collectors/available`
**Description:** Get list of available collectors in enterprise's district  
**Authorization:** Bearer token (Enterprise role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "collectorId": "guid",
      "fullName": "John Doe",
      "currentTaskCount": 2,
      "status": "Available"
    }
  ]
}
```

---

### **GET** `/api/enterprise/reports/statistics`
**Description:** Get enterprise's collection statistics  
**Authorization:** Bearer token (Enterprise role)  
**Query Parameters:**
- `fromDate` (required)
- `toDate` (required)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalReportsAccepted": 120,
    "totalReportsCollected": 110,
    "totalReportsFailed": 10,
    "wasteTypeBreakdown": {
      "Plastic": 50,
      "Paper": 30,
      "Metal": 20,
      "Organic": 10,
      "Electronic": 10
    },
    "averageCollectionTime": "2.5 hours",
    "currentDailyVolume": 15,
    "capacityLimit": 50
  }
}
```

---

##  COLLECTOR ENDPOINTS

### **GET** `/api/collector/tasks/assigned`
**Description:** Get list of assigned tasks  
**Authorization:** Bearer token (Collector role)  
**Query Parameters:**
- `status` (optional): Filter by status (Assigned|OnTheWay)
- `page` (default: 1)
- `pageSize` (default: 10)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "reportId": "guid",
      "description": "string",
      "wasteType": "Plastic",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "districtName": "District 1",
      "citizenName": "Nguyen Van A",
      "citizenPhone": "+84901234567",
      "status": "Assigned",
      "assignedAt": "2024-02-05T10:00:00Z",
      "images": ["url1"]
    }
  ]
}
```

---

### **PUT** `/api/collector/tasks/{reportId}/status`
**Description:** Update collection status  
**Authorization:** Bearer token (Collector role)  
**Request Body:**
```json
{
  "status": "OnTheWay|Collected",
  "note": "string" // Optional
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Status updated successfully",
  "data": {
    "reportId": "guid",
    "status": "OnTheWay"
  }
}
```

---

### **POST** `/api/collector/tasks/{reportId}/complete`
**Description:** Mark task as completed with confirmation photo  
**Authorization:** Bearer token (Collector role)  
**Request Body:**
```json
{
  "isValid": true,
  "confirmationImage": "base64-string",
  "actualWasteType": "Plastic", // Can update if different from report
  "note": "string"
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Task completed successfully",
  "data": {
    "reportId": "guid",
    "status": "Collected",
    "pointsAwarded": 50 // To citizen
  }
}
```

---

### **POST** `/api/collector/tasks/{reportId}/mark-invalid`
**Description:** Mark report as invalid (deduct citizen points)  
**Authorization:** Bearer token (Collector role)  
**Request Body:**
```json
{
  "reason": "No waste found|Wrong location|Duplicate report|Other",
  "note": "string",
  "evidenceImage": "base64-string"
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Report marked as invalid",
  "data": {
    "reportId": "guid",
    "status": "Failed",
    "pointsDeducted": 10 // From citizen
  }
}
```

---

### **GET** `/api/collector/tasks/history`
**Description:** Get collector's task completion history  
**Authorization:** Bearer token (Collector role)  
**Query Parameters:**
- `fromDate` (optional)
- `toDate` (optional)
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:** `200 OK` (similar to assigned tasks)

---

## 📋 TEAM ENDPOINTS

### **GET** `/api/team`
**Description:** Get all teams (for Collector registration)  
**Authorization:** None (public endpoint)  
**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Teams retrieved successfully.",
  "data": [
    {
      "teamId": 3,
      "name": "Team North",
      "areaId": 1
    },
    {
      "teamId": 4,
      "name": "Team South",
      "areaId": 2
    }
  ]
}
```

---

### **GET** `/api/team/{id}`
**Description:** Get team details by ID  
**Authorization:** None (public endpoint)  
**Path Parameters:**
- `id` (integer): Team ID

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Team retrieved successfully.",
  "data": {
    "teamId": 3,
    "name": "Team North",
    "areaId": 1
  }
}
```

**Error Response:** `404 Not Found`
```json
{
  "success": false,
  "message": "Team with ID 999 not found.",
  "errors": ["Team not found."]
}
```

---

##  ADMIN ENDPOINTS

### **GET** `/api/admin/users`
**Description:** Get all users with filters  
**Authorization:** Bearer token (Admin role)  
**Query Parameters:**
- `role` (optional): Filter by role
- `status` (optional): Filter by status
- `search` (optional): Search by name/email
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "guid",
        "fullName": "string",
        "email": "string",
        "phone": "string",
        "role": "Enterprise",
        "status": "Pending",
        "createdAt": "2024-02-05T10:00:00Z"
      }
    ],
    "totalCount": 150
  }
}
```

---

### **PUT** `/api/admin/users/{userId}/approve`
**Description:** Approve pending enterprise account (R1)  
**Authorization:** Bearer token (Admin role)  
**Response:** `200 OK`

---

### **PUT** `/api/admin/users/{userId}/reject`
**Description:** Reject pending enterprise account  
**Authorization:** Bearer token (Admin role)  
**Request Body:**
```json
{
  "reason": "string"
}
```
**Response:** `200 OK`

---

### **DELETE** `/api/admin/users/{userId}`
**Description:** Deactivate user account  
**Authorization:** Bearer token (Admin role)  
**Response:** `200 OK`

---

### **GET** `/api/admin/reports`
**Description:** Get all reports with advanced filters  
**Authorization:** Bearer token (Admin role)  
**Query Parameters:**
- `status` (optional)
- `districtId` (optional)
- `fromDate` (optional)
- `toDate` (optional)
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:** `200 OK` (similar structure to citizen reports)

---

### **GET** `/api/admin/dashboard/statistics`
**Description:** Get overall system statistics  
**Authorization:** Bearer token (Admin role)  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalUsers": {
      "citizens": 1000,
      "enterprises": 50,
      "collectors": 100
    },
    "reports": {
      "total": 5000,
      "pending": 50,
      "inProgress": 100,
      "completed": 4800,
      "failed": 50
    },
    "pointsSystem": {
      "totalPointsIssued": 50000,
      "totalPointsRedeemed": 20000
    },
    "topDistricts": [
      {
        "districtName": "District 1",
        "reportCount": 500
      }
    ]
  }
}
```

---

### **GET/POST/PUT/DELETE** `/api/admin/districts`
**Description:** CRUD operations for districts  
**Authorization:** Bearer token (Admin role)

---

### **GET/POST/PUT/DELETE** `/api/admin/vouchers`
**Description:** CRUD operations for vouchers  
**Authorization:** Bearer token (Admin role)

---

##  NOTIFICATION ENDPOINTS

### **GET** `/api/notifications`
**Description:** Get user's notifications  
**Authorization:** Bearer token (All roles)  
**Query Parameters:**
- `unreadOnly` (default: false)
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "notificationId": "guid",
      "message": "Your report has been accepted",
      "type": "StatusUpdate",
      "reportId": "guid",
      "isRead": false,
      "createdAt": "2024-02-05T10:00:00Z"
    }
  ]
}
```

---

### **PUT** `/api/notifications/{notificationId}/mark-read`
**Description:** Mark notification as read  
**Authorization:** Bearer token (All roles)  
**Response:** `200 OK`

---

### **PUT** `/api/notifications/mark-all-read`
**Description:** Mark all notifications as read  
**Authorization:** Bearer token (All roles)  
**Response:** `200 OK`

---

##  DISTRICT ENDPOINTS

### **GET** `/api/districts`
**Description:** Get all districts (public)  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "districtId": "guid",
      "districtName": "District 1"
    }
  ]
}
```

---

##  UTILITY ENDPOINTS

### **POST** `/api/gps/determine-district`
**Description:** Determine district from GPS coordinates (used during report creation)  
**Request Body:**
```json
{
  "latitude": 10.762622,
  "longitude": 106.660172
}
```
**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "districtId": "guid",
    "districtName": "District 1"
  }
}
```

---

##  SIGNALR HUB EVENTS

### Hub URL: `/hubs/notification`

### Client Methods (Receive):
1. **`ReceiveNotification`**
   ```json
   {
     "notificationId": "guid",
     "message": "string",
     "type": "StatusUpdate",
     "reportId": "guid"
   }
   ```

2. **`ReportStatusChanged`**
   ```json
   {
     "reportId": "guid",
     "newStatus": "Collected",
     "collectorName": "John Doe"
   }
   ```

### Server Methods (Send):
1. **`JoinUserGroup(userId)`** - Join user's notification group
2. **`LeaveUserGroup(userId)`** - Leave notification group

---

##  ERROR RESPONSE FORMAT

All error responses follow this format:
```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    {
      "field": "email",
      "message": "Email is already registered"
    }
  ],
  "timestamp": "2024-02-05T10:00:00Z"
}
```

### HTTP Status Codes:
- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Authentication failed
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Business rule violation (e.g., capacity limit)
- `500 Internal Server Error` - Server error

---

## 🔒 AUTHORIZATION MATRIX

| Endpoint Group | Citizen | Enterprise | Collector | Admin |
|---------------|---------|------------|-----------|-------|
| Auth | ✅ | ✅ | ✅ | ✅ |
| Citizen Reports | ✅ | ❌ | ❌ | ✅ (read) |
| Enterprise Management | ❌ | ✅ | ❌ | ✅ |
| Collector Tasks | ❌ | ❌ | ✅ | ✅ (read) |
| Admin Panel | ❌ | ❌ | ❌ | ✅ |
| Notifications | ✅ | ✅ | ✅ | ✅ |

---

##  NOTES FOR IMPLEMENTATION

1. **Pagination:** All list endpoints support pagination with `page` and `pageSize`
2. **Filtering:** Use query parameters for filtering, not POST body
3. **File Upload:** Use base64 encoding for images initially (consider multipart/form-data later)
4. **Rate Limiting:** Implement rate limiting on auth endpoints (e.g., 5 login attempts per minute)
5. **API Versioning:** Consider `/api/v1/` prefix for future versions
6. **CORS:** Configure CORS to allow frontend domain
7. **Swagger:** Document all endpoints with Swagger/OpenAPI annotations

---

**Next Step:** Implement these endpoints in `API/Controllers/` following this specification.

# 🚀 HƯỚNG DẪN SỬA LỖI REFRESHTOKEN FK CONSTRAINT

## 📋 TÓM TẮT VẤN ĐỀ
Foreign Key trong bảng `refreshtoken` tham chiếu sai tên bảng:
- ❌ Hiện tại: `REFERENCES "User"(userid)` (SAI - bảng không tồn tại)
- ✅ Cần sửa: `REFERENCES "Users"("UserId")` (ĐÚNG)

---

## ⚡ CÁCH SỬA NHANH

### **Option 1: Tự động (Khuyến nghị)**
```powershell
.\fix-refreshtoken-auto.ps1
```
Script sẽ tự động:
1. Chẩn đoán database
2. Hỏi xác nhận
3. Sửa FK constraint
4. Verify kết quả

---

### **Option 2: Thủ công với psql**
```powershell
$env:PGPASSWORD="123456"
psql -h localhost -p 5432 -U postgres -d waste_management -f fix_refreshtoken_fk.sql
```

---

### **Option 3: Dùng pgAdmin/DBeaver**
1. Mở pgAdmin hoặc DBeaver
2. Connect đến database `waste_management`
3. Mở file `fix_refreshtoken_fk.sql`
4. Execute SQL:
```sql
ALTER TABLE refreshtoken DROP CONSTRAINT IF EXISTS fk_refreshtoken_user;
ALTER TABLE refreshtoken ADD CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) REFERENCES "Users"("UserId") ON DELETE CASCADE;
```

---

## 📊 FILES ĐÃ TẠO

| File | Mục đích |
|------|----------|
| `ERROR_ANALYSIS_REFRESHTOKEN.md` | Báo cáo chi tiết cho ChatGPT |
| `diagnose_refreshtoken_fk.sql` | Script chẩn đoán database |
| `fix_refreshtoken_fk.sql` | Script sửa nhanh (basic) |
| `fix_refreshtoken_comprehensive.sql` | Script sửa đầy đủ (nhiều options) |
| `fix-refreshtoken-auto.ps1` | PowerShell tự động fix |
| `HOW_TO_FIX_REFRESHTOKEN.md` | File này |

---

## ✅ SAU KHI SỬA

1. **Test lại đăng ký trên Swagger UI:**
```json
{
  "fullName": "Tran Thi Uyen Nhi",
  "email": "Vannhattran35@gmail.com",
  "password": "Testnumber1@123",
  "phone": "0946584167",
  "role": 0
}
```

2. **Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": 30,
    "email": "Vannhattran35@gmail.com",
    "fullName": "Tran Thi Uyen Nhi",
    "role": "Citizen",
    "token": "eyJhbGc...",
    "expiresAt": "2026-03-12T14:45:00Z",
    "status": true
  }
}
```

---

## 🔍 VERIFY FIX THÀNH CÔNG

Chạy SQL này để kiểm tra:
```sql
SELECT 
    tc.constraint_name,
    ccu.table_name AS references_table,
    ccu.column_name AS references_column
FROM information_schema.table_constraints AS tc
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name = 'refreshtoken';
```

**Kết quả đúng:**
```
constraint_name        | references_table | references_column
-----------------------|------------------|------------------
fk_refreshtoken_user   | Users            | UserId
```

---

## 📝 GỬI CHO CHATGPT

Copy toàn bộ nội dung file `ERROR_ANALYSIS_REFRESHTOKEN.md` và gửi cho ChatGPT với prompt:

```
Tôi đang gặp lỗi Foreign Key Constraint khi đăng ký user trong .NET 8 + PostgreSQL + EF Core.
Đây là báo cáo chi tiết:

[Paste nội dung ERROR_ANALYSIS_REFRESHTOKEN.md]

Hãy phân tích và đưa ra giải pháp tốt nhất.
```

---

## ⚠️ LƯU Ý

- **Backup database trước khi sửa** (nếu có data quan trọng)
- Script sửa sẽ **DROP và RECREATE constraint** → safe operation, không mất data
- Nếu có RefreshToken đang tồn tại trong DB với `userid` không hợp lệ → sẽ bị reject khi recreate FK

---

## 🆘 NẾU VẪN LỖI

1. Chạy diagnosis:
```powershell
$env:PGPASSWORD="123456"
psql -h localhost -p 5432 -U postgres -d waste_management -f diagnose_refreshtoken_fk.sql
```

2. Copy kết quả và gửi cho ChatGPT kèm ERROR_ANALYSIS_REFRESHTOKEN.md

3. Hoặc liên hệ team lead để assist

---

**Chúc may mắn!** 🚀

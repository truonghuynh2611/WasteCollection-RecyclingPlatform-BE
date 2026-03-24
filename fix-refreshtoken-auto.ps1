# ========================================
# AUTO FIX REFRESHTOKEN FK CONSTRAINT
# PowerShell Script
# ========================================

param(
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432",
    [string]$DbName = "waste_management",
    [string]$DbUser = "postgres",
    [string]$DbPassword = "123456"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RefreshToken FK Constraint Fixer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "❌ ERROR: psql not found in PATH" -ForegroundColor Red
    Write-Host "Please install PostgreSQL client tools or use pgAdmin/DBeaver" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Manual steps:" -ForegroundColor Yellow
    Write-Host "1. Open pgAdmin or DBeaver" -ForegroundColor Gray
    Write-Host "2. Connect to database: $DbName" -ForegroundColor Gray
    Write-Host "3. Run file: fix_refreshtoken_fk.sql" -ForegroundColor Gray
    exit 1
}

# Step 1: Diagnose
Write-Host "Step 1: Running diagnosis..." -ForegroundColor Yellow
$env:PGPASSWORD = $DbPassword

try {
    $diagResult = psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f "diagnose_refreshtoken_fk.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Diagnosis completed" -ForegroundColor Green
        Write-Host ""
        Write-Host "Diagnosis Results:" -ForegroundColor Yellow
        $diagResult | Write-Host -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "⚠️ Diagnosis had issues, but continuing..." -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Could not run diagnosis: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 2: Ask for confirmation
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "Ready to fix FK constraint" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "This will:" -ForegroundColor White
Write-Host "  1. Drop constraint: fk_refreshtoken_user" -ForegroundColor Gray
Write-Host "  2. Recreate with correct reference: Users.UserId" -ForegroundColor Gray
Write-Host ""

$confirm = Read-Host "Do you want to proceed? (Y/N)"

if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

# Step 3: Apply fix
Write-Host ""
Write-Host "Step 2: Applying fix..." -ForegroundColor Yellow

try {
    $fixResult = psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f "fix_refreshtoken_fk.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Fix applied successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Fix Results:" -ForegroundColor Yellow
        $fixResult | Write-Host -ForegroundColor Gray
    } else {
        Write-Host "❌ Fix failed!" -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        $fixResult | Write-Host -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error applying fix: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Verify
Write-Host ""
Write-Host "Step 3: Verifying fix..." -ForegroundColor Yellow

$verifySQL = @"
SELECT 
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name = 'refreshtoken';
"@

try {
    $verifyResult = $verifySQL | psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -t 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Verification completed" -ForegroundColor Green
        Write-Host ""
        Write-Host "Current FK Constraints:" -ForegroundColor Yellow
        $verifyResult | Write-Host -ForegroundColor Gray
    }
} catch {
    Write-Host "⚠️ Could not verify: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ FIX COMPLETED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test registration again on Swagger UI" -ForegroundColor Gray
Write-Host "2. Use this test data:" -ForegroundColor Gray
Write-Host @"
   {
     "fullName": "Test User",
     "email": "test@example.com",
     "password": "Test@123",
     "phone": "0901234567",
     "role": 0
   }
"@ -ForegroundColor Cyan
Write-Host ""

# Cleanup
Remove-Item Env:PGPASSWORD

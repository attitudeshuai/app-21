$BASE = "http://localhost:8096/api"
$pass = 0
$fail = 0

function Test-API {
    param($Name, $Method, $Path, $Body, $Token, $ExpectCode = 200)
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    try {
        if ($Body) {
            $r = Invoke-RestMethod -Uri "$BASE$Path" -Method $Method -Headers $headers -Body ($Body | ConvertTo-Json -Depth 5) -ContentType "application/json"
        } else {
            $r = Invoke-RestMethod -Uri "$BASE$Path" -Method $Method -Headers $headers
        }
        $actualCode = $r.code
        if ($actualCode -eq $ExpectCode) {
            $script:pass++
            $status = "PASS"
        } else {
            $script:fail++
            $status = "FAIL (expected $ExpectCode, got $actualCode - $($r.message))"
        }
        Write-Host "  [$status] $Name"
        return $r
    } catch {
        $actualStatus = 0
        if ($_.Exception.Response) {
            $actualStatus = [int]$_.Exception.Response.StatusCode
        }
        if ($actualStatus -eq $ExpectCode) {
            $script:pass++
            $status = "PASS (HTTP $actualStatus)"
        } else {
            $script:fail++
            $status = "FAIL (expected HTTP $ExpectCode, got HTTP $actualStatus)"
        }
        Write-Host "  [$status] $Name"
        return $null
    }
}

Write-Host "=== CraftSwap API Test Suite (Corrected) ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "--- Auth Module ---" -ForegroundColor Yellow

$uniq = [DateTime]::UtcNow.Ticks % 100000
$r = Test-API "POST /auth/register (new user$uniq)" "POST" "/auth/register" @{username="newuser$uniq"; email="new$uniq@test.com"; password="Test@12345"; confirmPassword="Test@12345"}

$r = Test-API "POST /auth/login (zhangsan/123456)" "POST" "/auth/login" @{emailOrUsername="zhangsan"; password="123456"}
$zhangsanToken = $r.data.token

$r = Test-API "POST /auth/login (lisi/123456)" "POST" "/auth/login" @{emailOrUsername="lisi"; password="123456"}
$lisiToken = $r.data.token

$r = Test-API "POST /auth/login (wangwu/123456)" "POST" "/auth/login" @{emailOrUsername="wangwu"; password="123456"}
$wangwuToken = $r.data.token

Test-API "GET /auth/me (zhangsan)" "GET" "/auth/me" -Token $zhangsanToken

Test-API "PUT /auth/me (update avatar)" "PUT" "/auth/me" @{avatar="https://example.com/new-avatar.jpg"} -Token $zhangsanToken

Test-API "GET /auth/me (no token -> 401)" "GET" "/auth/me" -ExpectCode 401

Test-API "POST /auth/login (wrong pwd -> 400)" "POST" "/auth/login" @{emailOrUsername="zhangsan"; password="wrongpass"} -ExpectCode 400

Write-Host ""
Write-Host "--- Materials Module ---" -ForegroundColor Yellow

$r = Test-API "GET /materials (public list)" "GET" "/materials"
Write-Host "     -> Total: $($r.data.totalCount) items"

Test-API "GET /materials/1" "GET" "/materials/1"

Test-API "GET /materials/mine (zhangsan)" "GET" "/materials/mine" -Token $zhangsanToken

$r = Test-API "POST /materials (create)" "POST" "/materials" @{
    title = "Test Blue Thread"; category = "Thread"; description = "Test blue cotton thread"
    imageUrls = @("https://example.com/test1.jpg"); tags = @("cotton", "blue")
} -Token $zhangsanToken
$newMaterialId = $r.data.id

if ($newMaterialId) {
    Test-API "PUT /materials/$newMaterialId (update)" "PUT" "/materials/$newMaterialId" @{
        title = "Test Blue Updated"; description = "Updated description"
    } -Token $zhangsanToken

    Test-API "PUT /materials/$newMaterialId (lisi forbid 403)" "PUT" "/materials/$newMaterialId" @{title="hacked"} -Token $lisiToken -ExpectCode 403

    Test-API "PATCH /materials/1/status (Reserved)" "PATCH" "/materials/1/status" @{status = "Reserved"} -Token $zhangsanToken

    Test-API "DELETE /materials/$newMaterialId (delete)" "DELETE" "/materials/$newMaterialId" -Token $zhangsanToken
}

Test-API "GET /materials?search=wool" "GET" "/materials?search=wool"
Test-API "GET /materials?category=Thread" "GET" "/materials?category=Thread"
Test-API "GET /materials?status=Available" "GET" "/materials?status=Available"
Test-API "GET /materials?pageIndex=1&pageSize=3" "GET" "/materials?pageIndex=1&pageSize=3"

Write-Host ""
Write-Host "--- Swap Requests Module ---" -ForegroundColor Yellow

Test-API "GET /swap-requests (zhangsan)" "GET" "/swap-requests" -Token $zhangsanToken

Test-API "GET /swap-requests/1" "GET" "/swap-requests/1" -Token $zhangsanToken

# Use material Ids from seed data: zhangsan owns Id=1 (wool), lisi owns Id=3
$r = Test-API "POST /swap-requests (create zhangsan->lisi mat1 for mat3)" "POST" "/swap-requests" @{
    title = "Want to swap wool for silk"
    description = "Can we swap my red wool for your green silk?"
    requesterMaterialId = 1
    responderMaterialId = 3
} -Token $zhangsanToken
$newRequestId = $r.data.id

if ($newRequestId) {
    # lisi accepts the request (receiver material 3 belongs to lisi=id2)
    Test-API "PATCH /swap-requests/$newRequestId/status (lisi accept)" "PATCH" "/swap-requests/$newRequestId/status" @{status = "Accepted"} -Token $lisiToken
    Test-API "PATCH /swap-requests/$newRequestId/status (lisi complete)" "PATCH" "/swap-requests/$newRequestId/status" @{status = "Completed"} -Token $lisiToken
    $script:lastCompletedRequestId = $newRequestId
}

Write-Host ""
Write-Host "--- Swap Reviews Module ---" -ForegroundColor Yellow

# Request #2 is seed and Completed (between lisi id=2 and wangwu id=3)
# Use lisi (id=2 token) to review
$r = Test-API "POST /swap-reviews (create lisi for req#2)" "POST" "/swap-reviews" @{
    swapRequestId = 2
    rating = 5
    content = "Great swap experience!"
} -Token $lisiToken
$newReviewId = $r.data.id

if ($newReviewId) {
    Test-API "GET /swap-reviews/$newReviewId" "GET" "/swap-reviews/$newReviewId" -Token $zhangsanToken
    Test-API "PUT /swap-reviews/$newReviewId (update)" "PUT" "/swap-reviews/$newReviewId" @{rating = 4; content = "Updated review"} -Token $zhangsanToken
    Test-API "DELETE /swap-reviews/$newReviewId (delete)" "DELETE" "/swap-reviews/$newReviewId" -Token $zhangsanToken
}

Test-API "GET /swap-reviews (list)" "GET" "/swap-reviews" -Token $zhangsanToken
Test-API "GET /swap-reviews?revieweeId=3" "GET" "/swap-reviews?revieweeId=3" -Token $zhangsanToken

Write-Host ""
Write-Host "--- Project Showcases Module ---" -ForegroundColor Yellow

$r = Test-API "GET /project-showcases (list)" "GET" "/project-showcases"

Test-API "GET /project-showcases/1" "GET" "/project-showcases/1"

Test-API "GET /project-showcases/mine (zhangsan)" "GET" "/project-showcases/mine" -Token $zhangsanToken

$r = Test-API "POST /project-showcases (create)" "POST" "/project-showcases" @{
    title = "Test Bracelet"
    description = "Handmade bracelet with beads"
    category = "Accessories"
    imageUrls = @("https://example.com/b1.jpg")
    tags = @("beads", "handmade")
} -Token $zhangsanToken
$newShowcaseId = $r.data.id

if ($newShowcaseId) {
    Test-API "PUT /project-showcases/$newShowcaseId (update)" "PUT" "/project-showcases/$newShowcaseId" @{title = "Test Bracelet V2"} -Token $zhangsanToken
    Test-API "DELETE /project-showcases/$newShowcaseId (delete)" "DELETE" "/project-showcases/$newShowcaseId" -Token $zhangsanToken
}

Write-Host ""
Write-Host "--- Stats Module ---" -ForegroundColor Yellow

Test-API "GET /stats/overview (zhangsan)" "GET" "/stats/overview" -Token $zhangsanToken
Test-API "GET /stats/trend?days=7 (zhangsan)" "GET" "/stats/trend?days=7" -Token $zhangsanToken

Write-Host ""
Write-Host "--- Fluent Validation Tests ---" -ForegroundColor Yellow

Test-API "POST /auth/register (empty user -> 400)" "POST" "/auth/register" @{username=""; email="a@b.com"; password="Test@123"; confirmPassword="Test@123"} -ExpectCode 400
Test-API "POST /auth/register (bad email -> 400)" "POST" "/auth/register" @{username="abc"; email="not-email"; password="Test@123"; confirmPassword="Test@123"} -ExpectCode 400
Test-API "POST /auth/register (weak pwd -> 400)" "POST" "/auth/register" @{username="abc2"; email="a2@b.com"; password="123"; confirmPassword="123"} -ExpectCode 400
Test-API "POST /auth/register (pwd mismatch -> 400)" "POST" "/auth/register" @{username="abc3"; email="a3@b.com"; password="Test@123"; confirmPassword="Different@123"} -ExpectCode 400

Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Passed: $pass" -ForegroundColor Green
Write-Host "Failed: $fail" -ForegroundColor Red
$total = $pass + $fail
if ($total -gt 0) {
    Write-Host "Success rate: $([math]::Round(($pass/$total)*100, 1))%"
}

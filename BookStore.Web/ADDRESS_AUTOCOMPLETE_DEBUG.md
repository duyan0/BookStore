# Address Autocomplete Debug Guide - UPDATED

## 🚨 CRITICAL TESTING STEPS

### **IMMEDIATE TESTING PROCEDURE:**

1. **Mở trang checkout**: http://localhost:5106/Shop/Checkout
2. **Mở Developer Tools**: Nhấn F12
3. **Vào tab Console**
4. **Chạy lệnh test**: `showTestSuggestions()`
5. **Kiểm tra dropdown có xuất hiện không**

### **MANUAL TESTING COMMANDS:**

```javascript
// Test 1: Force show test suggestions
showTestSuggestions()

// Test 2: Test real API
testAddressAutocomplete()

// Test 3: Check elements
console.log('Input:', document.getElementById('shippingAddressInput'));
console.log('Suggestions:', document.getElementById('addressSuggestions'));

// Test 4: Manual API call
fetch('https://rsapi.goong.io/Place/AutoComplete?api_key=UcgP2TVpMRCoR1V7iLaL6yHuPhtTYDfFxlkSVChD&input=95%20Nguyen&location=10.8231,106.6297&radius=50000')
  .then(r => r.json())
  .then(d => console.log('API Response:', d));
```

## Overview
This guide helps debug and test the address autocomplete functionality in the BookStore checkout page.

## Issues Fixed

### ✅ **1. Critical API Key Validation Bug**
**Problem**: The searchAddresses function had incorrect API key validation logic:
```javascript
// WRONG - This would always return early!
if (!GOONG_API_KEY || GOONG_API_KEY === 'UcgP2TVpMRCoR1V7iLaL6yHuPhtTYDfFxlkSVChD') {
```

**Fix**: Changed to proper validation:
```javascript
// CORRECT - Only return early if key is not configured
if (!GOONG_API_KEY || GOONG_API_KEY === 'YOUR_GOONG_API_KEY_HERE') {
```

### ✅ **2. Removed Duplicate Functions**
**Problem**: Multiple duplicate functions were causing conflicts
**Fix**: Removed all duplicate searchAddresses, displayAddressSuggestions, and hideAddressSuggestions functions

### ✅ **3. Enhanced Debugging**
**Added**: Comprehensive console logging throughout the autocomplete flow
**Added**: Error handling for missing DOM elements
**Added**: Test function accessible from browser console

### ✅ **4. Improved CSS Styling**
**Enhanced**: Better positioning and visibility for suggestion dropdown
**Added**: Debug styles for troubleshooting
**Fixed**: Z-index and positioning issues

## Testing Instructions

### **Step 1: Open Checkout Page**
Navigate to: http://localhost:5106/Shop/Checkout

### **Step 2: Open Browser Developer Tools**
- Press F12 or right-click → Inspect
- Go to Console tab

### **Step 3: Check Initialization**
Look for these console messages:
```
Initializing address autocomplete...
Address input found: true
Address suggestions container found: true
Address autocomplete initialized successfully
```

### **Step 4: Test API Status**
Check the badge next to "Địa chỉ giao hàng":
- 🟢 Green "API sẵn sàng" = Working
- 🟡 Yellow "API không khả dụng" = Connection issue
- 🔴 Red "Chưa cấu hình API" = Configuration issue

### **Step 5: Test Address Input**
1. Click in the address textarea
2. Type at least 3 characters (e.g., "95 Nguyen")
3. Watch console for:
   ```
   Input changed, query: 95 Nguyen, length: 9
   Starting search with debounce...
   Searching addresses for: 95 Nguyen
   API Response status: 200
   API Response data: {status: "OK", predictions: [...]}
   Displaying address suggestions: [...]
   Address suggestions displayed, container visible: true
   ```

### **Step 6: Verify Dropdown Appearance**
- Suggestions should appear below the textarea
- Each suggestion should have an address icon and text
- Hovering should highlight suggestions

### **Step 7: Test Selection**
1. Click on any suggestion
2. Watch console for:
   ```
   Address suggestion clicked: [place_id]
   ```
3. Address should fill in the textarea
4. Success notification should appear

## Manual Testing from Console

### **Test Function**
Run this in browser console:
```javascript
testAddressAutocomplete()
```

### **Manual API Test**
```javascript
// Test API directly
fetch(`https://rsapi.goong.io/Place/AutoComplete?api_key=UcgP2TVpMRCoR1V7iLaL6yHuPhtTYDfFxlkSVChD&input=95%20Nguyen&location=10.8231,106.6297&radius=50000`)
  .then(r => r.json())
  .then(d => console.log('Direct API test:', d));
```

### **Check Elements**
```javascript
// Verify DOM elements exist
console.log('Address input:', document.getElementById('shippingAddressInput'));
console.log('Suggestions container:', document.getElementById('addressSuggestions'));
```

## Common Issues & Solutions

### **Issue 1: No suggestions appearing**
**Check**: Console for API errors
**Solution**: Verify API key and internet connection

### **Issue 2: Suggestions appear but can't click**
**Check**: CSS z-index and positioning
**Solution**: Verify suggestion items have click handlers

### **Issue 3: API key errors**
**Check**: Console for "API key not configured" messages
**Solution**: Verify GOONG_API_KEY is set correctly

### **Issue 4: Network errors**
**Check**: Browser Network tab for failed requests
**Solution**: Check CORS settings and API quotas

## Expected API Response Format

### **AutoComplete Response**
```json
{
  "status": "OK",
  "predictions": [
    {
      "place_id": "...",
      "structured_formatting": {
        "main_text": "95 Nguyễn Du",
        "secondary_text": "Quận 1, Thành phố Hồ Chí Minh"
      }
    }
  ]
}
```

### **Place Detail Response**
```json
{
  "status": "OK",
  "result": {
    "formatted_address": "95 Nguyễn Du, Quận 1, Thành phố Hồ Chí Minh",
    "geometry": {
      "location": {
        "lat": 10.7769,
        "lng": 106.7009
      }
    }
  }
}
```

## Debug CSS Classes

### **Temporary Debug Styles**
Add this to browser console to highlight the suggestions container:
```javascript
document.getElementById('addressSuggestions').classList.add('debug');
```

### **Check Visibility**
```javascript
const container = document.getElementById('addressSuggestions');
console.log('Container classes:', container.className);
console.log('Is hidden:', container.classList.contains('d-none'));
console.log('Computed style:', getComputedStyle(container).display);
```

## Performance Monitoring

### **API Call Timing**
Monitor Network tab for:
- Request timing
- Response size
- Rate limiting

### **Debounce Verification**
Type quickly and verify only one API call is made after 500ms delay

## Production Considerations

### **Remove Debug Code**
Before production, remove:
- Console.log statements
- Test functions
- Debug CSS classes

### **API Key Security**
- Move API key to server-side
- Implement rate limiting
- Monitor API usage

### **Error Handling**
- Graceful fallback for API failures
- User-friendly error messages
- Retry mechanisms

## Success Criteria

✅ **Initialization**: Console shows successful setup
✅ **API Status**: Green badge appears
✅ **Input Response**: Typing triggers search after 3+ characters
✅ **API Calls**: Network tab shows successful requests
✅ **Suggestions**: Dropdown appears with Vietnamese addresses
✅ **Selection**: Clicking fills address and hides dropdown
✅ **Notifications**: Success message appears on selection

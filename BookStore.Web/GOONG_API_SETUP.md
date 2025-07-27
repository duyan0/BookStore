# GOONG Maps API Configuration

## Overview
The BookStore checkout page uses GOONG Maps API for address autocomplete and interactive map functionality.

## API Keys Required

### 1. GOONG API Key (for Geocoding & Places)
- **Purpose**: Address autocomplete, reverse geocoding, place details
- **Current Key**: `cNNqPnwuPiecDAb0midTWtf8NJkrIXhh3qws63ns`
- **Services Used**:
  - Place AutoComplete API
  - Place Detail API  
  - Geocoding API (reverse geocoding)

### 2. GOONG Maptiles Key (for Map Display)
- **Purpose**: Interactive map tiles and display
- **Current Key**: `cpubDH6sLrBMA06ID8oxJtc9kdgBVKWwwvlqVmji1`
- **Services Used**:
  - Map tiles rendering
  - Interactive map controls

## Setup Instructions

### 1. Get GOONG API Keys
1. Visit [GOONG Console](https://console.goong.io/)
2. Create an account or sign in
3. Create a new project
4. Generate API keys for:
   - **REST API** (for geocoding and places)
   - **Map Tiles** (for map display)

### 2. Update Configuration
Update the API keys in `/Views/Shop/Checkout.cshtml`:

```javascript
const GOONG_API_KEY = 'your_goong_api_key_here';
const GOONG_MAPTILES_KEY = 'your_goong_maptiles_key_here';
```

### 3. Domain Restrictions (Recommended)
For security, restrict API keys to your domain:
- Development: `localhost:5106`
- Production: `yourdomain.com`

## Features Implemented

### Address Autocomplete
- Real-time address suggestions as user types
- Vietnam-specific address formatting
- Click to select from dropdown

### Interactive Map
- Click to select location on map
- Drag-and-drop pin functionality
- Current location detection
- Reverse geocoding to get address from coordinates

### Error Handling
- Graceful fallback when API is unavailable
- User-friendly error messages
- Loading states and indicators

## API Usage Limits
- Check your GOONG console for current usage limits
- Monitor API calls to avoid exceeding quotas
- Consider implementing caching for frequently accessed locations

## Troubleshooting

### Map Not Loading
1. Check API keys are correct
2. Verify domain restrictions
3. Check browser console for errors
4. Ensure internet connection

### Address Autocomplete Not Working
1. Verify GOONG_API_KEY is correct
2. Check API quota limits
3. Ensure proper CORS configuration

### Location Detection Issues
1. User must grant location permission
2. HTTPS required for geolocation in production
3. Check browser compatibility

## Security Notes
- Never expose API keys in client-side code in production
- Consider using environment variables
- Implement server-side proxy for API calls in production
- Monitor API usage regularly

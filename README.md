### Server Setup

1. Create the "Keys" directory:

   ```bash
   mkdir -p PlataCuOraApp/PlataCuOraApp.Server/Keys
   ```

2. Update the configuration files with your Firebase credentials

   - You should create two files here: firebaseApiKey.json and firebaseKey.json
   - firebaseApiKey should follow this structure:

   ```bash
   {
   "apiKey": "YOUR-API-KEY"
   }
   ```

   - and firebaseKey.json should follow this structure:

   ```bash
   {
   "type": "service_account",
   "project_id": "YOUR_PROJECT_ID",
   "private_key_id": "YOUR_PRIVATE_KEY_ID",
   "private_key": "YOUR_PRIVATE_KEY",
   "client_email": "YOUR_CLIENT_EMAIL",
   "client_id": "YOUR_CLIENT_ID",
   "auth_uri": "https://accounts.google.com/o/oauth2/auth",
   "token_uri": "https://oauth2.googleapis.com/token",
   "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
   "client_x509_cert_url": "YOUR_CLIENT_CERT_URL",
   "universe_domain": "googleapis.com"
   }

   ```

3. Run the server:
   ```bash
   cd PlataCuOraApp/PlataCuOraApp.Server
   dotnet restore
   dotnet build
   dotnet run
   ```

### Client Setup

1. Install dependencies:

   ```bash
   cd PlataCuOraApp/platacuoraapp.client
   npm install
   ```

2. Run the Angular application:
   ```bash
   ng serve
   ```

## Important Security Notes

- Never commit Firebase API keys or service account credentials to public repositories
- The .gitignore file is set up to prevent these sensitive files from being committed
- Only example files with placeholders should be included in the repository
>>>>>>> f203875b527f091002f1ed24a73d672c098249ea

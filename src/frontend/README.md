# Archie Frontend

A modern web interface for Archie - AI-powered repository documentation and analysis platform.

## 🚀 Features

- **Repository Management**: Connect, analyze, and manage repositories
- **AI Documentation**: View and interact with AI-generated documentation
- **Semantic Search**: Powerful search across repositories with visual results
- **Conversational AI**: Chat interface for repository exploration
- **Real-time Updates**: WebSocket subscriptions for live updates
- **Modern UI**: Built with Next.js 15, TypeScript, and Tailwind CSS
- **Enterprise Auth**: Secure Azure AD integration
- **Responsive Design**: Optimized for desktop, tablet, and mobile

## 🛠️ Technology Stack

### Core Framework
- **Next.js 15**: React framework with App Router
- **TypeScript**: Type-safe development
- **React 18**: Modern React with concurrent features

### UI & Styling
- **Tailwind CSS**: Utility-first CSS framework
- **shadcn/ui**: High-quality component library
- **Radix UI**: Accessible component primitives
- **Lucide Icons**: Beautiful SVG icons

### Data & API
- **Apollo Client**: GraphQL client with caching
- **GraphQL**: Type-safe API queries and mutations
- **WebSocket**: Real-time subscriptions

### Authentication
- **NextAuth.js**: Authentication framework
- **Azure AD**: Enterprise authentication provider

### Development & Testing
- **Jest**: Unit testing framework
- **Testing Library**: React testing utilities
- **Playwright**: End-to-end testing
- **ESLint**: Code linting
- **Prettier**: Code formatting

## 📁 Project Structure

```
src/
├── app/                    # Next.js App Router pages
│   ├── auth/              # Authentication pages
│   ├── dashboard/         # Dashboard and overview
│   ├── repositories/      # Repository management
│   ├── search/           # Search interface
│   ├── chat/             # Conversational interface
│   └── documentation/    # Documentation viewer
├── components/            # React components
│   ├── ui/               # Base UI components
│   ├── layout/           # Layout components
│   ├── navigation/       # Navigation components
│   ├── providers/        # Context providers
│   └── [feature]/        # Feature-specific components
├── lib/                   # Utilities and configuration
├── hooks/                 # Custom React hooks
├── types/                 # TypeScript type definitions
├── graphql/              # GraphQL queries and mutations
└── stores/               # State management
```

## 🏃‍♂️ Getting Started

### Prerequisites

- Node.js 18+ and npm 8+
- Access to Archie GraphQL API
- Azure AD application for authentication

### Installation

1. **Clone and navigate to frontend directory**:
   ```bash
   cd src/frontend
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Environment setup**:
   ```bash
   cp .env.example .env.local
   ```
   
   Update `.env.local` with your configuration:
   ```env
   NEXTAUTH_URL=http://localhost:3000
   NEXTAUTH_SECRET=your-secret-key
   AZURE_AD_CLIENT_ID=your-azure-client-id
   AZURE_AD_CLIENT_SECRET=your-azure-client-secret
   AZURE_AD_TENANT_ID=your-tenant-id
   NEXT_PUBLIC_GRAPHQL_ENDPOINT=https://localhost:7001/graphql
   NEXT_PUBLIC_GRAPHQL_WS_ENDPOINT=wss://localhost:7001/graphql
   ```

4. **Start development server**:
   ```bash
   npm run dev
   ```

5. **Open application**:
   Navigate to [http://localhost:3000](http://localhost:3000)

## 🧪 Testing

### Unit Tests
```bash
# Run all tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:ci
```

### End-to-End Tests
```bash
# Run E2E tests
npm run e2e

# Run E2E tests with UI
npm run e2e:ui
```

## 📋 Scripts

| Script | Description |
|--------|-------------|
| `dev` | Start development server |
| `build` | Build for production |
| `start` | Start production server |
| `lint` | Run ESLint |
| `test` | Run Jest tests |
| `test:watch` | Run Jest in watch mode |
| `test:ci` | Run Jest with coverage |
| `e2e` | Run Playwright E2E tests |
| `e2e:ui` | Run Playwright with UI |
| `type-check` | Run TypeScript compiler |
| `format` | Format code with Prettier |
| `format:check` | Check code formatting |

## 🎨 Component Library

The application uses shadcn/ui components for consistency and accessibility:

- **Navigation**: Sidebar, Header, Mobile Menu
- **Layout**: Cards, Dialogs, Sheets
- **Forms**: Input, Button, Select, Checkbox
- **Feedback**: Toast, Alert, Badge
- **Data Display**: Avatar, Skeleton, Tabs

## 🔒 Authentication Flow

1. **Sign In**: Users authenticate via Azure AD
2. **Token Management**: JWT tokens with automatic refresh
3. **Session**: Secure session management with NextAuth.js
4. **Authorization**: GraphQL requests include Bearer token
5. **Security**: CSRF protection and secure headers

## 📡 GraphQL Integration

### Queries
- Repository management and file structure
- Documentation retrieval and search
- Conversation history and messages
- Knowledge graph data

### Mutations  
- Repository connection and refresh
- Documentation generation
- Chat message sending
- User preferences

### Subscriptions
- Real-time chat updates
- Repository analysis progress
- Documentation generation status
- System notifications

## 🎯 Performance Optimization

### Core Web Vitals Targets
- **LCP**: <2.5 seconds
- **FID**: <100 milliseconds  
- **CLS**: <0.1
- **TTI**: <3.5 seconds

### Optimization Strategies
- Next.js automatic code splitting
- Image optimization with next/image
- Apollo Client caching and normalization
- Virtualized lists for large datasets
- Lazy loading of heavy components
- Service worker for offline functionality

## 🚀 Deployment

### Development
```bash
npm run build
npm run start
```

### Azure Static Web Apps
The application is configured for deployment to Azure Static Web Apps with:
- Automatic builds on push
- Environment-specific configuration
- CDN distribution
- Custom domain support

## 🛡️ Security

### Client-Side Security
- Content Security Policy headers
- HTTPS enforcement
- XSS protection
- Input validation and sanitization
- No sensitive data in client state

### Authentication Security  
- Azure AD SSO integration
- JWT token validation
- Session timeout handling
- CSRF protection
- Secure cookie configuration

## 📊 Monitoring

### Application Insights
- Real-time performance monitoring
- Error tracking and diagnostics
- User behavior analytics
- Core Web Vitals tracking

### Development Tools
- React Developer Tools
- Apollo Client DevTools
- Next.js built-in analytics
- Lighthouse performance auditing

## 🤝 Contributing

1. Follow TypeScript and React best practices
2. Write tests for new components and features
3. Use conventional commit messages
4. Ensure accessibility compliance (WCAG 2.1 AA)
5. Test across different browsers and devices
6. Update documentation for API changes

## 📈 Architecture Decisions

### State Management
- Apollo Client for server state
- React Context for UI state
- Local state for component-specific data

### Routing
- Next.js App Router for file-based routing
- Middleware for authentication checks
- Dynamic imports for code splitting

### Styling
- Tailwind CSS for utility classes
- CSS variables for theming
- Component composition over inheritance

## 🔧 Troubleshooting

### Common Issues

1. **Authentication Errors**:
   - Verify Azure AD configuration
   - Check NEXTAUTH_SECRET is set
   - Ensure redirect URLs match

2. **GraphQL Connection Issues**:
   - Verify API endpoint URLs
   - Check network connectivity
   - Validate authentication tokens

3. **Build Errors**:
   - Run `npm run type-check`
   - Check ESLint errors
   - Verify environment variables

### Debug Mode
Set `NODE_ENV=development` and check browser console for detailed error messages.

## 📄 License

This project is part of the Archie platform and follows the same licensing terms.
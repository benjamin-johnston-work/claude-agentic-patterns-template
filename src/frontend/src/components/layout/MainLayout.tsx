'use client';

import { ReactNode, useState } from 'react';
import { useSession } from 'next-auth/react';
import { Sidebar } from '@/components/navigation/Sidebar';
import { Header } from '@/components/navigation/Header';
import { MobileSidebar } from '@/components/navigation/MobileSidebar';
import { ErrorBoundary } from '@/components/layout/ErrorBoundary';
import { LoadingScreen } from '@/components/layout/LoadingScreen';

interface MainLayoutProps {
  children: ReactNode;
}

export function MainLayout({ children }: MainLayoutProps) {
  const { data: session, status } = useSession();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  // For local development testing, bypass authentication
  const isDevelopment = process.env.NODE_ENV === 'development';
  const mockUser = {
    id: 'dev-user',
    name: 'Development User',
    email: 'dev@localhost',
    image: null
  };

  if (status === 'loading' && !isDevelopment) {
    return <LoadingScreen />;
  }

  if (status === 'unauthenticated' && !isDevelopment) {
    // This should be handled by middleware, but as a fallback
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p>Please sign in to continue.</p>
      </div>
    );
  }

  return (
    <div className="h-screen flex overflow-hidden bg-background">
      {/* Mobile sidebar */}
      <MobileSidebar
        open={mobileMenuOpen}
        onClose={() => setMobileMenuOpen(false)}
        user={isDevelopment ? mockUser : session?.user}
      />

      {/* Desktop sidebar */}
      <div className="hidden lg:flex lg:flex-shrink-0">
        <div className="flex flex-col">
          <Sidebar
            open={sidebarOpen}
            user={isDevelopment ? mockUser : session?.user}
          />
        </div>
      </div>

      {/* Main content area */}
      <div className="flex-1 overflow-hidden flex flex-col min-w-0">
        <Header
          onMobileMenuOpen={() => setMobileMenuOpen(true)}
          onSidebarToggle={() => setSidebarOpen(!sidebarOpen)}
          user={isDevelopment ? mockUser : session?.user}
        />
        
        <main className="flex-1 overflow-auto focus:outline-none">
          <ErrorBoundary>
            <div className="py-6">
              <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                {children}
              </div>
            </div>
          </ErrorBoundary>
        </main>
      </div>
    </div>
  );
}
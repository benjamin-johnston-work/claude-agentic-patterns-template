import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { Providers } from '@/components/providers/Providers';
import { Toaster } from '@/components/ui/toaster';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'Archie - Repository Documentation & Analysis',
  description: 'AI-powered repository analysis, documentation generation, and conversational interface for code exploration.',
  keywords: ['repository', 'documentation', 'AI', 'code analysis', 'GraphQL'],
  authors: [{ name: 'Archie Team' }],
  creator: 'Archie Team',
  publisher: 'Archie',
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  metadataBase: new URL(process.env.NEXTAUTH_URL || 'http://localhost:3000'),
  openGraph: {
    title: 'Archie - Repository Documentation & Analysis',
    description: 'AI-powered repository analysis, documentation generation, and conversational interface for code exploration.',
    type: 'website',
    locale: 'en_US',
    siteName: 'Archie',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Archie - Repository Documentation & Analysis',
    description: 'AI-powered repository analysis, documentation generation, and conversational interface for code exploration.',
    creator: '@archie',
  },
  robots: {
    index: false, // Prevent search engine indexing for internal tool
    follow: false,
    googleBot: {
      index: false,
      follow: false,
    },
  },
  viewport: {
    width: 'device-width',
    initialScale: 1,
    maximumScale: 1,
    userScalable: false,
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="h-full">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="" />
        <meta name="theme-color" content="#ffffff" />
        <meta name="msapplication-TileColor" content="#ffffff" />
        <meta name="msapplication-config" content="/browserconfig.xml" />
        <link rel="manifest" href="/manifest.json" />
      </head>
      <body className={`${inter.className} h-full antialiased`}>
        <Providers>
          <div className="min-h-full">
            {children}
          </div>
          <Toaster />
        </Providers>
      </body>
    </html>
  );
}
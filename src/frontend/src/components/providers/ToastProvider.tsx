'use client';

import { ReactNode } from 'react';
import { Toaster } from 'react-hot-toast';
import { useTheme } from 'next-themes';

interface ToastProviderProps {
  children: ReactNode;
}

export function ToastProvider({ children }: ToastProviderProps) {
  const { theme } = useTheme();

  return (
    <>
      {children}
      <Toaster
        position="top-right"
        reverseOrder={false}
        gutter={8}
        containerClassName=""
        containerStyle={{}}
        toastOptions={{
          // Define default options
          className: '',
          duration: 4000,
          style: {
            background: theme === 'dark' ? '#363636' : '#fff',
            color: theme === 'dark' ? '#fff' : '#363636',
            borderRadius: '8px',
            border: `1px solid ${theme === 'dark' ? '#404040' : '#e5e7eb'}`,
          },

          // Default options for specific types
          success: {
            duration: 3000,
            iconTheme: {
              primary: '#10b981',
              secondary: '#fff',
            },
          },
          error: {
            duration: 5000,
            iconTheme: {
              primary: '#ef4444',
              secondary: '#fff',
            },
          },
          loading: {
            duration: Infinity,
          },
        }}
      />
    </>
  );
}
'use client';

import { Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface LoadingScreenProps {
  message?: string;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}

export function LoadingScreen({ 
  message = 'Loading...', 
  className,
  size = 'md' 
}: LoadingScreenProps) {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
  };

  return (
    <div className={cn(
      'min-h-screen flex flex-col items-center justify-center bg-background',
      className
    )}>
      <div className="flex flex-col items-center space-y-4">
        {/* Logo or brand icon */}
        <div className="w-12 h-12 bg-primary rounded-lg flex items-center justify-center">
          <svg
            className="w-6 h-6 text-primary-foreground"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
        </div>

        {/* Loading spinner */}
        <div className="flex items-center space-x-3">
          <Loader2 className={cn('animate-spin text-primary', sizeClasses[size])} />
          <span className="text-muted-foreground text-sm font-medium">
            {message}
          </span>
        </div>

        {/* Progress indicator dots */}
        <div className="flex space-x-1">
          <div className="w-2 h-2 bg-primary rounded-full animate-pulse"></div>
          <div className="w-2 h-2 bg-primary/60 rounded-full animate-pulse delay-75"></div>
          <div className="w-2 h-2 bg-primary/30 rounded-full animate-pulse delay-150"></div>
        </div>
      </div>
    </div>
  );
}

// Inline loading component for smaller sections
export function InlineLoading({ 
  message = 'Loading...', 
  className,
  size = 'sm' 
}: LoadingScreenProps) {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
  };

  return (
    <div className={cn('flex items-center justify-center p-8', className)}>
      <div className="flex items-center space-x-3">
        <Loader2 className={cn('animate-spin text-muted-foreground', sizeClasses[size])} />
        <span className="text-muted-foreground text-sm">
          {message}
        </span>
      </div>
    </div>
  );
}

// Skeleton loading component
export function SkeletonLoader({ className }: { className?: string }) {
  return (
    <div className={cn('animate-pulse', className)}>
      <div className="space-y-3">
        <div className="h-4 bg-muted rounded w-3/4"></div>
        <div className="h-4 bg-muted rounded w-1/2"></div>
        <div className="h-4 bg-muted rounded w-full"></div>
        <div className="h-4 bg-muted rounded w-2/3"></div>
      </div>
    </div>
  );
}